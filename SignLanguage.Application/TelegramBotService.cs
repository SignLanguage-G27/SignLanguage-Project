using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using SignLanguage.Core.TelegramHelpers;
using SignLanguage.Core.Service.Contract;

namespace SignLanguage.Application
{
    public class TelegramBotService : ITelegramService
    {
        private readonly HttpClient _httpClient;
        private readonly string _botToken;
        private readonly IRedisService _redisService;
        private readonly ILogger<TelegramBotService> _logger;

        public TelegramBotService(IConfiguration configuration, HttpClient httpClient, IRedisService redisService,ILogger<TelegramBotService> logger)
        {
            _httpClient = httpClient;
            _botToken = configuration["TelegramBot:BotToken"];
            _redisService = redisService;
            _logger=logger;
        }

        public async Task SendMessageAsync(string chatId, string message, string phoneNumber = null)
        {
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage?chat_id={chatId}&text={message}";
            await _httpClient.GetAsync(url);

            // حفظ chatId في Redis عند إرسال رسالة لأول مرة
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                await _redisService.SetDataAsync(phoneNumber, chatId, TimeSpan.FromDays(30));
                _logger.LogInformation($"Chat ID {chatId} stored in Redis for {phoneNumber}");
            }
        }
    }
}