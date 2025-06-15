using System.ComponentModel.DataAnnotations;

namespace SignLanguage.APIs.DTOs
{
    public class ResetPasswordDto
    {
        public string Identifier { get; set; }
        public string NewPassword { get; set; }
        public bool IsEmail { get; set; }



    }
}
