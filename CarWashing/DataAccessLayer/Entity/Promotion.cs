using System;
using System.Collections.Generic;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Entity
{
    public class Promotion
    {
        public Promotion()
        {
            PromotionID = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            PromotionCustomers = new HashSet<PromotionCustomer>();
            PromotionServices = new HashSet<PromotionService>();
            BookingPromotions = new HashSet<BookingPromotion>();
            BehavioralLogs = new HashSet<BehavioralLog>();
        }

        public Guid PromotionID { get; set; }
        public string PromotionName { get; set; } = null!;
        public string? PromotionCode { get; set; }
        public string? Description { get; set; }
        public PromotionTypeEnum PromotionType { get; set; }
        public decimal PromotionValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public int BonusPoints { get; set; }
        public Guid? FreeServiceID { get; set; }
        public decimal MinimumSpend { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid? MinTierID { get; set; }
        public int? TotalUsageLimit { get; set; }
        public int? UsageLimitPerCustomer { get; set; }
        public int Priority { get; set; }
        public bool IsStackable { get; set; }
        public PromotionStatusEnum Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual LoyaltyTier? MinTier { get; set; }
        public virtual Service? FreeService { get; set; }
        public virtual ICollection<PromotionCustomer> PromotionCustomers { get; set; }
        public virtual ICollection<PromotionService> PromotionServices { get; set; }
        public virtual ICollection<BookingPromotion> BookingPromotions { get; set; }
        public virtual ICollection<BehavioralLog> BehavioralLogs { get; set; }
    }
}
