using DataAccessLayer.Enums;

namespace DataAccessLayer.Entity
{
    public class Reward
    {
        public Reward()
        {
            RewardID = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            Redemptions = new HashSet<RewardRedemption>();
        }

        public Guid RewardID { get; set; }
        public string RewardName { get; set; } = null!;
        public string? Description { get; set; }
        public RewardTypeEnum RewardType { get; set; }
        public int PointsRequired { get; set; }
        public decimal RewardValue { get; set; }
        public Guid? ServiceID { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public int? UsageLimitPerCustomer { get; set; }
        public RewardStatusEnum Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Service? Service { get; set; }
        public virtual ICollection<RewardRedemption> Redemptions { get; set; }
    }
}
