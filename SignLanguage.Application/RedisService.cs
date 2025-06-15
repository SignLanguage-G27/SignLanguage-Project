using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SignLanguage.Core.Service.Contract;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignLanguage.Application
{
    public class RedisService : IRedisService
    {
        private readonly StackExchange.Redis.IDatabase _db;
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisService> _logger;

        public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger)
        { 
            _redis=redis;
            _db = _redis.GetDatabase();
            _logger=logger;
        }

        public async Task SetDataAsync(string key, string value, TimeSpan expiry)
        {
            try
            {
                await _db.StringSetAsync(key, value, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Redis SetData Error: {ex.Message}");
            }
        }

        public async Task<string?> GetDataAsync(string key)
        {
            try
            {
                return await _db.StringGetAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Redis GetData Error: {ex.Message}");
                return null;
            }
        }

        public async Task DeleteDataAsync(string key)
        {
            try
            {
                await _db.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Redis DeleteData Error: {ex.Message}");
            }
        }

        public async Task<TimeSpan?> GetTimeToLiveAsync(string key)
        {
            var db = _redis.GetDatabase();
            var ttl = await db.KeyTimeToLiveAsync(key);
            return ttl;
        }


    }
}
