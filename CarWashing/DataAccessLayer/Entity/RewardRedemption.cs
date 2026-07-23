using DataAccessLayer.Enums;

namespace DataAccessLayer.Entity
{
    public class RewardRedemption
    {
        public RewardRedemption()
        {
            RedemptionID = Guid.NewGuid();
            RedeemedAt = DateTime.UtcNow;
            PointTransactions = new HashSet<LoyaltyPointTransaction>();
        }

        public Guid RedemptionID { get; set; }
        public Guid CustomerID { get; set; }
        public Guid RewardID { get; set; }
        public Guid? BookingID { get; set; }
        public int PointsSpent { get; set; }
        public RewardRedemptionStatusEnum Status { get; set; }
        public DateTime RedeemedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
        public decimal AppliedDiscountAmount { get; set; }

        public virtual Customer Customer { get; set; } = null!;
        public virtual Reward Reward { get; set; } = null!;
        public virtual Booking? Booking { get; set; }
        public virtual ICollection<LoyaltyPointTransaction> PointTransactions { get; set; }
    }
}
