namespace SignLanguage.APIs.DTOs
{
    public class PredictionLogDto
    {
        public string ImagePath { get; set; }
        public string Result { get; set; }
        public string Confidence { get; set; }
        public DateTime PredictTime { get; set; }
    }
}
