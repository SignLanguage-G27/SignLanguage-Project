namespace SignLanguage.APIs.DTOs
{
    public class VerifyResetCodeRequest
    {
        public string Identifier { get; set; } 
        public string ResetCode { get; set; }
        public bool IsEmail { get; set; }
    }
}
