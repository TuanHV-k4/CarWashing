using DataAccessLayer.Enums;

namespace DataAccessLayer.Entity
{
    public class CustomerTierHistory
    {
        public CustomerTierHistory()
        {
            CustomerTierHistoryID = Guid.NewGuid();
            ChangedAt = DateTime.UtcNow;
        }

        public Guid CustomerTierHistoryID { get; set; }
        public Guid CustomerID { get; set; }
        public Guid? PreviousTierID { get; set; }
        public Guid NewTierID { get; set; }
        public DateTime ReviewPeriodStart { get; set; }
        public DateTime ReviewPeriodEnd { get; set; }
        public decimal QualifiedSpent { get; set; }
        public int QualifiedVisits { get; set; }
        public TierChangeReasonEnum ChangeReason { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Notes { get; set; }

        public virtual Customer Customer { get; set; } = null!;
        public virtual LoyaltyTier? PreviousTier { get; set; }
        public virtual LoyaltyTier NewTier { get; set; } = null!;
    }
}
