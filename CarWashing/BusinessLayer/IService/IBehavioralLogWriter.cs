using DataAccessLayer.Enums;

namespace BusinessLayer.IService
{
    public class BehavioralLogWriteRequest
    {
        public Guid? CustomerId { get; set; }
        public Guid? BookingId { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? PromotionId { get; set; }
        public BehavioralActionTypeEnum ActionType { get; set; }
        public int PointsChanged { get; set; }
        public decimal SpendingAmount { get; set; }
        public decimal RewardUsed { get; set; }
        public bool PromotionUsed { get; set; }
        public string? MetadataJson { get; set; }
        public string? Notes { get; set; }
        public DateTime? ActionTime { get; set; }
    }

    public interface IBehavioralLogWriter
    {
        Task WriteAsync(BehavioralLogWriteRequest request, CancellationToken cancellationToken = default);
    }
}
