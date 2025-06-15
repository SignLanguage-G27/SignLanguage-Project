using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignLanguage.Core.Service.Contract
{
    public interface IAttachmentService
    {
        string? Upload(IFormFile file, string foldername);
        bool Delete(string filePath);
    }
}
