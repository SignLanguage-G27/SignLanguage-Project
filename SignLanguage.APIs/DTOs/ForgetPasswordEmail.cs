using System.ComponentModel.DataAnnotations;

namespace SignLanguage.APIs.DTOs
{
    public class ForgetPasswordEmail
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
