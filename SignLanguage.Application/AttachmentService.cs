using Microsoft.AspNetCore.Http;
using SignLanguage.Core.Service.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignLanguage.Application
{
    public class AttachmentService : IAttachmentService
    {
        public bool Delete(string filePath)
        {
            throw new NotImplementedException();
        }

        public string Upload(IFormFile file, string folderName)
        {
            string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files", folderName);
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder); // إنشاء المجلد إذا لم يكن موجودًا
            }

            string filePath = Path.Combine(uploadFolder, Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            return filePath;
        }

    }
}
