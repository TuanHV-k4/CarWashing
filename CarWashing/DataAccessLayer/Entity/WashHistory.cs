using System;

namespace DataAccessLayer.Entity
{
    public class WashHistory
    {
        public WashHistory()
        {
            WashHistoryID = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            PointTransactions = new HashSet<LoyaltyPointTransaction>();
        }

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
        public DateTime CreatedAt { get; set; }

        public virtual Booking Booking { get; set; } = null!;
        public virtual ICollection<LoyaltyPointTransaction> PointTransactions { get; set; }
    }
}
