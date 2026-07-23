namespace BusinessLayer.Dtos.History
{
    public class WashHistoryListItemDto
    {
        public Guid WashHistoryID { get; set; }
        public Guid BookingID { get; set; }
        public DateTime WashDate { get; set; }
        public decimal FinalAmount { get; set; }
        public int PointsEarned { get; set; }
        public int? CustomerRating { get; set; }
        public string? VehiclePlate { get; set; }
        public string? BranchName { get; set; }
        public List<string> Services { get; set; } = [];
    }

    public class WashHistoryDetailDto
    {
        public Guid WashHistoryID { get; set; }
        public Guid BookingID { get; set; }
        public DateTime WashDate { get; set; }
        public decimal ActualTotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public int PointsEarned { get; set; }
        public decimal RewardUsed { get; set; }
        public int? CustomerRating { get; set; }
        public string? Feedback { get; set; }
        public string? VehiclePlate { get; set; }
        public string? BranchName { get; set; }
        public List<WashHistoryServiceDto> Services { get; set; } = [];
    }

    public class WashHistoryServiceDto
    {
        public Guid ServiceID { get; set; }
        public string ServiceName { get; set; } = null!;
        public decimal Price { get; set; }
    }

    public class SubmitWashFeedbackRequest
    {
        public int Rating { get; set; }
        public string? Feedback { get; set; }
    }

    public class CustomerFeedbackFilter
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? Rating { get; set; }
        public Guid? BranchId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class CustomerFeedbackStaffDto
    {
        public Guid StaffUserId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string? WorkRole { get; set; }
    }

    public class CustomerFeedbackItemDto
    {
        public Guid WashHistoryId { get; set; }
        public DateTime WashDate { get; set; }
        public int Rating { get; set; }
        public string? Feedback { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public List<string> Services { get; set; } = [];
        public List<CustomerFeedbackStaffDto> StaffMembers { get; set; } = [];
    }

    public class OperationalWashHistoryFilter
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Search { get; set; }
        public Guid? BranchId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class OperationalWashHistoryStaffDto
    {
        public Guid StaffUserId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public string? WorkRole { get; set; }
        public decimal? ContributionPercent { get; set; }
    }

    public class OperationalWashHistoryItemDto
    {
        public Guid WashHistoryId { get; set; }
        public DateTime WashDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public decimal ActualTotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public int? CustomerRating { get; set; }
        public string? Feedback { get; set; }
        public List<string> Services { get; set; } = [];
        public List<OperationalWashHistoryStaffDto> StaffMembers { get; set; } = [];
    }
}
