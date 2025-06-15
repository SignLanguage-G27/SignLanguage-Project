using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using SignLanguage.APIs.DTOs;
using SignLanguage.Core.Entities;
using SignLanguage.Core.Entities.Identity;
using SignLanguage.Core.Service.Contract;
using SignLanguage.Infrastracture.Data.Identity;
using System.Text.Json.Serialization;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SignLanguage.APIs.Controllers
{
    public class SignPredictionController : BaseApiController
    {
        private readonly IAttachmentService _attachmentService;
        private readonly HttpClient _httpClient;
        private readonly UserManager<AppUser> _userManager;
        private readonly AppIdentityDbContext _context;

        public SignPredictionController(IAttachmentService attachmentService,HttpClient httpClient, UserManager<AppUser> userManager, AppIdentityDbContext identityContext)
        {
            _attachmentService = attachmentService;
            _httpClient = httpClient;
            _userManager=userManager;
            _context=identityContext;
        }

        //[HttpPost("predict")]
        //public async Task<ActionResult> Predict(IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //        return BadRequest("Invalid file.");

        //    // رفع الملف إلى السيرفر
        //    string? filePath = _attachmentService.Upload(file, "Uploads");
        //    if (string.IsNullOrEmpty(filePath))
        //        return StatusCode(500, "File upload failed.");

        //    // إرسال الملف إلى FastAPI عبر HTTP
        //    var response = await SendToPythonApi(filePath);
        //    if (string.IsNullOrEmpty(response))
        //        return StatusCode(500, "Error calling the Python API.");

        //    // إرسال الاستجابة من FastAPI مرة أخرى إلى العميل
        //    return Ok(response);
        //}
        [HttpPost("predict")]
        public async Task<ActionResult> Predict(IFormFile file, [FromQuery] string email)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file.");

            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is required.");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found.");

            var userId = user.Id;

            // رفع الملف
            string? filePath = _attachmentService.Upload(file, "Uploads");
            if (string.IsNullOrEmpty(filePath))
                return StatusCode(500, "File upload failed.");

            // إرسال الملف لـ Python API
            var response = await SendToPythonApi(filePath);
            if (string.IsNullOrEmpty(response))
                return StatusCode(500, "Error calling the Python API.");

            // تحليل الرد
            var prediction = System.Text.Json.JsonSerializer.Deserialize<PredictionResponse>(response);
            if (prediction == null)
                return StatusCode(500, "Invalid response from Python API.");

            // تحويل التوقيت لتوقيت مصر
            DateTime egyptTime;
            try
            {
                TimeZoneInfo egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                egyptTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);
            }
            catch
            {
                try
                {
                    TimeZoneInfo egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
                    egyptTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);
                }
                catch
                {
                    egyptTime = DateTime.UtcNow;
                }
            }

            // حفظ اللوج
            var fileName = Path.GetFileName(filePath);
            var log = new PredictionLog
            {
                ImagePath = fileName,
                Result = prediction.PredictedLabel,
                Confidence = prediction.Confidence,
                PredictTime = egyptTime,
                UserId = userId
            };

            // إعادة تعيين الـ Identity بعد الحفظ
            var hasAny = await _context.Logs.AnyAsync();
            if (hasAny)
            {
                var maxId = await _context.Logs.OrderByDescending(l => l.Id).Select(l => l.Id).FirstOrDefaultAsync();
                if (maxId > 0)
                {
                    var sql = $"DBCC CHECKIDENT ('Logs', RESEED, {maxId})";
                    await _context.Database.ExecuteSqlRawAsync(sql);
                }
            }
            else
            {
                var sql = "DBCC CHECKIDENT ('Logs', RESEED, 0)";
                await _context.Database.ExecuteSqlRawAsync(sql);
            }


            _context.Logs.Add(log);
            await _context.SaveChangesAsync();


            return Ok(prediction);
        }



        [HttpGet("userPredictions")]
        public async Task<ActionResult> GetUserPredictions([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new
                {
                    success = false,
                    error = "Email is required."
                });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    error = "User not found."
                });
            }

            var userId = user.Id;

            var logs = await _context.Logs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.PredictTime)
                .Select(l => new PredictionLogDto
                {
                    ImagePath = l.ImagePath,
                    Result = l.Result,
                    Confidence = l.Confidence,
                    PredictTime = l.PredictTime
                })
                .ToListAsync();

            if (logs == null || logs.Count == 0)
            {
                await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Logs', RESEED, 0);");

                return Ok(new
                {
                    success = true,
                    message = "No predictions found for this user.",
                    data = new List<PredictionLogDto>()
                });
            }

            return Ok(new
            {
                success = true,
                data = logs
            });
        }
        private async Task<string> SendToPythonApi(string filePath)
        {
            try
            {
                using var form = new MultipartFormDataContent();
                using var fileStream = new FileStream(filePath, FileMode.Open);
                using var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                form.Add(streamContent, "file", Path.GetFileName(filePath));

                // إرسال الطلب إلى FastAPI
                var response = await _httpClient.PostAsync("https://0fc6-196-130-219-111.ngrok-free.app/predict/", form);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return $"Error: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
