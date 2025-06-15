using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignLanguage.APIs.DTOs;
using SignLanguage.Core;
using SignLanguage.Core.Entities.Identity;
using SignLanguage.Core.Service.Contract;

namespace SignLanguage.APIs.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountController
            (
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IAuthService authService,
            IEmailService emailService,
            ILogger<AccountController> logger,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _userManager=userManager;
            _signInManager=signInManager;
            _authService = authService;
            _emailService=emailService;
            _logger=logger;
            _httpContextAccessor=httpContextAccessor;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user==null) return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (!result.Succeeded) return Unauthorized();
            var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString();

            await _authService.SendLoginNotificationEmailAsync(user.Email, ipAddress, userAgent);


            return Ok(new UserDto()
            {
                Message="Success",
                DisplayName=user.DisplayName,
                Email=user.Email,
                Token=await _authService.CreateTokenAsync(user, _userManager)
            });
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto model)
        {
            var existingUserByEmail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            var existingUserByPhone = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);

            if (existingUserByEmail != null && existingUserByPhone != null)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Registration failed",
                    Detail = "Both email and phone number are already in use."
                });
            }
            else if (existingUserByEmail != null)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Registration failed",
                    Detail = "Email is already in use."
                });
            }
            else if (existingUserByPhone != null)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Registration failed",
                    Detail = "Phone number is already in use."
                });
            }

            await _authService.SendWelcomeEmailAfterRegistration(model.Email);

            var user = new AppUser()
            {
                DisplayName=model.DisplayName,
                Email=model.Email,
                UserName=model.Email.Split("@")[0],
                PhoneNumber=model.PhoneNumber
            };

            //var result = await _userManager.CreateAsync(user, model.Password);

            // إنشاء كلمة المرور (تشفيرها)
            var passwordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);
            user.PasswordHash = passwordHash;

            // تعيين قيمة RePassword بنفس قيمة PasswordHash
            user.RePassword = passwordHash;

            // إضافة المستخدم إلى قاعدة البيانات
            var result = await _userManager.CreateAsync(user);


            if (!result.Succeeded)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Registration failed",
                    Detail = string.Join("; ", result.Errors.Select(e => e.Description))
                });
            }

            user.RePassword=user.PasswordHash;

            return Ok(new UserDto()
            {
                Message="Success",
                DisplayName=user.DisplayName,
                Email=user.Email,
                Token= await _authService.CreateTokenAsync(user, _userManager)
            });

        }

        [HttpPost("forgotPasswordByEmail")]
        public async Task<ActionResult<ApiResponse>> ForgotPasswordByEmail([FromBody] ForgotPasswordRequest request)
        {
            var result = await _authService.ForgetPasswordByEmailAsync(request.Email);

            if (result.Success)
            {
                return Ok(result); // استخدام ApiResponse مباشرة
            }
            return BadRequest(result);
        }

        [HttpPost("forgotPasswordByTelegram")]
        public async Task<ActionResult<ApiResponse>> ForgotPasswordByTelegram([FromBody] ForgotPasswordTelegram request)
        {
            var result = await _authService.ForgetPasswordByTelegramAsync(request.PhoneNumber);

            if (result.Success)
            {
                return Ok(result); // استخدام ApiResponse مباشرة
            }
            return BadRequest(result);
        }

        [HttpPost("verifyResetCode")]
        public async Task<ActionResult<ApiResponseForForgetPass>> VerifyResetCode([FromBody] VerifyResetCodeRequest request)
        {
            var result = await _authService.VerifyResetCodeAsync(request.Identifier, request.ResetCode, request.IsEmail);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("resetPassword")]
        public async Task<ActionResult<ApiResponseForForgetPass>> ResetPassword([FromBody] ResetPasswordDto request)
        {
            var result = await _authService.ResetPasswordAsync(request.Identifier, request.NewPassword, request.IsEmail);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

    }
}
