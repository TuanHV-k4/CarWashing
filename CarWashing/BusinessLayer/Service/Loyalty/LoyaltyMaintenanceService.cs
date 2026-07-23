using BusinessLayer.IService.Loyalty;
using DataAccessLayer.Context;
using DataAccessLayer.Entity;
using DataAccessLayer.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service.Loyalty
{
    public class LoyaltyMaintenanceService : ILoyaltyMaintenanceService
    {
        private readonly ApplicationDbContext _context;

        public LoyaltyMaintenanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LoyaltyMaintenanceResult> ExpireOverdueItemsAsync(DateTime now, CancellationToken cancellationToken = default)
        {
            var result = new LoyaltyMaintenanceResult();

            var expiredPointTransactions = await _context.LoyaltyPointTransactions
                .Include(transaction => transaction.Customer)
                .Where(transaction =>
                    transaction.TransactionType == PointTransactionTypeEnum.Earn &&
                    transaction.RemainingPoints > 0 &&
                    transaction.ExpiryDate.HasValue &&
                    transaction.ExpiryDate.Value <= now)
                .OrderBy(transaction => transaction.ExpiryDate)
                .ThenBy(transaction => transaction.CreatedAt)
                .ToListAsync(cancellationToken);

            foreach (var transaction in expiredPointTransactions)
            {
                var expiredPoints = transaction.RemainingPoints;
                if (expiredPoints <= 0)
                {
                    continue;
                }

                transaction.RemainingPoints = 0;
                transaction.Customer.CurrentPoints = Math.Max(0, transaction.Customer.CurrentPoints - expiredPoints);
                transaction.Customer.UpdatedAt = now;

                _context.LoyaltyPointTransactions.Add(new LoyaltyPointTransaction
                {
                    CustomerID = transaction.CustomerID,
                    BookingID = transaction.BookingID,
                    WashHistoryID = transaction.WashHistoryID,
                    ReferenceTransactionID = transaction.TransactionID,
                    Points = -expiredPoints,
                    OriginalPoints = 0,
                    RemainingPoints = 0,
                    BalanceAfter = transaction.Customer.CurrentPoints,
                    TransactionType = PointTransactionTypeEnum.Expire,
                    Description = "Points expired"
                });

                result.ExpiredPointTransactions++;
                result.ExpiredPoints += expiredPoints;
            }

            var expiredRedemptions = await _context.RewardRedemptions
                .Where(redemption =>
                    redemption.Status == RewardRedemptionStatusEnum.Reserved &&
                    redemption.ExpiresAt.HasValue &&
                    redemption.ExpiresAt.Value <= now)
                .ToListAsync(cancellationToken);

            foreach (var redemption in expiredRedemptions)
            {
                redemption.Status = RewardRedemptionStatusEnum.Expired;
                result.ExpiredRedemptions++;
            }

            if (result.ExpiredPointTransactions > 0 || result.ExpiredRedemptions > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return result;
        }
    }
}
