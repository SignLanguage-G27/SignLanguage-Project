using System.Text.Json.Serialization;


namespace SignLanguage.Core.TelegramHelpers
{
    public class TelegramChat
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
    }
}
