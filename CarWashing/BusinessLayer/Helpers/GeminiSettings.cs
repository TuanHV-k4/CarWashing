namespace BusinessLayer.Helpers
{
    public class GeminiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gemini-2.0-flash";
        public int TimeoutSeconds { get; set; } = 30;
        public bool UseMockFallback { get; set; } = true;
    }
}
