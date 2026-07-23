namespace BusinessLayer.Dtos.Customer
{
    public class CustomerProfileDto
    {
        public Guid CustomerID { get; set; }
        public Guid UserID { get; set; }
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public int CurrentPoints { get; set; }
        public int LifetimePoints { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalVisits { get; set; }
        public DateTime? LastVisitDate { get; set; }
        public Guid? TierID { get; set; }
        public string? TierName { get; set; }
        public int? TierRank { get; set; }
        public DateTime? CurrentTierSince { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> TierPerks { get; set; } = [];
    }

    public class UpdateCustomerProfileRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }
}
