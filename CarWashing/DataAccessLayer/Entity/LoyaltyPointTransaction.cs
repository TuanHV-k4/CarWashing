using System;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Entity
{
    public class LoyaltyPointTransaction
    {
        public LoyaltyPointTransaction()
        {
            TransactionID = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        public Guid TransactionID { get; set; }
        public Guid CustomerID { get; set; }
        public Guid? BookingID { get; set; }
        public Guid? WashHistoryID { get; set; }
        public Guid? RedemptionID { get; set; }
        public Guid? ReferenceTransactionID { get; set; }
        public int Points { get; set; }
        public int OriginalPoints { get; set; }
        public int RemainingPoints { get; set; }
        public int BalanceAfter { get; set; }
        public PointTransactionTypeEnum TransactionType { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? IdempotencyKey { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Customer Customer { get; set; } = null!;
        public virtual Booking? Booking { get; set; }
        public virtual WashHistory? WashHistory { get; set; }
        public virtual RewardRedemption? Redemption { get; set; }
        public virtual LoyaltyPointTransaction? ReferenceTransaction { get; set; }
    }
}
