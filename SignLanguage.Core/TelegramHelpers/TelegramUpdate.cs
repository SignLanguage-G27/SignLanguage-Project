
using System.Text.Json.Serialization;

namespace SignLanguage.Core.TelegramHelpers
{
    public class TelegramUpdate
    {
        [JsonPropertyName("message")]
        public TelegramMessage? Message { get; set; }

        [JsonPropertyName("update_id")]
        public long UpdateId { get; set; }
    }
}
