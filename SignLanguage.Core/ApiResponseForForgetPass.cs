using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignLanguage.Core
{
    public class ApiResponseForForgetPass
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? ExpiresIn { get; set; }
    }
}
