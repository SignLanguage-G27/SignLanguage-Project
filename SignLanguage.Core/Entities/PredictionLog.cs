using SignLanguage.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignLanguage.Core.Entities
{
    public class PredictionLog
    {
        public int Id { get; set; }             
        public string ImagePath { get; set; }
        public string Result { get; set; }
        public string Confidence { get; set; }
        public DateTime PredictTime { get; set; } = DateTime.UtcNow;
        public string UserId { get; set; }      
        public AppUser User { get; set; }

    }
}
