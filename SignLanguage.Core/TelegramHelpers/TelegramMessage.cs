using System.Text.Json.Serialization;


namespace SignLanguage.Core.TelegramHelpers
{
    public class TelegramMessage
    {
        [JsonPropertyName("chat")]
        public TelegramChat? Chat { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("contact")]
        public TelegramContact? Contact { get; set; }

    }
}
