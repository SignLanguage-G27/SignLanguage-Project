using System.ComponentModel.DataAnnotations;

namespace SignLanguage.APIs.DTOs
{
    public class ImageUploadDto
    {

        [Required]
        public string ImgPath  { get; set; }
    }
}
