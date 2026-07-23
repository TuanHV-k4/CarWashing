namespace BusinessLayer.Dtos.AI
{
    public class AiChatRequestDto
    {
        public string Message { get; set; } = null!;
        public string? ConversationId { get; set; }
    }

    public class AiChatResponseDto
    {
        public string Reply { get; set; } = null!;
        public string ConversationId { get; set; } = null!;
        public bool IsFallback { get; set; }
        public string Source { get; set; } = null!;
    }

    public class AiSuggestServicesRequestDto
    {
        public string? VehicleType { get; set; }
        public string? Preference { get; set; }
    }

    public class AiSuggestedServiceDto
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Reason { get; set; }
    }

    public class AiSuggestServicesResponseDto
    {
        public List<AiSuggestedServiceDto> Suggestions { get; set; } = [];
        public string Summary { get; set; } = null!;
        public bool IsFallback { get; set; }
        public string Source { get; set; } = null!;
    }

    public class AiAdminChatRequestDto
    {
        public string Message { get; set; } = null!;
        public string? ConversationId { get; set; }
    }

    public sealed class AiCustomerAssistantRequestDto
    {
        public string? Preference { get; set; }
        public Guid? BranchId { get; set; }
        public DateOnly? Date { get; set; }
        public List<Guid> ServiceIds { get; set; } = [];
    }

    public sealed class AiOfferDto
    {
        public Guid PromotionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string EligibilityNote { get; set; } = string.Empty;
    }

    public sealed class AiSlotSuggestionDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int AvailableBayCount { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public sealed class AiCustomerAssistantResponseDto
    {
        public List<AiSuggestedServiceDto> Recommendations { get; set; } = [];
        public List<AiOfferDto> EligibleOffers { get; set; } = [];
        public List<AiSlotSuggestionDto> SuggestedSlots { get; set; } = [];
        public string LoyaltySummary { get; set; } = string.Empty;
        public string CareTip { get; set; } = string.Empty;
        public bool IsFallback { get; set; } = true;
        public string Source { get; set; } = "rules";
    }

    public sealed class AiFeedbackThemeDto { public string Theme { get; set; } = string.Empty; public int Count { get; set; } }
    public sealed class AiFeedbackRecordDto { public Guid WashHistoryId { get; set; } public Guid BookingId { get; set; } public int Rating { get; set; } public string? FeedbackPreview { get; set; } public DateTime WashDate { get; set; } }
    public sealed class AiFeedbackInsightsResponseDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public Guid? BranchId { get; set; }
        public int FeedbackCount { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = [];
        public List<AiFeedbackThemeDto> Themes { get; set; } = [];
        public List<AiFeedbackRecordDto> LowRatings { get; set; } = [];
        public bool IsFallback { get; set; } = true;
        public string Source { get; set; } = "rules";
    }

    public sealed class AiOperationsCopilotRequestDto { public string Message { get; set; } = string.Empty; public DateTime From { get; set; } public DateTime To { get; set; } public Guid? BranchId { get; set; } }
    public sealed class AiEvidenceDto { public string Label { get; set; } = string.Empty; public string Value { get; set; } = string.Empty; public string Period { get; set; } = string.Empty; }
    public sealed class AiActionDto { public string Label { get; set; } = string.Empty; public string Path { get; set; } = string.Empty; }
    public sealed class AiOperationsCopilotResponseDto
    {
        public string Answer { get; set; } = string.Empty;
        public Guid? BranchId { get; set; }
        public List<AiEvidenceDto> Evidence { get; set; } = [];
        public List<AiActionDto> Actions { get; set; } = [];
        public bool IsFallback { get; set; } = true;
        public string Source { get; set; } = "rules";
    }

    public class CustomerAiContextDto
    {
        public string CustomerName { get; set; } = null!;
        public int CurrentPoints { get; set; }
        public string? TierName { get; set; }
        public int TotalVisits { get; set; }
        public decimal TotalSpent { get; set; }
        public List<string> Vehicles { get; set; } = [];
        public List<string> Perks { get; set; } = [];
        public List<CustomerWashHistoryContextDto> RecentWashes { get; set; } = [];
        public List<ServiceCatalogItemDto> AvailableServices { get; set; } = [];
    }

    public class CustomerWashHistoryContextDto
    {
        public DateTime WashDate { get; set; }
        public string? Services { get; set; }
        public decimal FinalAmount { get; set; }
    }

    public class AdminAiContextDto
    {
        public int TotalCustomers { get; set; }
        public int TotalActiveUsers { get; set; }
        public int TotalVehicles { get; set; }
        public int TotalBookingsToday { get; set; }
        public List<TierSummaryDto> TierDistribution { get; set; } = [];
        public List<ServiceCatalogItemDto> ActiveServices { get; set; } = [];
    }

    public class TierSummaryDto
    {
        public string TierName { get; set; } = null!;
        public int CustomerCount { get; set; }
    }

    public class ServiceCatalogItemDto
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }
}
