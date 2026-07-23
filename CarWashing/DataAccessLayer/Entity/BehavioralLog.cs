using System;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Entity
{
    public class BehavioralLog
    {
        public BehavioralLog()
        {
            LogID = Guid.NewGuid();
            ActionTime = DateTime.UtcNow;
        }

        public Guid LogID { get; set; }
        public Guid? CustomerID { get; set; }
        public Guid? BookingID { get; set; }
        public Guid? ServiceID { get; set; }
        public Guid? PromotionID { get; set; }
        public string? SessionID { get; set; }
        public BehavioralActionTypeEnum ActionType { get; set; }
        public DateTime ActionTime { get; set; }
        public int PointsChanged { get; set; }
        public decimal SpendingAmount { get; set; }
        public decimal RewardUsed { get; set; }
        public bool PromotionUsed { get; set; }
        public string? MetadataJson { get; set; }
        public string? Notes { get; set; }

        public virtual Customer? Customer { get; set; }
        public virtual Booking? Booking { get; set; }
        public virtual Service? Service { get; set; }
        public virtual Promotion? Promotion { get; set; }
    }
}
