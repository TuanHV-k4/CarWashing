namespace BusinessLayer.Helpers
{
    public class AiSettings
    {
        public int MaxConversationMessages { get; set; } = 20;
        public int RateLimitPerMinute { get; set; } = 10;
        public int AdminRateLimitPerMinute { get; set; } = 20;
        public bool UseMockCustomerContext { get; set; }
    }
}
