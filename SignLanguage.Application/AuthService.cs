using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SignLanguage.Core;
using SignLanguage.Core.Entities.Identity;
using SignLanguage.Core.Service.Contract;
using SignLanguage.Core.TelegramHelpers;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SignLanguage.Application
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IRedisService _redisService;
        private readonly ILogger<AuthService> _logger;
        private readonly ITelegramService _telegramService;
        private readonly HttpClient _httpClient;
        private readonly string _botToken;

        public AuthService(IConfiguration configuration, UserManager<AppUser> userManager, IEmailService emailService, IRedisService redisService, ILogger<AuthService> logger,ITelegramService telegramService,HttpClient httpClient)
        {
            _configuration = configuration;
            _userManager=userManager;
            _emailService=emailService;
            _redisService=redisService;
            _logger=logger;
            _telegramService=telegramService;
            _httpClient=httpClient;
            _botToken = configuration["TelegramBot:BotToken"];
        }
        public async Task<string> CreateTokenAsync(AppUser user, UserManager<AppUser> userManager)
        {
            var authClaims = new List<Claim>()
            {
            new Claim (ClaimTypes.GivenName, user.UserName),
            new Claim(ClaimTypes.Email, user.Email)
            };

            var userRoles = await userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
                authClaims.Add(new Claim(ClaimTypes.Role, role));

            var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));

            var token = new JwtSecurityToken(
                audience: _configuration["JWT:ValidAudience"],
                issuer: _configuration["JWT:ValidIssuer"],
                expires: DateTime.UtcNow.AddDays(double.Parse(_configuration["JWT:DurationInDays"])),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256Signature)

                );
            return new JwtSecurityTokenHandler().WriteToken(token);

        }
        public async Task SendWelcomeEmailAfterRegistration(string email)
        {
            string userName = email.Split('@')[0]; // لو عندك الاسم الحقيقي بداله استخدمه هنا

            string body = $@"
<html>
<body style='font-family: Arial, sans-serif; text-align: center; background-color: #f9f9f9; padding: 15px;'>
    <div style='max-width: 480px; margin: auto; background: white; padding: 20px; border-radius: 8px; 
                box-shadow: 0px 0px 8px rgba(0, 0, 0, 0.1);'>
        <h2 style='color: #4CAF50; margin-bottom: 10px;'>🎉 Welcome o Our SLR Platform!</h2>
        <p style='color: #333; font-size: 14px; margin: 5px 0;'>Dear <strong>{userName}</strong>,</p>
        <p style='color: #555; font-size: 14px; margin: 5px 0;'>Thank you for registering with the Sign Language Recognition system.</p>

        <p style='color: #2E7D32; font-size: 16px; font-weight: bold; margin: 10px 0;'>
            We're excited to have you on board!
        </p>

        <p style='color: #555; font-size: 13px; margin: 5px 0;'>
            You can now upload sign language images and get instant AI-powered predictions.
        </p>

        <hr style='margin: 15px 0; border: 0.5px solid #ddd;'>

        <p style='color: #777; font-size: 12px; margin: 5px 0;'>If you have any questions or need support, feel free to reach out.</p>
        <p style='font-weight: bold; color: #333; font-size: 13px;'>SLR Support Team</p>

        <p style='color: #999; font-size: 11px; margin-top: 10px;'>&copy; 2025 Sign Language Recognition G27</p>
    </div>
</body>
</html>";

            string subject = "Welcome To Our SLR Platform!";

            try
            {
                await _emailService.SendEmailAsync(email, subject, body);
            }
            catch
            {
              
            }
        }

        public async Task SendLoginNotificationEmailAsync(string email, string ipAddress, string userAgent)
        {
            string userName = email.Split('@')[0];

            // استخدم UAParser لاستخراج معلومات الجهاز
            var uaParser = UAParser.Parser.GetDefault();
            var clientInfo = uaParser.Parse(userAgent);
            string device = clientInfo.Device.Family ?? "Unknown";
            string os = clientInfo.OS.ToString();
            string browser = clientInfo.UA.ToString();

            // استخدم ipwho.is لتحديد الموقع
            string location = "Unknown";
            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetStringAsync($"https://ipwho.is/{ipAddress}");
                var ipInfo = JsonSerializer.Deserialize<IpWhoResponse>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (ipInfo != null && ipInfo.Success)
                {
                    location = $"{ipInfo.City}, {ipInfo.Region}, {ipInfo.Country}";
                }
            }
            catch
            {
                // تجاهل الخطأ وخلي اللوكيشن Unknown
            }

            string body = $@"
<html>
<body style='font-family: Arial, sans-serif; text-align: center; background-color: #f9f9f9; padding: 15px;'>
    <div style='max-width: 480px; margin: auto; background: white; padding: 20px; border-radius: 8px; 
                box-shadow: 0px 0px 8px rgba(0, 0, 0, 0.1);'>
        <h2 style='color: #4CAF50;'>👋 Welcome back, {userName}!</h2>
        <p style='color: #555;'>A new login to your SLR account was detected.</p>

        <table style='margin: auto; text-align: left; font-size: 14px; margin-top: 10px;'>
            <tr><td><strong>📍 Location:</strong></td><td>{location}</td></tr>
            <tr><td><strong>💻 Device:</strong></td><td>{device}</td></tr>
            <tr><td><strong>🧭 Browser:</strong></td><td>{browser}</td></tr>
            <tr><td><strong>🌐 IP Address:</strong></td><td>{ipAddress}</td></tr>
            <tr><td><strong>🕒 Time:</strong></td><td>{DateTime.Now:f}</td></tr>
        </table>

        <p style='margin-top: 20px; color: #D32F2F; font-size: 13px;'>If this wasn't you, we recommend changing your password immediately.</p>
        <hr style='margin: 15px 0; border: 0.5px solid #ddd;'>
        <p style='color: #777; font-size: 12px;'>SLR Support Team</p>
    </div>
</body>
</html>";

            string subject = "New Login Detected - SLR Account";
            await _emailService.SendEmailAsync(email, subject, body);
        }



        public async Task<ApiResponseForForgetPass> ForgetPasswordByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning($"ForgetPasswordByEmailAsync: User not found with email {email}");
                return new ApiResponseForForgetPass { Success = false, Message = "Email address is not registered.", ExpiresIn = 0 };
            }

            var existingOtp = await _redisService.GetDataAsync($"reset_code:{email}");
            if (!string.IsNullOrEmpty(existingOtp))
            {
                var ttl = await _redisService.GetTimeToLiveAsync($"reset_code:{email}");
                if (ttl.HasValue)
                {
                    _logger.LogInformation($"ForgetPasswordByEmailAsync: OTP already exists for {email}, {ttl.Value.TotalSeconds} seconds remaining.");
                    return new ApiResponseForForgetPass { Success = false, Message = "A reset code has already been sent.", ExpiresIn = (int)ttl.Value.TotalSeconds };
                }
            }

            // Generate reset code (6-digit random number)
            var resetCode = GenerateToken();

            // Store code in Redis for 3 minutes
            await _redisService.SetDataAsync($"reset_code:{email}", resetCode, TimeSpan.FromMinutes(3));

            // Send email
            string userName = email.Split('@')[0];
            string body = $@"
<html>
<body style='font-family: Arial, sans-serif; text-align: center; background-color: #f9f9f9; padding: 15px;'>
    <div style='max-width: 480px; margin: auto; background: white; padding: 20px; border-radius: 8px; 
                box-shadow: 0px 0px 8px rgba(0, 0, 0, 0.1);'>
        <h2 style='color: #4CAF50; margin-bottom: 10px;'>🔒 Password Reset Code</h2>
        <p style='color: #333; font-size: 14px; margin: 5px 0;'>Dear <strong>{userName}</strong>,</p>
        <p style='color: #555; font-size: 14px; margin: 5px 0;'>We received a request to reset your password.</p>
        
        <p style='color: #D32F2F; font-size: 16px; font-weight: bold; margin: 10px 0;'>
            ⚠️ This code is valid for <span style='color: #000;'>3 minutes</span> only and can be used <span style='color: #000;'>once</span>!
        </p>
        
        <h3 style='background: #f4f4f4; padding: 12px; border-radius: 6px; display: inline-block; 
                   font-size: 22px; font-weight: bold; margin: 10px 0; color: #2E7D32;'>{resetCode}</h3>
        
        <p style='color: #555; font-size: 13px; margin: 5px 0;'>If you did not request this, please ignore this email.</p>
        <hr style='margin: 15px 0; border: 0.5px solid #ddd;'>
        
        <p style='color: #777; font-size: 12px; margin: 5px 0;'>Best regards,</p>
        <p style='font-weight: bold; color: #333; font-size: 13px;'>SLR Support Team</p>
        
        <p style='color: #999; font-size: 11px; margin-top: 10px;'>&copy; 2025 Sign Language Recognition G27</p>
    </div>
</body>
</html>";

            string subject = "Password Reset Code";
            await _emailService.SendEmailAsync(email, subject, body);

            _logger.LogInformation($"ForgetPasswordByEmailAsync: Reset code sent to {email}");

            return new ApiResponseForForgetPass { Success = true, Message = "Success", ExpiresIn = 180 };
        }

        public async Task<ApiResponseForForgetPass> ForgetPasswordByTelegramAsync(string phoneNumber)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            if (user == null)
            {
                _logger.LogWarning($"ForgetPasswordByTelegramAsync: User not found with phone {phoneNumber}");
                return new ApiResponseForForgetPass { Success = false, Message = "Phone number is not registered.", ExpiresIn = 0 };
            }

            // Check if an OTP already exists in Redis
            var existingOtp = await _redisService.GetDataAsync($"otp:{phoneNumber}");
            if (!string.IsNullOrEmpty(existingOtp))
            {
                var ttl = await _redisService.GetTimeToLiveAsync($"otp:{phoneNumber}");
                if (ttl.HasValue)
                {
                    _logger.LogInformation($"ForgetPasswordByTelegramAsync: OTP already exists for {phoneNumber}, {ttl.Value.TotalSeconds} seconds remaining.");
                    return new ApiResponseForForgetPass { Success = false, Message = "A reset code has already been sent. Please wait before requesting a new one.", ExpiresIn = (int)ttl.Value.TotalSeconds };
                }
            }

            var otpCode = GenerateToken();
            await _redisService.SetDataAsync($"otp:{phoneNumber}", otpCode, TimeSpan.FromMinutes(3));

            phoneNumber = NormalizeTelegramPhoneNumber(phoneNumber);

            // Retrieve chat ID from Redis
            var chatId = await _redisService.GetDataAsync(phoneNumber);
            if (string.IsNullOrEmpty(chatId))
            {
                chatId = await GetChatIdByPhoneNumberAsync(phoneNumber);
                if (!string.IsNullOrEmpty(chatId))
                {
                    await _redisService.SetDataAsync(phoneNumber, chatId, TimeSpan.FromDays(30));
                }
                else
                {
                    _logger.LogWarning($"ForgetPasswordByTelegramAsync: Unable to retrieve Telegram chat ID for phone {phoneNumber}");
                    return new ApiResponseForForgetPass { Success = false, Message = "Could not send reset code via Telegram. Please make sure you have contacted the bot first.", ExpiresIn = 0 };
                }
            }

            string message = $"Your password reset code is: {otpCode}. It is valid for 3 minutes.";
            _logger.LogInformation("Sending OTP to Telegram Chat ID: {ChatId}", chatId);
            await _telegramService.SendMessageAsync(chatId, message, phoneNumber);

            _logger.LogInformation($"ForgetPasswordByTelegramAsync: OTP sent successfully via Telegram to {phoneNumber}");

            return new ApiResponseForForgetPass { Success = true, Message = "Success", ExpiresIn = 180 };
        }

        public async Task<string?> GetChatIdByPhoneNumberAsync(string phoneNumber)
        {
            phoneNumber=NormalizeTelegramPhoneNumber(phoneNumber);
            try
            {
                var url = $"https://api.telegram.org/bot{_botToken}/getUpdates";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch Telegram updates. Status Code: {StatusCode}", response.StatusCode);
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var updates = JsonSerializer.Deserialize<TelegramUpdateResponse>(jsonResponse);

                if (updates?.Result == null || !updates.Result.Any())
                {
                    _logger.LogWarning(" No updates found in Telegram.");
                    return null;
                }

                _logger.LogInformation("Telegram Updates: {Updates}", jsonResponse);

                // جلب الـ Chat ID بناءً على رقم الهاتف أو أي رسالة
                var chat = updates.Result
                    .OrderByDescending(u => u.UpdateId) // اختيار آخر تحديث
                    .FirstOrDefault(u =>
                        (u.Message?.Contact?.PhoneNumber == phoneNumber) ||  // البحث عن جهة الاتصال
                        (u.Message != null && u.Message.Chat?.Id != null)    // البحث عن أي رسالة أخرى
                    )?.Message?.Chat;

                if (chat?.Id != null)
                {
                    _logger.LogInformation("Found Chat ID: {ChatId} for Phone Number: {PhoneNumber}", chat.Id, phoneNumber);
                    return chat.Id.ToString();
                }

                _logger.LogWarning("No chat ID found for phone number: {PhoneNumber}", phoneNumber);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching chat ID for phone number: {PhoneNumber}", phoneNumber);
                return null;
            }
        }

        public async Task<ApiResponseForForgetPass> VerifyResetCodeAsync(string identifier, string resetCode, bool isEmail)
        {
            var user = isEmail
                ? await _userManager.FindByEmailAsync(identifier)
                : await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == identifier);

            if (user == null)
            {
                _logger.LogWarning($"VerifyResetCodeAsync: User not found with {(isEmail ? "email" : "phone")} {identifier}");
                return new ApiResponseForForgetPass { Success = false, Message = isEmail ? "Email address is not registered." : "Phone number is not registered." };
            }

            string redisKey = isEmail ? $"reset_code:{identifier}" : $"otp:{identifier}";
            var storedCode = await _redisService.GetDataAsync(redisKey);

            if (storedCode == null)
            {
                _logger.LogWarning($"VerifyResetCodeAsync: No reset code found for {identifier}");
                return new ApiResponseForForgetPass { Success = false, Message = "No reset code found. Please request a new one." };
            }

            if (storedCode != resetCode)
            {
                _logger.LogWarning($"VerifyResetCodeAsync: Invalid reset code for {identifier}");
                return new ApiResponseForForgetPass { Success = false, Message = "Invalid reset code. Please check and try again." };
            }

            _logger.LogInformation($"VerifyResetCodeAsync: Reset code verified successfully for {identifier}");
            return new ApiResponseForForgetPass { Success = true, Message = "Success" };
        }

        public async Task<ApiResponseForForgetPass> ResetPasswordAsync(string identifier, string newPassword, bool isEmail)
        {
            var user = isEmail
                ? await _userManager.FindByEmailAsync(identifier)
                : await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == identifier);

            if (user == null)
            {
                _logger.LogWarning($"ResetPasswordAsync: User not found with {(isEmail ? "email" : "phone")} {identifier}");
                return new ApiResponseForForgetPass { Success = false, Message = isEmail ? "Email address is not registered." : "Phone number is not registered." };
            }

            string redisKey = isEmail ? $"reset_code:{identifier}" : $"otp:{identifier}";
            var storedCode = await _redisService.GetDataAsync(redisKey);

            if (storedCode == null)
            {
                _logger.LogWarning($"ResetPasswordAsync: No reset code found for {identifier}");
                return new ApiResponseForForgetPass { Success = false, Message = "No reset code found. Please request a new one." };
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError($"ResetPasswordAsync: Failed for {identifier} - {errors}");
                return new ApiResponseForForgetPass { Success = false, Message = $"Password reset failed: {errors}" };
            }

            // Delete code from Redis
            await _redisService.DeleteDataAsync(redisKey);

            _logger.LogInformation($"ResetPasswordAsync: Password reset successful for {identifier}");
            return new ApiResponseForForgetPass { Success = true, Message = "Success" };
        }











        //public async Task PrintAllUserPhonesAsync()
        //{
        //    var url = $"https://api.telegram.org/bot{_botToken}/getUpdates";
        //    var response = await _httpClient.GetAsync(url);
        //    var content = await response.Content.ReadAsStringAsync();

        //    _logger.LogInformation("📥 Raw Telegram Updates: {Updates}", content);

        //    var updates = JsonSerializer.Deserialize<TelegramUpdates>(content);
        //    if (updates?.Result == null || !updates.Result.Any())
        //    {
        //        _logger.LogWarning("❌ No updates found!");
        //        return;
        //    }

        //    var phoneNumbers = updates.Result
        //        .Where(u => u.Message?.Contact != null)
        //        .Select(u => u.Message.Contact.PhoneNumber)
        //        .Distinct()
        //        .ToList();

        //    if (phoneNumbers.Any())
        //    {
        //        _logger.LogInformation("📞 Found Phone Numbers:");
        //        foreach (var phone in phoneNumbers)
        //        {
        //            _logger.LogInformation($"➡️ {phone}");
        //        }
        //    }
        //    else
        //    {
        //        _logger.LogWarning("❌ No phone numbers found!");
        //    }
        //}
        private string GenerateToken()
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] tokenBytes = new byte[4]; // 4 بايت كفاية لأرقام عشوائية
            rng.GetBytes(tokenBytes);
            var otp = BitConverter.ToUInt32(tokenBytes, 0) % 1000000; // تحويله إلى رقم مكون من 6 أرقام
            return otp.ToString("D6"); // تنسيق لضمان 6 أرقام
        }

        private string NormalizeTelegramPhoneNumber(string phoneNumber)
        {
            phoneNumber = phoneNumber switch
            {
                string p when p.StartsWith("+200") => "20" + p.Substring(4),
                string p when p.StartsWith("+20") => p.Substring(1),
                string p when p.StartsWith("0") => "2" +p,
                _ => phoneNumber
            };

            return phoneNumber;
        }
    }
}
