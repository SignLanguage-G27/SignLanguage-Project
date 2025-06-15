using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignLanguage.APIs.DTOs;
using SignLanguage.Core.Entities.Identity;
using SignLanguage.Core.Service.Contract;

namespace SignLanguage.APIs.Controllers
{
    public class ProfileController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public ProfileController
            (
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IAuthService authService,
            ILogger<AccountController> logger
            )
        {
            _userManager=userManager;
            _signInManager=signInManager;
            _authService = authService;
            _logger=logger;
        }

        [HttpGet("getUserProfile")]
        public async Task<IActionResult> GetProfile([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is required.");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found.");

            var profile = new UserProfileDto
            {
                DisplayName = user.DisplayName,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return Ok(profile);
        }

        [HttpPut("updateUserProfile")]
        public async Task<IActionResult> UpdateProfile([FromQuery] string email, [FromBody] UpdateProfileDto model)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is required.");


            if (string.IsNullOrEmpty(model.DisplayName) &&
                string.IsNullOrEmpty(model.UserName) &&
                string.IsNullOrEmpty(model.Email) &&
                string.IsNullOrEmpty(model.PhoneNumber))
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Update failed",
                    Detail = "No update data provided."
                });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found.");

            // تحقق من تكرار الإيميل لمستخدم آخر
            if (!string.IsNullOrEmpty(model.Email))
            {
                var emailExists = await _userManager.Users
                    .AnyAsync(u => u.Email == model.Email && u.Id != user.Id);

                if (emailExists)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Update failed",
                        Detail = "Email is already in use by another user."
                    });
                }

                user.Email = model.Email;
                user.NormalizedEmail = _userManager.NormalizeEmail(model.Email);
            }

            // تحقق من تكرار رقم الهاتف لمستخدم آخر
            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                var phoneExists = await _userManager.Users
                    .AnyAsync(u => u.PhoneNumber == model.PhoneNumber && u.Id != user.Id);

                if (phoneExists)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Update failed",
                        Detail = "Phone number is already in use by another user."
                    });
                }

                user.PhoneNumber = model.PhoneNumber;
            }

            if (!string.IsNullOrEmpty(model.DisplayName))
                user.DisplayName = model.DisplayName;

            if (!string.IsNullOrEmpty(model.UserName))
            {
                var userNameExists = await _userManager.Users
                    .AnyAsync(u => u.UserName == model.UserName && u.Id != user.Id);

                if (userNameExists)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Update failed",
                        Detail = "Username is already in use by another user."
                    });
                }

                user.UserName = model.UserName;
                user.NormalizedUserName = _userManager.NormalizeName(model.UserName);
            }


            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Update failed",
                    Detail = string.Join("; ", result.Errors.Select(e => e.Description))
                });
            }

            var updatedProfile = new UserProfileDto
            {
                DisplayName = user.DisplayName,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return Ok(updatedProfile);
        }

    }
}
