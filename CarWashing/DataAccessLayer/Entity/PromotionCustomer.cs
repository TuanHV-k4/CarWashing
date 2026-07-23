using System;

namespace DataAccessLayer.Entity
{
    public class PromotionCustomer
    {
        public PromotionCustomer()
        {
            PromotionCustomerID = Guid.NewGuid();
        }

        public Guid PromotionCustomerID { get; set; }
        public Guid PromotionID { get; set; }
        public Guid CustomerID { get; set; }
        public bool IsUsed { get; set; }
        public int UsageCount { get; set; }
        public DateTime? UsedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public Guid? SentByUserID { get; set; }
        public string? AudienceSnapshot { get; set; }
        public string? EligibilitySnapshot { get; set; }

        public virtual Promotion Promotion { get; set; } = null!;
        public virtual Customer Customer { get; set; } = null!;
    }
}
