using System.Text.Json.Serialization;

namespace SignLanguage.Core
{
    public class LocationDto
    {
        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("country_name")]
        public string CountryName { get; set; }
    }
}
