using System.Text.Json.Serialization;

namespace SignLanguage.APIs.DTOs
{
    public class PredictionResponse
    {
        [JsonPropertyName("predicted_label")]
        public string PredictedLabel { get; set; }

        [JsonPropertyName("confidence")]
        public string Confidence { get; set; }
    }

}
