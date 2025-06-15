using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignLanguage.Core.Service.Contract
{
    public interface IRedisService
    {
        Task SetDataAsync(string key, string value, TimeSpan expiry);
        Task<string?> GetDataAsync(string key);
        Task DeleteDataAsync(string key);
        Task<TimeSpan?> GetTimeToLiveAsync(string key);
    }
}
