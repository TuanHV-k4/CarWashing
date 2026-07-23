using System;
using System.Collections.Generic;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Entity
{
    public class LoyaltyTier
    {
        public LoyaltyTier()
        {
            TierID = Guid.NewGuid();
            Promotions = new HashSet<Promotion>();
            Customers = new HashSet<Customer>();
            Benefits = new HashSet<TierBenefit>();
            TierHistoriesAsPrevious = new HashSet<CustomerTierHistory>();
            TierHistoriesAsNew = new HashSet<CustomerTierHistory>();
            BookingSnapshots = new HashSet<Booking>();
        }

        public Guid TierID { get; set; }
        public string TierName { get; set; } = null!;
        public int TierRank { get; set; }
        public decimal MinSpent { get; set; }
        public int MinVisits { get; set; }
        public int QualificationPeriodMonths { get; set; }
        public TierQualificationModeEnum QualificationMode { get; set; }
        public int BookingWindowDays { get; set; }
        public int PriorityLevel { get; set; }
        public decimal PointMultiplier { get; set; }
        public string? TierBenefits { get; set; }
        public LoyaltyTierStatusEnum Status { get; set; }

        public virtual ICollection<Promotion> Promotions { get; set; }
        public virtual ICollection<Customer> Customers { get; set; }
        public virtual ICollection<TierBenefit> Benefits { get; set; }
        public virtual ICollection<CustomerTierHistory> TierHistoriesAsPrevious { get; set; }
        public virtual ICollection<CustomerTierHistory> TierHistoriesAsNew { get; set; }
        public virtual ICollection<Booking> BookingSnapshots { get; set; }
    }
}
