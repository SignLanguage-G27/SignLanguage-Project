using Microsoft.AspNetCore.Identity;
using SignLanguage.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignLanguage.Core.Service.Contract
{
    public interface IAuthService
    {
        Task SendWelcomeEmailAfterRegistration(string email);
        Task SendLoginNotificationEmailAsync(string email, string ipAddress, string userAgent);
        Task<string>CreateTokenAsync(AppUser user,UserManager<AppUser> userManager);

        Task<ApiResponseForForgetPass> ForgetPasswordByEmailAsync(string email);

        Task<ApiResponseForForgetPass> ForgetPasswordByTelegramAsync(string phoneNumber);

        Task<ApiResponseForForgetPass> ResetPasswordAsync(string identifier, string newPassword, bool isEmail);

        Task<ApiResponseForForgetPass> VerifyResetCodeAsync(string identifier, string resetCode, bool isEmail);
    }
}
