using System;
using DataAccessLayer.Enums;
using System.Collections.Generic;

namespace DataAccessLayer.Entity
{
    public class Service
    {
        public Service()
        {
            ServiceID = Guid.NewGuid();
            BookingDetails = new HashSet<BookingDetail>();
            PromotionServices = new HashSet<PromotionService>();
            Rewards = new HashSet<Reward>();
            TierBenefits = new HashSet<TierBenefit>();
            BehavioralLogs = new HashSet<BehavioralLog>();
            FreeServicePromotions = new HashSet<Promotion>();
        }

        public Guid ServiceID { get; set; }
        public string ServiceName { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public TimeSpan? EstimatedDuration { get; set; }
        public ServiceStatusEnum Status { get; set; }

        public virtual ICollection<BookingDetail> BookingDetails { get; set; }
        public virtual ICollection<PromotionService> PromotionServices { get; set; }
        public virtual ICollection<Reward> Rewards { get; set; }
        public virtual ICollection<TierBenefit> TierBenefits { get; set; }
        public virtual ICollection<BehavioralLog> BehavioralLogs { get; set; }
        public virtual ICollection<Promotion> FreeServicePromotions { get; set; }
    }
}
