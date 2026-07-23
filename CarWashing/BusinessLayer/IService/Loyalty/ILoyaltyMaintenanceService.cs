namespace BusinessLayer.IService.Loyalty
{
    public class LoyaltyMaintenanceResult
    {
        public int ExpiredPointTransactions { get; set; }
        public int ExpiredPoints { get; set; }
        public int ExpiredRedemptions { get; set; }
    }

    public interface ILoyaltyMaintenanceService
    {
        Task<LoyaltyMaintenanceResult> ExpireOverdueItemsAsync(DateTime now, CancellationToken cancellationToken = default);
    }
}
