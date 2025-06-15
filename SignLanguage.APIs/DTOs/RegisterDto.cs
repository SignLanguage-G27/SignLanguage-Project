using System.ComponentModel.DataAnnotations;

namespace SignLanguage.APIs.DTOs
{
    public class RegisterDto
    {
        [Required]
        [RegularExpression(@"^[A-Za-z0-9_.\- ]{1,20}$")]
        public string DisplayName { get; set; }

        [Required]
        [RegularExpression(@"(\+20)[0-9]{10}$")]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        [RegularExpression(@"^[a-zA-Z0-9](?:[a-zA-Z0-9._]*[a-zA-Z0-9])?@gmail\.com$")]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,20}$")]
        public string Password { get; set; }

        [Required]
        [RegularExpression(@"^^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,20}$")]
        public string RePassword { get; set; }
    }
}
