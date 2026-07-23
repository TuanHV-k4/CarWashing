using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entity
{
    public class Customer
    {
        public Customer()
        {
            CustomerID = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            Vehicles = new HashSet<Vehicle>();
            Bookings = new HashSet<Booking>();
            Transactions = new HashSet<LoyaltyPointTransaction>();
            PromotionCustomers = new HashSet<PromotionCustomer>();
            BehavioralLogs = new HashSet<BehavioralLog>();
            TierHistories = new HashSet<CustomerTierHistory>();
            RewardRedemptions = new HashSet<RewardRedemption>();
        }

        public Guid CustomerID { get; set; }
        public Guid UserID { get; set; }
        public Guid? TierID { get; set; }
        public int CurrentPoints { get; set; }
        public int LifetimePoints { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalVisits { get; set; }
        public DateTime? LastVisitDate { get; set; }
        public DateTime? CurrentTierSince { get; set; }
        public DateTime? NextTierReviewAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public uint Version { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual LoyaltyTier? Tier { get; set; }
        public virtual ICollection<Vehicle> Vehicles { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<LoyaltyPointTransaction> Transactions { get; set; }
        public virtual ICollection<PromotionCustomer> PromotionCustomers { get; set; }
        public virtual ICollection<BehavioralLog> BehavioralLogs { get; set; }
        public virtual ICollection<CustomerTierHistory> TierHistories { get; set; }
        public virtual ICollection<RewardRedemption> RewardRedemptions { get; set; }
    }
}
