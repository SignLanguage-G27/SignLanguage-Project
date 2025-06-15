using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignLanguage.Core.Service.Contract
{
    public interface ITelegramService
    {
        Task SendMessageAsync(string chatId, string message, string phoneNumber = null);
    }
}
