using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.Dtos.Loyalty
{
    public class LoyaltyTierResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Rank { get; set; }
        public decimal MinSpent { get; set; }
        public int MinVisits { get; set; }
        public int QualificationPeriodMonths { get; set; }
        public string QualificationMode { get; set; } = string.Empty;
        public int BookingWindowDays { get; set; }
        public int PriorityLevel { get; set; }
        public decimal PointMultiplier { get; set; }
        public string? Benefits { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class LoyaltySettingsResponse
    {
        public decimal PointEarnRateAmount { get; set; }
        public int PointEarnRatePoints { get; set; }
        public int PointExpiryMonths { get; set; }
        public string EarnRule { get; set; } = string.Empty;
    }

    public class CreateLoyaltyTierRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        public int Rank { get; set; }
        public decimal MinSpent { get; set; }
        public int MinVisits { get; set; }
        public int QualificationPeriodMonths { get; set; } = 12;
        public string QualificationMode { get; set; } = "AllConditions";
        public int BookingWindowDays { get; set; } = 7;
        public int PriorityLevel { get; set; }
        public decimal PointMultiplier { get; set; } = 1;
        public string? Benefits { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateLoyaltyTierRequest : CreateLoyaltyTierRequest
    {
    }

    public class PointBalanceResponse
    {
        public Guid CustomerId { get; set; }
        public int CurrentPoints { get; set; }
        public int LifetimePoints { get; set; }
        public string? CurrentTier { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalVisits { get; set; }
    }

    public class PointTransactionResponse
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? BookingId { get; set; }
        public Guid? WashHistoryId { get; set; }
        public Guid? RedemptionId { get; set; }
        public int Points { get; set; }
        public int OriginalPoints { get; set; }
        public int RemainingPoints { get; set; }
        public int BalanceAfter { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public string? IdempotencyKey { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class WashHistoryResponse
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public DateTime WashDate { get; set; }
        public decimal ActualTotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public int PointsEarned { get; set; }
        public decimal RewardUsed { get; set; }
        public int? CustomerRating { get; set; }
        public string? Feedback { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RewardResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
        public int PointsRequired { get; set; }
        public decimal Value { get; set; }
        public Guid? ServiceId { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public int? UsageLimitPerCustomer { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateRewardRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = "FixedDiscount";
        public int PointsRequired { get; set; }
        public decimal Value { get; set; }
        public Guid? ServiceId { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public int? UsageLimitPerCustomer { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateRewardRequest : CreateRewardRequest
    {
    }

    public class RedeemRewardRequest
    {
        public Guid CustomerId { get; set; }
        public Guid? BookingId { get; set; }
        public string? IdempotencyKey { get; set; }
    }

    public class RewardRedemptionResponse
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid RewardId { get; set; }
        public Guid? BookingId { get; set; }
        public int PointsSpent { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RedeemedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
        public decimal AppliedDiscountAmount { get; set; }
    }

    public class ApplyRewardRedemptionRequest
    {
        public Guid BookingId { get; set; }
        public Guid CustomerId { get; set; }
    }

    public class CustomerVoucherResponse
    {
        public Guid Id { get; set; }
        public Guid SourceId { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public int? PointsSpent { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
        public bool CanApply { get; set; }
    }

    public class TierEvaluationResponse
    {
        public Guid CustomerId { get; set; }
        public Guid? PreviousTierId { get; set; }
        public Guid CurrentTierId { get; set; }
        public string CurrentTierName { get; set; } = string.Empty;
        public decimal QualifiedSpent { get; set; }
        public int QualifiedVisits { get; set; }
        public bool Changed { get; set; }
    }

    public class TierHistoryResponse
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? PreviousTierId { get; set; }
        public Guid NewTierId { get; set; }
        public DateTime ReviewPeriodStart { get; set; }
        public DateTime ReviewPeriodEnd { get; set; }
        public decimal QualifiedSpent { get; set; }
        public int QualifiedVisits { get; set; }
        public string ChangeReason { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class LoyaltyDashboardResponse
    {
        public int ActiveCustomers { get; set; }
        public int ActiveRewards { get; set; }
        public int PointsIssued { get; set; }
        public int PointsRedeemed { get; set; }
        public decimal Revenue { get; set; }
        public int CompletedWashes { get; set; }
        public int ExpiringPoints { get; set; }
        public decimal PromotionUsageRate { get; set; }
        public IReadOnlyList<LoyaltyDailyPointResponse> DailyPoints { get; set; } = Array.Empty<LoyaltyDailyPointResponse>();
        public IReadOnlyList<LoyaltyTierDistributionResponse> TierDistribution { get; set; } = Array.Empty<LoyaltyTierDistributionResponse>();
    }

    public class LoyaltyDailyPointResponse { public DateTime Date { get; set; } public int Issued { get; set; } public int Redeemed { get; set; } }
    public class LoyaltyPointActivityResponse { public Guid TransactionId { get; set; } public Guid CustomerId { get; set; } public string CustomerName { get; set; } = string.Empty; public string? PhoneNumber { get; set; } public int Points { get; set; } public string Type { get; set; } = string.Empty; public string? Description { get; set; } public DateTime CreatedAt { get; set; } }
    public class LoyaltyTierDistributionResponse { public string TierName { get; set; } = string.Empty; public int CustomerCount { get; set; } }
    public class LoyaltySegmentCustomerResponse { public Guid CustomerId { get; set; } public string FullName { get; set; } = string.Empty; public string? PhoneNumber { get; set; } public string? TierName { get; set; } public int CurrentPoints { get; set; } public int LifetimePoints { get; set; } public DateTime? LastVisitDate { get; set; } public DateTime? NearestExpiryDate { get; set; } public int ExpiringPoints { get; set; } public decimal TotalSpent { get; set; } }
    public class LoyaltyCustomer360Response { public LoyaltySegmentCustomerResponse Customer { get; set; } = new(); public IReadOnlyList<PointTransactionResponse> PointLedger { get; set; } = Array.Empty<PointTransactionResponse>(); public IReadOnlyList<CustomerVoucherResponse> Vouchers { get; set; } = Array.Empty<CustomerVoucherResponse>(); public IReadOnlyList<WashHistoryResponse> WashHistory { get; set; } = Array.Empty<WashHistoryResponse>(); }
    public class CustomerLoyaltyOverviewResponse { public PointBalanceResponse Balance { get; set; } = new(); public string? NextTierName { get; set; } public decimal NextTierMinSpent { get; set; } public int NextTierMinVisits { get; set; } public int ExpiringPoints { get; set; } public DateTime? NearestExpiryDate { get; set; } public IReadOnlyList<CustomerVoucherResponse> Vouchers { get; set; } = Array.Empty<CustomerVoucherResponse>(); public IReadOnlyList<PointTransactionResponse> RecentLedger { get; set; } = Array.Empty<PointTransactionResponse>(); }
}
