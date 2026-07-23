using BusinessLayer.Dtos.Loyalty;
using BusinessLayer.Dtos.Operations;
using BusinessLayer.IService;
using BusinessLayer.IService.Loyalty;
using BusinessLayer.IService.Operations;
using DataAccessLayer.Context;
using DataAccessLayer.Entity;
using DataAccessLayer.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BusinessLayer.Service.Loyalty
{
    public class LoyaltyService : ILoyaltyService, IWashCompletionService
    {
        private const decimal PointEarnRateAmount = 10000m;
        private const int PointEarnRatePoints = 1;
        private const int PointExpiryMonths = 12;

        private readonly ApplicationDbContext _context;
        private readonly IBehavioralLogWriter _behavioralLogWriter;
        private readonly ICurrentUserService _currentUserService;

        public LoyaltyService(ApplicationDbContext context, IBehavioralLogWriter behavioralLogWriter, ICurrentUserService currentUserService)
        {
            _context = context;
            _behavioralLogWriter = behavioralLogWriter;
            _currentUserService = currentUserService;
        }

        public Task<LoyaltySettingsResponse> GetSettingsAsync()
        {
            return Task.FromResult(new LoyaltySettingsResponse
            {
                PointEarnRateAmount = PointEarnRateAmount,
                PointEarnRatePoints = PointEarnRatePoints,
                PointExpiryMonths = PointExpiryMonths,
                EarnRule = $"{PointEarnRatePoints} point per {PointEarnRateAmount:N0} VND, multiplied by current tier."
            });
        }

        public async Task<PagedResult<LoyaltyTierResponse>> GetTiersAsync(int page, int pageSize, bool includeInactive)
        {
            page = NormalizePage(page);
            pageSize = NormalizePageSize(pageSize);

            var query = _context.LoyaltyTiers.AsNoTracking();
            if (!includeInactive)
            {
                query = query.Where(tier => tier.Status == LoyaltyTierStatusEnum.Active);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(tier => tier.TierRank)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(tier => MapTier(tier))
                .ToListAsync();

            return new PagedResult<LoyaltyTierResponse> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
        }

        public async Task<OperationResult<LoyaltyTierResponse>> GetTierAsync(Guid id)
        {
            var tier = await _context.LoyaltyTiers.AsNoTracking().FirstOrDefaultAsync(item => item.TierID == id);
            return tier is null
                ? OperationResult<LoyaltyTierResponse>.Failure("Loyalty tier not found.", 404)
                : OperationResult<LoyaltyTierResponse>.Success(MapTier(tier));
        }

        public async Task<OperationResult<LoyaltyTierResponse>> CreateTierAsync(CreateLoyaltyTierRequest request)
        {
            var validation = await ValidateTierRequestAsync(request, null);
            if (validation is not null)
            {
                return OperationResult<LoyaltyTierResponse>.Failure(validation, 400);
            }

            var tier = new LoyaltyTier();
            ApplyTierRequest(tier, request);

            _context.LoyaltyTiers.Add(tier);
            await _context.SaveChangesAsync();

            return OperationResult<LoyaltyTierResponse>.Success(MapTier(tier), 201);
        }

        public async Task<OperationResult<LoyaltyTierResponse>> UpdateTierAsync(Guid id, UpdateLoyaltyTierRequest request)
        {
            var tier = await _context.LoyaltyTiers.FirstOrDefaultAsync(item => item.TierID == id);
            if (tier is null)
            {
                return OperationResult<LoyaltyTierResponse>.Failure("Loyalty tier not found.", 404);
            }

            var validation = await ValidateTierRequestAsync(request, id);
            if (validation is not null)
            {
                return OperationResult<LoyaltyTierResponse>.Failure(validation, 400);
            }

            ApplyTierRequest(tier, request);
            await _context.SaveChangesAsync();

            return OperationResult<LoyaltyTierResponse>.Success(MapTier(tier));
        }

        public async Task<OperationResult<bool>> DeleteTierAsync(Guid id)
        {
            var tier = await _context.LoyaltyTiers.FirstOrDefaultAsync(item => item.TierID == id);
            if (tier is null)
            {
                return OperationResult<bool>.Failure("Loyalty tier not found.", 404);
            }

            tier.Status = LoyaltyTierStatusEnum.Inactive;
            await _context.SaveChangesAsync();

            return OperationResult<bool>.Success(true);
        }

        public async Task<OperationResult<PointBalanceResponse>> GetPointBalanceAsync(Guid customerId)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .Include(item => item.Tier)
                .FirstOrDefaultAsync(item => item.CustomerID == customerId);

            return customer is null
                ? OperationResult<PointBalanceResponse>.Failure("Customer not found.", 404)
                : OperationResult<PointBalanceResponse>.Success(MapBalance(customer));
        }

        public async Task<PagedResult<PointTransactionResponse>> GetPointHistoryAsync(Guid customerId, int page, int pageSize)
        {
            page = NormalizePage(page);
            pageSize = NormalizePageSize(pageSize);

            var query = _context.LoyaltyPointTransactions
                .AsNoTracking()
                .Where(transaction => transaction.CustomerID == customerId);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(transaction => transaction.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(transaction => MapPointTransaction(transaction))
                .ToListAsync();

            return new PagedResult<PointTransactionResponse> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
        }

        public async Task<PagedResult<WashHistoryResponse>> GetWashHistoryAsync(Guid? customerId, int page, int pageSize)
        {
            page = NormalizePage(page);
            pageSize = NormalizePageSize(pageSize);

            var query = _context.WashHistories.AsNoTracking().Include(history => history.Booking).AsQueryable();
            if (customerId.HasValue)
            {
                query = query.Where(history => history.Booking.CustomerID == customerId.Value);
            }

            var total = await query.CountAsync();
            var histories = await query
                .OrderByDescending(history => history.WashDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<WashHistoryResponse>
            {
                Items = histories.Select(MapWashHistory).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public async Task<PagedResult<RewardResponse>> GetRewardsAsync(int page, int pageSize, bool includeInactive)
        {
            page = NormalizePage(page);
            pageSize = NormalizePageSize(pageSize);

            var query = _context.Rewards.AsNoTracking();
            if (!includeInactive)
            {
                query = query.Where(reward => reward.Status == RewardStatusEnum.Active);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(reward => reward.PointsRequired)
                .ThenBy(reward => reward.RewardName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(reward => MapReward(reward))
                .ToListAsync();

            return new PagedResult<RewardResponse> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
        }

        public async Task<OperationResult<RewardResponse>> GetRewardAsync(Guid id)
        {
            var reward = await _context.Rewards.AsNoTracking().FirstOrDefaultAsync(item => item.RewardID == id);
            return reward is null
                ? OperationResult<RewardResponse>.Failure("Reward not found.", 404)
                : OperationResult<RewardResponse>.Success(MapReward(reward));
        }

        public async Task<OperationResult<RewardResponse>> CreateRewardAsync(CreateRewardRequest request)
        {
            var validation = await ValidateRewardRequestAsync(request, null);
            if (validation is not null)
            {
                return OperationResult<RewardResponse>.Failure(validation, 400);
            }

            var reward = new Reward();
            ApplyRewardRequest(reward, request);

            _context.Rewards.Add(reward);
            await _context.SaveChangesAsync();

            return OperationResult<RewardResponse>.Success(MapReward(reward), 201);
        }

        public async Task<OperationResult<RewardResponse>> UpdateRewardAsync(Guid id, UpdateRewardRequest request)
        {
            var reward = await _context.Rewards.FirstOrDefaultAsync(item => item.RewardID == id);
            if (reward is null)
            {
                return OperationResult<RewardResponse>.Failure("Reward not found.", 404);
            }

            var validation = await ValidateRewardRequestAsync(request, id);
            if (validation is not null)
            {
                return OperationResult<RewardResponse>.Failure(validation, 400);
            }

            ApplyRewardRequest(reward, request);
            await _context.SaveChangesAsync();

            return OperationResult<RewardResponse>.Success(MapReward(reward));
        }

        public async Task<OperationResult<bool>> DeleteRewardAsync(Guid id)
        {
            var reward = await _context.Rewards.FirstOrDefaultAsync(item => item.RewardID == id);
            if (reward is null)
            {
                return OperationResult<bool>.Failure("Reward not found.", 404);
            }

            reward.Status = RewardStatusEnum.Archived;
            await _context.SaveChangesAsync();

            return OperationResult<bool>.Success(true);
        }

        public async Task<OperationResult<RewardRedemptionResponse>> RedeemRewardAsync(Guid rewardId, RedeemRewardRequest request)
        {
            if (request.CustomerId == Guid.Empty)
            {
                return OperationResult<RewardRedemptionResponse>.Failure("CustomerId is required.", 400);
            }

            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                var normalizedIdempotencyKey = NormalizeOptional(request.IdempotencyKey);
                var existing = await _context.RewardRedemptions
                    .AsNoTracking()
                    .Include(redemption => redemption.PointTransactions)
                    .FirstOrDefaultAsync(redemption => redemption.PointTransactions.Any(transaction =>
                        transaction.IdempotencyKey != null &&
                        (transaction.IdempotencyKey == normalizedIdempotencyKey ||
                         transaction.IdempotencyKey.StartsWith(normalizedIdempotencyKey + ":"))));

                if (existing is not null)
                {
                    return OperationResult<RewardRedemptionResponse>.Success(MapRedemption(existing));
                }
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(item => item.CustomerID == request.CustomerId);
            if (customer is null)
            {
                return OperationResult<RewardRedemptionResponse>.Failure("Customer not found.", 404);
            }

            var reward = await _context.Rewards.FirstOrDefaultAsync(item => item.RewardID == rewardId);
            if (reward is null || reward.Status != RewardStatusEnum.Active)
            {
                return OperationResult<RewardRedemptionResponse>.Failure("Active reward not found.", 404);
            }

            var now = DateTime.UtcNow;
            if ((reward.ValidFrom.HasValue && reward.ValidFrom.Value > now) || (reward.ValidTo.HasValue && reward.ValidTo.Value < now))
            {
                return OperationResult<RewardRedemptionResponse>.Failure("Reward is not valid at this time.", 400);
            }

            if (reward.UsageLimitPerCustomer.HasValue)
            {
                var usedCount = await _context.RewardRedemptions.CountAsync(redemption =>
                    redemption.CustomerID == request.CustomerId &&
                    redemption.RewardID == rewardId &&
                    redemption.Status != RewardRedemptionStatusEnum.Cancelled);

                if (usedCount >= reward.UsageLimitPerCustomer.Value)
                {
                    return OperationResult<RewardRedemptionResponse>.Failure("Reward usage limit reached for this customer.", 400);
                }
            }

            if (customer.CurrentPoints < reward.PointsRequired)
            {
                return OperationResult<RewardRedemptionResponse>.Failure("Customer does not have enough points.", 400);
            }

            var earningTransactions = await _context.LoyaltyPointTransactions
                .Where(transaction =>
                    transaction.CustomerID == request.CustomerId &&
                    transaction.TransactionType == PointTransactionTypeEnum.Earn &&
                    transaction.RemainingPoints > 0 &&
                    (!transaction.ExpiryDate.HasValue || transaction.ExpiryDate.Value > now))
                .OrderBy(transaction => transaction.ExpiryDate ?? DateTime.MaxValue)
                .ThenBy(transaction => transaction.CreatedAt)
                .ToListAsync();

            var pointsToSpend = reward.PointsRequired;
            var redemption = new RewardRedemption
            {
                CustomerID = request.CustomerId,
                RewardID = rewardId,
                BookingID = request.BookingId,
                PointsSpent = reward.PointsRequired,
                Status = RewardRedemptionStatusEnum.Reserved,
                ExpiresAt = reward.ValidTo
            };

            _context.RewardRedemptions.Add(redemption);

            var baseIdempotencyKey = NormalizeOptional(request.IdempotencyKey);
            var transactionLine = 0;
            foreach (var earningTransaction in earningTransactions)
            {
                if (pointsToSpend <= 0)
                {
                    break;
                }

                var pointsFromTransaction = Math.Min(earningTransaction.RemainingPoints, pointsToSpend);
                earningTransaction.RemainingPoints -= pointsFromTransaction;
                pointsToSpend -= pointsFromTransaction;

                customer.CurrentPoints -= pointsFromTransaction;

                _context.LoyaltyPointTransactions.Add(new LoyaltyPointTransaction
                {
                    CustomerID = request.CustomerId,
                    BookingID = request.BookingId,
                    RedemptionID = redemption.RedemptionID,
                    ReferenceTransactionID = earningTransaction.TransactionID,
                    Points = -pointsFromTransaction,
                    OriginalPoints = 0,
                    RemainingPoints = 0,
                    BalanceAfter = customer.CurrentPoints,
                    TransactionType = PointTransactionTypeEnum.Redeem,
                    IdempotencyKey = BuildTransactionIdempotencyKey(baseIdempotencyKey, ++transactionLine),
                    Description = $"Redeemed reward {reward.RewardName}"
                });
            }

            if (pointsToSpend > 0 && customer.CurrentPoints >= pointsToSpend)
            {
                customer.CurrentPoints -= pointsToSpend;

                _context.LoyaltyPointTransactions.Add(new LoyaltyPointTransaction
                {
                    CustomerID = request.CustomerId,
                    BookingID = request.BookingId,
                    RedemptionID = redemption.RedemptionID,
                    Points = -pointsToSpend,
                    OriginalPoints = 0,
                    RemainingPoints = 0,
                    BalanceAfter = customer.CurrentPoints,
                    TransactionType = PointTransactionTypeEnum.Redeem,
                    IdempotencyKey = BuildTransactionIdempotencyKey(baseIdempotencyKey, ++transactionLine),
                    Description = $"Redeemed reward {reward.RewardName} from current point balance"
                });

                pointsToSpend = 0;
            }

            if (pointsToSpend > 0)
            {
                return OperationResult<RewardRedemptionResponse>.Failure("Customer does not have enough unexpired points.", 400);
            }

            customer.UpdatedAt = now;
            await _context.SaveChangesAsync();

            return OperationResult<RewardRedemptionResponse>.Success(MapRedemption(redemption), 201);
        }

        public async Task<IReadOnlyList<CustomerVoucherResponse>> GetCustomerVouchersAsync(Guid customerId)
        {
            var now = DateTime.UtcNow;
            var promotions = await _context.PromotionCustomers
                .AsNoTracking()
                .Include(item => item.Promotion)
                .Where(item => item.CustomerID == customerId)
                .OrderBy(item => item.ExpiresAt ?? item.Promotion.EndDate)
                .Select(item => new CustomerVoucherResponse
                {
                    Id = item.PromotionCustomerID,
                    SourceId = item.PromotionID,
                    Source = "Promotion",
                    Name = item.Promotion.PromotionName,
                    Description = item.Promotion.Description,
                    Type = item.Promotion.PromotionType.ToString(),
                    Value = item.Promotion.PromotionValue,
                    MaxDiscountAmount = item.Promotion.MaxDiscountAmount,
                    Status = item.IsUsed ? "Used" : item.Promotion.Status.ToString(),
                    ExpiresAt = item.ExpiresAt ?? item.Promotion.EndDate,
                    CanApply = !item.IsUsed && item.Promotion.Status == PromotionStatusEnum.Active &&
                               item.Promotion.StartDate <= now && (item.ExpiresAt ?? item.Promotion.EndDate) >= now
                })
                .ToListAsync();

            var rewards = await _context.RewardRedemptions
                .AsNoTracking()
                .Include(item => item.Reward)
                .Where(item => item.CustomerID == customerId)
                .OrderBy(item => item.ExpiresAt)
                .Select(item => new CustomerVoucherResponse
                {
                    Id = item.RedemptionID,
                    SourceId = item.RewardID,
                    Source = "Reward",
                    Name = item.Reward.RewardName,
                    Description = item.Reward.Description,
                    Type = item.Reward.RewardType.ToString(),
                    Value = item.Reward.RewardValue,
                    PointsSpent = item.PointsSpent,
                    Status = item.Status.ToString(),
                    ExpiresAt = item.ExpiresAt,
                    CanApply = item.Status == RewardRedemptionStatusEnum.Reserved && (!item.ExpiresAt.HasValue || item.ExpiresAt >= now)
                })
                .ToListAsync();

            return promotions.Concat(rewards)
                .OrderBy(item => item.ExpiresAt ?? DateTime.MaxValue)
                .ToList();
        }

        public async Task<CustomerLoyaltyOverviewResponse> GetMyLoyaltyOverviewAsync(Guid customerId)
        {
            var customer = await _context.Customers.AsNoTracking().Include(item => item.Tier).FirstOrDefaultAsync(item => item.CustomerID == customerId)
                ?? throw new InvalidOperationException("Customer not found.");
            var now = DateTime.UtcNow;
            var expiring = await _context.LoyaltyPointTransactions.AsNoTracking()
                .Where(item => item.CustomerID == customerId && item.RemainingPoints > 0 && item.ExpiryDate.HasValue && item.ExpiryDate >= now)
                .OrderBy(item => item.ExpiryDate).ToListAsync();
            var currentTierRank = customer.Tier is null ? 0 : customer.Tier.TierRank;
            var nextTier = await _context.LoyaltyTiers.AsNoTracking().Where(item => item.Status == LoyaltyTierStatusEnum.Active && item.TierRank > currentTierRank).OrderBy(item => item.TierRank).FirstOrDefaultAsync();
            var ledger = await GetPointHistoryAsync(customerId, 1, 10);
            return new CustomerLoyaltyOverviewResponse
            {
                Balance = MapBalance(customer), NextTierName = nextTier?.TierName, NextTierMinSpent = nextTier?.MinSpent ?? customer.TotalSpent,
                NextTierMinVisits = nextTier?.MinVisits ?? customer.TotalVisits, ExpiringPoints = expiring.Sum(item => item.RemainingPoints),
                NearestExpiryDate = expiring.FirstOrDefault()?.ExpiryDate, Vouchers = await GetCustomerVouchersAsync(customerId), RecentLedger = ledger.Items
            };
        }

        public async Task<IReadOnlyList<LoyaltySegmentCustomerResponse>> GetSegmentCustomersAsync(string segment, int inactiveDays, Guid? branchId, Guid? tierId)
        {
            inactiveDays = Math.Clamp(inactiveDays, 1, 3650);
            var now = DateTime.UtcNow;
            var query = _context.Customers.AsNoTracking().Include(item => item.User).Include(item => item.Tier).AsQueryable();
            if (tierId.HasValue) query = query.Where(item => item.TierID == tierId);
            if (branchId.HasValue) query = query.Where(item => item.Bookings.Any(booking => booking.BranchID == branchId));
            var customers = await query.ToListAsync();
            var ids = customers.Select(item => item.CustomerID).ToList();
            var expiring = await _context.LoyaltyPointTransactions.AsNoTracking()
                .Where(item => ids.Contains(item.CustomerID) && item.RemainingPoints > 0 && item.ExpiryDate.HasValue && item.ExpiryDate >= now && item.ExpiryDate <= now.AddDays(45))
                .GroupBy(item => item.CustomerID).Select(group => new { CustomerId = group.Key, Points = group.Sum(item => item.RemainingPoints), Expiry = group.Min(item => item.ExpiryDate) }).ToListAsync();
            var expiryByCustomer = expiring.ToDictionary(item => item.CustomerId);
            var selected = segment.ToLowerInvariant() switch
            {
                "at-risk" => customers.Where(item => item.LastVisitDate.HasValue && item.LastVisitDate.Value <= now.AddDays(-inactiveDays)),
                "expiring-points" => customers.Where(item => expiryByCustomer.ContainsKey(item.CustomerID)),
                "loyal" => customers.Where(item => item.TotalVisits > 0),
                _ => customers.AsEnumerable()
            };
            var result = selected.Select(item => MapSegmentCustomer(item, expiryByCustomer.TryGetValue(item.CustomerID, out var expiry) ? expiry.Points : 0, expiryByCustomer.TryGetValue(item.CustomerID, out expiry) ? expiry.Expiry : null));
            return segment.Equals("at-risk", StringComparison.OrdinalIgnoreCase)
                ? result.OrderBy(item => item.LastVisitDate).ToList()
                : result.OrderByDescending(item => item.LifetimePoints).ToList();
        }

        public async Task<OperationResult<LoyaltyCustomer360Response>> GetCustomer360Async(Guid customerId)
        {
            var customer = await _context.Customers.AsNoTracking().Include(item => item.User).Include(item => item.Tier).FirstOrDefaultAsync(item => item.CustomerID == customerId);
            if (customer is null) return OperationResult<LoyaltyCustomer360Response>.Failure("Customer not found.", 404);
            var expiry = await _context.LoyaltyPointTransactions.AsNoTracking().Where(item => item.CustomerID == customerId && item.RemainingPoints > 0 && item.ExpiryDate.HasValue).OrderBy(item => item.ExpiryDate).FirstOrDefaultAsync();
            var ledger = await GetPointHistoryAsync(customerId, 1, 20);
            var washHistory = await GetWashHistoryAsync(customerId, 1, 20);
            return OperationResult<LoyaltyCustomer360Response>.Success(new LoyaltyCustomer360Response { Customer = MapSegmentCustomer(customer, expiry?.RemainingPoints ?? 0, expiry?.ExpiryDate), PointLedger = ledger.Items, Vouchers = await GetCustomerVouchersAsync(customerId), WashHistory = washHistory.Items });
        }

        public async Task<PagedResult<PromotionResponse>> GetPromotionsAsync(int page, int pageSize, bool includeInactive)
        {
            page = NormalizePage(page);
            pageSize = NormalizePageSize(pageSize);

            var query = _context.Promotions
                .AsNoTracking()
                .Include(promotion => promotion.PromotionServices)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(promotion => promotion.Status == PromotionStatusEnum.Active);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(promotion => promotion.Priority)
                .ThenBy(promotion => promotion.PromotionName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(promotion => MapPromotion(promotion))
                .ToListAsync();

            return new PagedResult<PromotionResponse> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
        }

        public async Task<OperationResult<PromotionResponse>> GetPromotionAsync(Guid id)
        {
            var promotion = await _context.Promotions
                .AsNoTracking()
                .Include(item => item.PromotionServices)
                .FirstOrDefaultAsync(item => item.PromotionID == id);

            return promotion is null
                ? OperationResult<PromotionResponse>.Failure("Promotion not found.", 404)
                : OperationResult<PromotionResponse>.Success(MapPromotion(promotion));
        }

        public async Task<OperationResult<PromotionResponse>> CreatePromotionAsync(CreatePromotionRequest request)
        {
            var validation = await ValidatePromotionRequestAsync(request, null);
            if (validation is not null)
            {
                return OperationResult<PromotionResponse>.Failure(validation, 400);
            }

            var promotion = new Promotion();
            ApplyPromotionRequest(promotion, request);
            AddPromotionServices(promotion, request.ServiceIds);

            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();

            return OperationResult<PromotionResponse>.Success(MapPromotion(promotion), 201);
        }

        public async Task<OperationResult<PromotionResponse>> UpdatePromotionAsync(Guid id, UpdatePromotionRequest request)
        {
            var promotion = await _context.Promotions
                .Include(item => item.PromotionServices)
                .FirstOrDefaultAsync(item => item.PromotionID == id);
            if (promotion is null)
            {
                return OperationResult<PromotionResponse>.Failure("Promotion not found.", 404);
            }

            var validation = await ValidatePromotionRequestAsync(request, id);
            if (validation is not null)
            {
                return OperationResult<PromotionResponse>.Failure(validation, 400);
            }

            ApplyPromotionRequest(promotion, request);
            promotion.PromotionServices.Clear();
            AddPromotionServices(promotion, request.ServiceIds);

            await _context.SaveChangesAsync();
            return OperationResult<PromotionResponse>.Success(MapPromotion(promotion));
        }

        public async Task<OperationResult<bool>> DeletePromotionAsync(Guid id)
        {
            var promotion = await _context.Promotions.FirstOrDefaultAsync(item => item.PromotionID == id);
            if (promotion is null)
            {
                return OperationResult<bool>.Failure("Promotion not found.", 404);
            }

            promotion.Status = PromotionStatusEnum.Disabled;
            await _context.SaveChangesAsync();
            return OperationResult<bool>.Success(true);
        }

        public async Task<OperationResult<CampaignPreviewResponse>> PreviewCampaignAsync(CampaignRequest request)
        {
            var validation = await ValidatePromotionRequestAsync(request.Promotion, null);
            if (validation is not null) return OperationResult<CampaignPreviewResponse>.Failure(validation, 400);

            var promotion = new Promotion();
            ApplyPromotionRequest(promotion, request.Promotion);
            var audience = await ResolveCampaignAudienceAsync(promotion, request.Audience, false);
            if (audience.Error is not null) return OperationResult<CampaignPreviewResponse>.Failure(audience.Error, 400);

            return OperationResult<CampaignPreviewResponse>.Success(new CampaignPreviewResponse
            {
                EligibleCount = audience.CustomerIds.Count,
                ExcludedCount = audience.ExcludedCount,
                ExclusionReasons = audience.ExclusionReasons
            });
        }

        public async Task<OperationResult<PromotionDeliveryResponse>> CreateCampaignAsync(CampaignRequest request)
        {
            var validation = await ValidatePromotionRequestAsync(request.Promotion, null);
            if (validation is not null) return OperationResult<PromotionDeliveryResponse>.Failure(validation, 400);

            var promotion = new Promotion();
            ApplyPromotionRequest(promotion, request.Promotion);
            var audience = await ResolveCampaignAudienceAsync(promotion, request.Audience, false);
            if (audience.Error is not null) return OperationResult<PromotionDeliveryResponse>.Failure(audience.Error, 400);
            if (audience.CustomerIds.Count == 0) return OperationResult<PromotionDeliveryResponse>.Failure("No eligible customers were found.", 400);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            AddPromotionServices(promotion, request.Promotion.ServiceIds);
            _context.Promotions.Add(promotion);
            var now = DateTime.UtcNow;
            foreach (var customerId in audience.CustomerIds)
            {
                _context.PromotionCustomers.Add(new PromotionCustomer
                {
                    PromotionID = promotion.PromotionID,
                    CustomerID = customerId,
                    SentAt = now,
                    ExpiresAt = request.Audience.ExpiresAt ?? promotion.EndDate,
                    SentByUserID = _currentUserService.UserId,
                    AudienceSnapshot = JsonSerializer.Serialize(new { request.Audience.Segment, request.Audience.InactiveDays, request.Audience.BranchId, request.Audience.TierId, CustomerId = customerId }),
                    EligibilitySnapshot = JsonSerializer.Serialize(new { promotion.MinTierID, promotion.MinimumSpend, promotion.StartDate, promotion.EndDate, promotion.TotalUsageLimit, promotion.UsageLimitPerCustomer })
                });
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return OperationResult<PromotionDeliveryResponse>.Success(new PromotionDeliveryResponse { PromotionId = promotion.PromotionID, SentCount = audience.CustomerIds.Count, SkippedCount = audience.ExcludedCount }, 201);
        }

        public async Task<OperationResult<PromotionDeliveryResponse>> SendPromotionAsync(Guid promotionId, SendPromotionRequest request)
        {
            var promotion = await _context.Promotions.FirstOrDefaultAsync(item => item.PromotionID == promotionId);
            if (promotion is null)
            {
                return OperationResult<PromotionDeliveryResponse>.Failure("Promotion not found.", 404);
            }

            var audience = await ResolveCampaignAudienceAsync(promotion, request, true);
            if (audience.Error is not null) return OperationResult<PromotionDeliveryResponse>.Failure(audience.Error, 400);
            if (audience.CustomerIds.Count == 0) return OperationResult<PromotionDeliveryResponse>.Failure("No eligible customers were found.", 400);

            var now = DateTime.UtcNow;
            var sent = 0;
            foreach (var customerId in audience.CustomerIds)
            {
                _context.PromotionCustomers.Add(new PromotionCustomer
                {
                    PromotionID = promotionId,
                    CustomerID = customerId,
                    SentAt = now,
                    ExpiresAt = request.ExpiresAt ?? promotion.EndDate,
                    SentByUserID = _currentUserService.UserId,
                    AudienceSnapshot = JsonSerializer.Serialize(new { request.Segment, request.InactiveDays, request.BranchId, request.TierId, CustomerId = customerId }),
                    EligibilitySnapshot = JsonSerializer.Serialize(new { promotion.MinTierID, promotion.MinimumSpend, promotion.StartDate, promotion.EndDate, promotion.TotalUsageLimit, promotion.UsageLimitPerCustomer })
                });
                sent++;
            }

            await _context.SaveChangesAsync();

            return OperationResult<PromotionDeliveryResponse>.Success(new PromotionDeliveryResponse
            {
                PromotionId = promotionId,
                SentCount = sent,
                SkippedCount = audience.ExcludedCount
            });
        }

        public async Task<OperationResult<PromotionUsageResponse>> GetPromotionUsageAsync(Guid promotionId, Guid customerId)
        {
            if (customerId == Guid.Empty)
            {
                return OperationResult<PromotionUsageResponse>.Failure("CustomerId is required.", 400);
            }

            var promotion = await _context.Promotions
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.PromotionID == promotionId);
            if (promotion is null)
            {
                return OperationResult<PromotionUsageResponse>.Failure("Promotion not found.", 404);
            }

            var usedCount = await _context.BookingPromotions
                .AsNoTracking()
                .CountAsync(item => item.PromotionID == promotionId && item.Booking.CustomerID == customerId);
            var hasReachedLimit = promotion.UsageLimitPerCustomer.HasValue &&
                usedCount >= promotion.UsageLimitPerCustomer.Value;
            var label = promotion.PromotionCode ?? promotion.PromotionName;

            return OperationResult<PromotionUsageResponse>.Success(new PromotionUsageResponse
            {
                PromotionId = promotionId,
                CustomerId = customerId,
                UsedCount = usedCount,
                UsageLimitPerCustomer = promotion.UsageLimitPerCustomer,
                HasReachedLimit = hasReachedLimit,
                Message = hasReachedLimit
                    ? $"Mã khuyến mãi {label} đã được khách hàng này sử dụng."
                    : null
            });
        }

        public async Task<OperationResult<ApplyPromotionResponse>> ApplyPromotionAsync(Guid promotionId, ApplyPromotionRequest request)
        {
            if (request.BookingId == Guid.Empty || request.CustomerId == Guid.Empty)
            {
                return OperationResult<ApplyPromotionResponse>.Failure("BookingId and CustomerId are required.", 400);
            }

            var booking = await _context.Bookings
                .Include(item => item.Customer)
                .ThenInclude(customer => customer.Tier)
                .Include(item => item.BookingDetails)
                .Include(item => item.BookingPromotions)
                .FirstOrDefaultAsync(item => item.BookingID == request.BookingId);
            if (booking is null || booking.CustomerID != request.CustomerId)
            {
                return OperationResult<ApplyPromotionResponse>.Failure("Booking not found for this customer.", 404);
            }

            if (booking.BookingStatus is not (BookingStatusEnum.Pending or BookingStatusEnum.Confirmed))
            {
                return OperationResult<ApplyPromotionResponse>.Failure("Promotion can only be applied before the service starts.", 400);
            }

            if (booking.BookingPromotions.Any(item => item.PromotionID == promotionId))
            {
                return OperationResult<ApplyPromotionResponse>.Failure("Promotion already applied to this booking.", 409);
            }

            var promotion = await _context.Promotions
                .Include(item => item.PromotionServices)
                .Include(item => item.PromotionCustomers)
                .FirstOrDefaultAsync(item => item.PromotionID == promotionId);
            if (promotion is null)
            {
                return OperationResult<ApplyPromotionResponse>.Failure("Promotion not found.", 404);
            }

            var eligibilityError = await ValidatePromotionEligibilityAsync(promotion, booking, request.Code);
            if (eligibilityError is not null)
            {
                return OperationResult<ApplyPromotionResponse>.Failure(eligibilityError, 400);
            }

            var before = booking.EstimatedTotalAmount;
            var discount = CalculatePromotionDiscount(promotion, before);
            var bonusPoints = promotion.PromotionType == PromotionTypeEnum.BonusPoints ? promotion.BonusPoints : 0;
            var after = Math.Max(0, before - discount);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            _context.BookingPromotions.Add(new BookingPromotion
            {
                BookingID = booking.BookingID,
                PromotionID = promotion.PromotionID,
                DiscountAmount = discount,
                BonusPoints = bonusPoints
            });

            await _context.Bookings
                .Where(item => item.BookingID == booking.BookingID)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(item => item.EstimatedTotalAmount, after)
                    .SetProperty(item => item.UpdatedAt, DateTime.UtcNow));

            var sentPromotion = promotion.PromotionCustomers.FirstOrDefault(item => item.CustomerID == booking.CustomerID);
            if (sentPromotion is not null)
            {
                sentPromotion.UsageCount += 1;
                sentPromotion.IsUsed = true;
                sentPromotion.UsedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            booking.EstimatedTotalAmount = after;
            await _behavioralLogWriter.WriteAsync(new BehavioralLogWriteRequest
            {
                CustomerId = booking.CustomerID,
                BookingId = booking.BookingID,
                ServiceId = booking.BookingDetails.FirstOrDefault()?.ServiceID,
                PromotionId = promotion.PromotionID,
                ActionType = BehavioralActionTypeEnum.ViewPromotion,
                PromotionUsed = true,
                SpendingAmount = booking.EstimatedTotalAmount,
                Notes = "Promotion applied"
            });

            return OperationResult<ApplyPromotionResponse>.Success(new ApplyPromotionResponse
            {
                BookingId = booking.BookingID,
                PromotionId = promotion.PromotionID,
                DiscountAmount = discount,
                BonusPoints = bonusPoints,
                TotalBeforeDiscount = before,
                TotalAfterDiscount = after
            });
        }

        public async Task<OperationResult<PromotionAudiencePreviewResponse>> PreviewPromotionAudienceAsync(Guid promotionId, SendPromotionRequest request)
        {
            var promotion = await _context.Promotions.AsNoTracking().Include(item => item.MinTier).FirstOrDefaultAsync(item => item.PromotionID == promotionId);
            if (promotion is null) return OperationResult<PromotionAudiencePreviewResponse>.Failure("Promotion not found.", 404);
            var candidates = request.CustomerIds.Count > 0
                ? await GetSegmentCustomersAsync("all", request.InactiveDays, request.BranchId, request.TierId)
                : await GetSegmentCustomersAsync(request.Segment ?? "all", request.InactiveDays, request.BranchId, request.TierId);
            if (request.CustomerIds.Count > 0) candidates = candidates.Where(item => request.CustomerIds.Contains(item.CustomerId)).ToList();
            var audience = await ResolveCampaignAudienceAsync(promotion, request, true);
            if (audience.Error is not null) return OperationResult<PromotionAudiencePreviewResponse>.Failure(audience.Error, 400);
            var eligible = candidates.Where(item => audience.CustomerIds.Contains(item.CustomerId)).ToList();
            return OperationResult<PromotionAudiencePreviewResponse>.Success(new PromotionAudiencePreviewResponse { Customers = eligible.Take(250).ToList(), EligibleCount = audience.CustomerIds.Count, ExcludedCount = audience.ExcludedCount, ExclusionReasons = audience.ExclusionReasons });
        }

        public async Task<OperationResult<PromotionAnalyticsResponse>> GetPromotionAnalyticsAsync(Guid promotionId)
        {
            if (!await _context.Promotions.AnyAsync(item => item.PromotionID == promotionId)) return OperationResult<PromotionAnalyticsResponse>.Failure("Promotion not found.", 404);
            var recipients = await _context.PromotionCustomers.AsNoTracking().CountAsync(item => item.PromotionID == promotionId);
            var applied = await _context.BookingPromotions.AsNoTracking().Where(item => item.PromotionID == promotionId).ToListAsync();
            var used = await _context.PromotionCustomers.AsNoTracking().CountAsync(item => item.PromotionID == promotionId && item.IsUsed);
            var revenue = await _context.BookingPromotions.AsNoTracking().Where(item => item.PromotionID == promotionId).Select(item => item.Booking.EstimatedTotalAmount - item.DiscountAmount).SumAsync(item => (decimal?)item) ?? 0m;
            return OperationResult<PromotionAnalyticsResponse>.Success(new PromotionAnalyticsResponse { PromotionId = promotionId, RecipientCount = recipients, AppliedCount = applied.Count, UsedCount = used, RedemptionRate = recipients == 0 ? 0 : Math.Round((decimal)applied.Count / recipients * 100m, 2), DiscountValue = applied.Sum(item => item.DiscountAmount), BookingRevenueAfterDiscount = revenue });
        }

        public async Task<OperationResult<ApplyPromotionResponse>> RemovePromotionAsync(Guid promotionId, Guid bookingId, Guid customerId)
        {
            var booking = await _context.Bookings
                .Include(item => item.BookingPromotions)
                .FirstOrDefaultAsync(item => item.BookingID == bookingId && item.CustomerID == customerId);
            if (booking is null)
            {
                return OperationResult<ApplyPromotionResponse>.Failure("Booking not found for this customer.", 404);
            }

            if (booking.BookingStatus is not (BookingStatusEnum.Pending or BookingStatusEnum.Confirmed))
            {
                return OperationResult<ApplyPromotionResponse>.Failure("Voucher can only be removed before the service starts.", 400);
            }

            var applied = booking.BookingPromotions.FirstOrDefault(item => item.PromotionID == promotionId);
            if (applied is null)
            {
                return OperationResult<ApplyPromotionResponse>.Failure("Voucher is not applied to this booking.", 404);
            }

            var promotionCustomer = await _context.PromotionCustomers
                .FirstOrDefaultAsync(item => item.PromotionID == promotionId && item.CustomerID == customerId);

            booking.EstimatedTotalAmount += applied.DiscountAmount;
            booking.UpdatedAt = DateTime.UtcNow;
            _context.BookingPromotions.Remove(applied);
            if (promotionCustomer is not null)
            {
                promotionCustomer.UsageCount = Math.Max(0, promotionCustomer.UsageCount - 1);
                promotionCustomer.IsUsed = promotionCustomer.UsageCount > 0;
                promotionCustomer.UsedAt = promotionCustomer.IsUsed ? promotionCustomer.UsedAt : null;
            }

            await _context.SaveChangesAsync();
            return OperationResult<ApplyPromotionResponse>.Success(new ApplyPromotionResponse
            {
                BookingId = booking.BookingID,
                PromotionId = promotionId,
                DiscountAmount = 0,
                TotalBeforeDiscount = booking.EstimatedTotalAmount - applied.DiscountAmount,
                TotalAfterDiscount = booking.EstimatedTotalAmount
            });
        }

        public async Task<OperationResult<ApplyPromotionResponse>> ApplyRewardRedemptionAsync(Guid redemptionId, ApplyRewardRedemptionRequest request)
        {
            if (request.BookingId == Guid.Empty || request.CustomerId == Guid.Empty)
                return OperationResult<ApplyPromotionResponse>.Failure("BookingId and CustomerId are required.", 400);

            var redemption = await _context.RewardRedemptions
                .Include(item => item.Reward)
                .FirstOrDefaultAsync(item => item.RedemptionID == redemptionId && item.CustomerID == request.CustomerId);
            if (redemption is null) return OperationResult<ApplyPromotionResponse>.Failure("Reward voucher not found for this customer.", 404);
            if (redemption.Status != RewardRedemptionStatusEnum.Reserved) return OperationResult<ApplyPromotionResponse>.Failure("Reward voucher is not available.", 400);
            if (redemption.ExpiresAt.HasValue && redemption.ExpiresAt.Value < DateTime.UtcNow) return OperationResult<ApplyPromotionResponse>.Failure("Reward voucher has expired.", 400);
            if (redemption.BookingID.HasValue && redemption.BookingID.Value != request.BookingId) return OperationResult<ApplyPromotionResponse>.Failure("Reward voucher is reserved for another booking.", 409);

            var booking = await _context.Bookings
                .Include(item => item.BookingDetails)
                .Include(item => item.RewardRedemptions)
                .FirstOrDefaultAsync(item => item.BookingID == request.BookingId && item.CustomerID == request.CustomerId);
            if (booking is null) return OperationResult<ApplyPromotionResponse>.Failure("Booking not found for this customer.", 404);
            if (booking.BookingStatus is not (BookingStatusEnum.Pending or BookingStatusEnum.Confirmed)) return OperationResult<ApplyPromotionResponse>.Failure("Reward voucher can only be applied before the service starts.", 400);
            if (booking.RewardRedemptions.Any(item => item.Status == RewardRedemptionStatusEnum.Used)) return OperationResult<ApplyPromotionResponse>.Failure("Only one reward voucher can be applied to a booking.", 409);

            var before = booking.EstimatedTotalAmount;
            var discount = CalculateRewardDiscount(redemption.Reward, booking);
            if (discount <= 0) return OperationResult<ApplyPromotionResponse>.Failure("Reward is not eligible for the selected booking services.", 400);
            var after = Math.Max(0, before - discount);
            redemption.BookingID = booking.BookingID;
            redemption.Status = RewardRedemptionStatusEnum.Used;
            redemption.UsedAt = DateTime.UtcNow;
            redemption.AppliedDiscountAmount = discount;
            booking.EstimatedTotalAmount = after;
            booking.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return OperationResult<ApplyPromotionResponse>.Success(new ApplyPromotionResponse
            {
                BookingId = booking.BookingID,
                PromotionId = redemption.RedemptionID,
                DiscountAmount = discount,
                TotalBeforeDiscount = before,
                TotalAfterDiscount = after
            });
        }

        public async Task<OperationResult<ApplyPromotionResponse>> RemoveRewardRedemptionAsync(Guid redemptionId, Guid bookingId, Guid customerId)
        {
            var redemption = await _context.RewardRedemptions
                .Include(item => item.Booking)
                .FirstOrDefaultAsync(item => item.RedemptionID == redemptionId && item.CustomerID == customerId && item.BookingID == bookingId);
            if (redemption is null) return OperationResult<ApplyPromotionResponse>.Failure("Applied reward voucher not found.", 404);
            var booking = redemption.Booking;
            if (booking is null || redemption.Status != RewardRedemptionStatusEnum.Used || booking.BookingStatus is not (BookingStatusEnum.Pending or BookingStatusEnum.Confirmed))
                return OperationResult<ApplyPromotionResponse>.Failure("Reward voucher can only be removed from a pending booking.", 400);

            var before = booking.EstimatedTotalAmount;
            booking.EstimatedTotalAmount += redemption.AppliedDiscountAmount;
            booking.UpdatedAt = DateTime.UtcNow;
            redemption.Status = RewardRedemptionStatusEnum.Reserved;
            redemption.UsedAt = null;
            redemption.AppliedDiscountAmount = 0;
            await _context.SaveChangesAsync();
            return OperationResult<ApplyPromotionResponse>.Success(new ApplyPromotionResponse
            {
                BookingId = bookingId,
                PromotionId = redemptionId,
                DiscountAmount = 0,
                TotalBeforeDiscount = before,
                TotalAfterDiscount = booking.EstimatedTotalAmount
            });
        }

        public async Task<OperationResult<TierEvaluationResponse>> EvaluateTierAsync(Guid customerId)
        {
            var customer = await _context.Customers.Include(item => item.Tier).FirstOrDefaultAsync(item => item.CustomerID == customerId);
            if (customer is null)
            {
                return OperationResult<TierEvaluationResponse>.Failure("Customer not found.", 404);
            }

            var result = await EvaluateAndApplyTierAsync(customer, DateTime.UtcNow, TierChangeReasonEnum.ManualAdjustment);
            await _context.SaveChangesAsync();

            return OperationResult<TierEvaluationResponse>.Success(result);
        }

        public async Task<PagedResult<TierHistoryResponse>> GetTierHistoryAsync(Guid customerId, int page, int pageSize)
        {
            page = NormalizePage(page);
            pageSize = NormalizePageSize(pageSize);

            var query = _context.CustomerTierHistories
                .AsNoTracking()
                .Where(history => history.CustomerID == customerId);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(history => history.ChangedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(history => MapTierHistory(history))
                .ToListAsync();

            return new PagedResult<TierHistoryResponse> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
        }

        public async Task<LoyaltyDashboardResponse> GetDashboardAsync(DateTime? fromDate, DateTime? toDate)
        {
            var start = fromDate ?? DateTime.UtcNow.Date.AddDays(-30);
            var end = toDate ?? DateTime.UtcNow;

            var transactions = await _context.LoyaltyPointTransactions.AsNoTracking().Where(transaction => transaction.CreatedAt >= start && transaction.CreatedAt <= end).ToListAsync();
            var dailyTotals = transactions.GroupBy(item => item.CreatedAt.Date).ToDictionary(group => group.Key, group => new { Issued = group.Where(x => x.TransactionType == PointTransactionTypeEnum.Earn).Sum(x => x.Points), Redeemed = Math.Abs(group.Where(x => x.TransactionType == PointTransactionTypeEnum.Redeem).Sum(x => x.Points)) });
            var daily = Enumerable.Range(0, Math.Max(0, (end.Date - start.Date).Days + 1)).Select(offset =>
            {
                var date = start.Date.AddDays(offset);
                return dailyTotals.TryGetValue(date, out var total)
                    ? new LoyaltyDailyPointResponse { Date = date, Issued = total.Issued, Redeemed = total.Redeemed }
                    : new LoyaltyDailyPointResponse { Date = date, Issued = 0, Redeemed = 0 };
            }).ToList();
            var tierDistribution = await _context.Customers.AsNoTracking().Include(item => item.Tier).GroupBy(item => item.Tier == null ? "Chưa xếp hạng" : item.Tier.TierName).Select(item => new LoyaltyTierDistributionResponse { TierName = item.Key, CustomerCount = item.Count() }).ToListAsync();
            var expiringPoints = await _context.LoyaltyPointTransactions.AsNoTracking().Where(item => item.RemainingPoints > 0 && item.ExpiryDate.HasValue && item.ExpiryDate >= DateTime.UtcNow && item.ExpiryDate <= DateTime.UtcNow.AddDays(45)).SumAsync(item => (int?)item.RemainingPoints) ?? 0;
            var recipients = await _context.PromotionCustomers.CountAsync();
            var applied = await _context.BookingPromotions.CountAsync();
            return new LoyaltyDashboardResponse
            {
                ActiveCustomers = await _context.Customers.CountAsync(),
                ActiveRewards = await _context.Rewards.CountAsync(reward => reward.Status == RewardStatusEnum.Active),
                PointsIssued = transactions.Where(transaction => transaction.TransactionType == PointTransactionTypeEnum.Earn).Sum(transaction => transaction.Points),
                PointsRedeemed = Math.Abs(transactions.Where(transaction => transaction.TransactionType == PointTransactionTypeEnum.Redeem).Sum(transaction => transaction.Points)),
                Revenue = await _context.WashHistories
                    .Where(history => history.WashDate >= start && history.WashDate <= end)
                    .SumAsync(history => (decimal?)history.FinalAmount) ?? 0,
                CompletedWashes = await _context.WashHistories.CountAsync(history => history.WashDate >= start && history.WashDate <= end),
                ExpiringPoints = expiringPoints, PromotionUsageRate = recipients == 0 ? 0 : Math.Round((decimal)applied / recipients * 100m, 2), DailyPoints = daily, TierDistribution = tierDistribution
            };
        }

        public async Task<PagedResult<LoyaltyPointActivityResponse>> GetDashboardPointActivityAsync(string activity, int page, int pageSize, DateTime? fromDate, DateTime? toDate)
        {
            page = NormalizePage(page);
            pageSize = NormalizePageSize(pageSize);
            var start = fromDate ?? DateTime.UtcNow.Date.AddDays(-30);
            var end = toDate ?? DateTime.UtcNow;
            var transactionType = activity == "issued" ? PointTransactionTypeEnum.Earn : PointTransactionTypeEnum.Redeem;
            var query = _context.LoyaltyPointTransactions
                .AsNoTracking()
                .Include(transaction => transaction.Customer)
                .ThenInclude(customer => customer.User)
                .Where(transaction => transaction.TransactionType == transactionType && transaction.CreatedAt >= start && transaction.CreatedAt <= end);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(transaction => transaction.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(transaction => new LoyaltyPointActivityResponse
                {
                    TransactionId = transaction.TransactionID,
                    CustomerId = transaction.CustomerID,
                    CustomerName = transaction.Customer.User.FullName,
                    PhoneNumber = transaction.Customer.User.PhoneNumber,
                    Points = Math.Abs(transaction.Points),
                    Type = transaction.TransactionType.ToString(),
                    Description = transaction.Description,
                    CreatedAt = transaction.CreatedAt
                })
                .ToListAsync();

            return new PagedResult<LoyaltyPointActivityResponse> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
        }

        public async Task CompleteWashAsync(WashCompletionPayload payload)
        {
            var existingHistory = await _context.WashHistories.FirstOrDefaultAsync(history => history.BookingID == payload.BookingId);
            if (existingHistory is not null)
            {
                return;
            }

            var customer = await _context.Customers
                .Include(item => item.Tier)
                .FirstOrDefaultAsync(item => item.CustomerID == payload.CustomerId);

            if (customer is null)
            {
                return;
            }

            var idempotencyKey = $"wash:{payload.BookingId}:earn";
            var existingEarn = await _context.LoyaltyPointTransactions.AnyAsync(transaction => transaction.IdempotencyKey == idempotencyKey);
            if (existingEarn)
            {
                return;
            }

            var multiplier = customer.Tier?.PointMultiplier > 0 ? customer.Tier.PointMultiplier : 1m;
            var appliedPromotions = await _context.BookingPromotions
                .Where(item => item.BookingID == payload.BookingId)
                .ToListAsync();
            var discountAmount = appliedPromotions.Sum(item => item.DiscountAmount);
            var bonusPoints = appliedPromotions.Sum(item => item.BonusPoints);
            var pointsEarned = (int)Math.Floor(payload.Amount / PointEarnRateAmount * PointEarnRatePoints * multiplier) + bonusPoints;
            var completedAt = payload.CompletedAt == default ? DateTime.UtcNow : payload.CompletedAt;

            var history = new WashHistory
            {
                BookingID = payload.BookingId,
                WashDate = completedAt,
                ActualTotalAmount = payload.Amount + discountAmount,
                DiscountAmount = discountAmount,
                FinalAmount = payload.Amount,
                PointsEarned = pointsEarned,
                RewardUsed = discountAmount
            };

            _context.WashHistories.Add(history);

            customer.TotalSpent += payload.Amount;
            customer.TotalVisits += 1;
            customer.LastVisitDate = completedAt;
            customer.UpdatedAt = completedAt;

            if (pointsEarned > 0)
            {
                customer.CurrentPoints += pointsEarned;
                customer.LifetimePoints += pointsEarned;

                _context.LoyaltyPointTransactions.Add(new LoyaltyPointTransaction
                {
                    CustomerID = customer.CustomerID,
                    BookingID = payload.BookingId,
                    WashHistoryID = history.WashHistoryID,
                    Points = pointsEarned,
                    OriginalPoints = pointsEarned,
                    RemainingPoints = pointsEarned,
                    BalanceAfter = customer.CurrentPoints,
                    TransactionType = PointTransactionTypeEnum.Earn,
                    ExpiryDate = completedAt.AddMonths(PointExpiryMonths),
                    IdempotencyKey = idempotencyKey,
                    Description = "Points earned from completed wash"
                });
            }

            _context.BehavioralLogs.Add(new BehavioralLog
            {
                CustomerID = payload.CustomerId,
                BookingID = payload.BookingId,
                ServiceID = payload.ServiceId,
                ActionType = BehavioralActionTypeEnum.Book,
                ActionTime = completedAt,
                PointsChanged = pointsEarned,
                SpendingAmount = payload.Amount,
                RewardUsed = discountAmount,
                PromotionUsed = appliedPromotions.Count > 0,
                Notes = "Wash completed"
            });

            await EvaluateAndApplyTierAsync(customer, completedAt, TierChangeReasonEnum.MonthlyReview);
        }

        private async Task<TierEvaluationResponse> EvaluateAndApplyTierAsync(Customer customer, DateTime now, TierChangeReasonEnum reason)
        {
            var activeTiers = await _context.LoyaltyTiers
                .Where(tier => tier.Status == LoyaltyTierStatusEnum.Active)
                .OrderByDescending(tier => tier.TierRank)
                .ToListAsync();

            var qualifiedTier = activeTiers.FirstOrDefault(tier => Qualifies(customer, tier)) ??
                                activeTiers.OrderBy(tier => tier.TierRank).FirstOrDefault();

            if (qualifiedTier is null)
            {
                return new TierEvaluationResponse
                {
                    CustomerId = customer.CustomerID,
                    PreviousTierId = customer.TierID,
                    CurrentTierId = Guid.Empty,
                    QualifiedSpent = customer.TotalSpent,
                    QualifiedVisits = customer.TotalVisits,
                    Changed = false
                };
            }

            var previousTierId = customer.TierID;
            var changed = previousTierId != qualifiedTier.TierID;
            if (changed)
            {
                customer.TierID = qualifiedTier.TierID;
                customer.CurrentTierSince = now;
                customer.NextTierReviewAt = now.AddMonths(Math.Max(1, qualifiedTier.QualificationPeriodMonths));

                _context.CustomerTierHistories.Add(new CustomerTierHistory
                {
                    CustomerID = customer.CustomerID,
                    PreviousTierID = previousTierId,
                    NewTierID = qualifiedTier.TierID,
                    ReviewPeriodStart = now.AddMonths(-Math.Max(1, qualifiedTier.QualificationPeriodMonths)),
                    ReviewPeriodEnd = now,
                    QualifiedSpent = customer.TotalSpent,
                    QualifiedVisits = customer.TotalVisits,
                    ChangeReason = previousTierId.HasValue ? reason : TierChangeReasonEnum.InitialAssignment,
                    Notes = "Tier evaluated from loyalty activity."
                });
            }

            return new TierEvaluationResponse
            {
                CustomerId = customer.CustomerID,
                PreviousTierId = previousTierId,
                CurrentTierId = qualifiedTier.TierID,
                CurrentTierName = qualifiedTier.TierName,
                QualifiedSpent = customer.TotalSpent,
                QualifiedVisits = customer.TotalVisits,
                Changed = changed
            };
        }

        private static bool Qualifies(Customer customer, LoyaltyTier tier)
        {
            var spentQualified = customer.TotalSpent >= tier.MinSpent;
            var visitsQualified = customer.TotalVisits >= tier.MinVisits;
            return tier.QualificationMode == TierQualificationModeEnum.AnyCondition
                ? spentQualified || visitsQualified
                : spentQualified && visitsQualified;
        }

        private async Task<string?> ValidateTierRequestAsync(CreateLoyaltyTierRequest request, Guid? existingId)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return "Name is required.";
            }

            if (request.Rank < 1)
            {
                return "Rank must be greater than 0.";
            }

            if (request.MinSpent < 0 || request.MinVisits < 0)
            {
                return "MinSpent and MinVisits must be greater than or equal to 0.";
            }

            if (request.PointMultiplier <= 0 || request.BookingWindowDays < 1 || request.QualificationPeriodMonths < 1)
            {
                return "PointMultiplier, BookingWindowDays and QualificationPeriodMonths must be greater than 0.";
            }

            if (!Enum.TryParse<TierQualificationModeEnum>(request.QualificationMode, true, out _))
            {
                return "QualificationMode must be AllConditions or AnyCondition.";
            }

            var normalizedName = request.Name.Trim().ToLower();
            if (await _context.LoyaltyTiers.AnyAsync(tier => tier.TierID != existingId && tier.TierName.ToLower() == normalizedName))
            {
                return "Tier name already exists.";
            }

            if (await _context.LoyaltyTiers.AnyAsync(tier => tier.TierID != existingId && tier.TierRank == request.Rank))
            {
                return "Tier rank already exists.";
            }

            return null;
        }

        private async Task<string?> ValidateRewardRequestAsync(CreateRewardRequest request, Guid? existingId)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return "Name is required.";
            }

            if (request.PointsRequired <= 0)
            {
                return "PointsRequired must be greater than 0.";
            }

            if (request.Value < 0)
            {
                return "Value must be greater than or equal to 0.";
            }

            if (request.ValidFrom.HasValue && request.ValidTo.HasValue && request.ValidTo <= request.ValidFrom)
            {
                return "ValidTo must be later than ValidFrom.";
            }

            if (request.UsageLimitPerCustomer.HasValue && request.UsageLimitPerCustomer <= 0)
            {
                return "UsageLimitPerCustomer must be greater than 0.";
            }

            if (!Enum.TryParse<RewardTypeEnum>(request.Type, true, out _))
            {
                return "Type must be FixedDiscount, PercentageDiscount, FreeService or AddOnService.";
            }

            var rewardType = Enum.Parse<RewardTypeEnum>(request.Type, true);
            if ((rewardType is RewardTypeEnum.FreeService or RewardTypeEnum.AddOnService) && !request.ServiceId.HasValue)
            {
                return "ServiceId is required for FreeService and AddOnService rewards.";
            }

            if (request.ServiceId.HasValue && !await _context.Services.AnyAsync(service => service.ServiceID == request.ServiceId.Value))
            {
                return "Service not found.";
            }

            var normalizedName = request.Name.Trim().ToLower();
            if (await _context.Rewards.AnyAsync(reward => reward.RewardID != existingId && reward.RewardName.ToLower() == normalizedName))
            {
                return "Reward name already exists.";
            }

            return null;
        }

        private static void ApplyTierRequest(LoyaltyTier tier, CreateLoyaltyTierRequest request)
        {
            tier.TierName = request.Name.Trim();
            tier.TierRank = request.Rank;
            tier.MinSpent = request.MinSpent;
            tier.MinVisits = request.MinVisits;
            tier.QualificationPeriodMonths = request.QualificationPeriodMonths;
            tier.QualificationMode = Enum.Parse<TierQualificationModeEnum>(request.QualificationMode, true);
            tier.BookingWindowDays = request.BookingWindowDays;
            tier.PriorityLevel = request.PriorityLevel;
            tier.PointMultiplier = request.PointMultiplier;
            tier.TierBenefits = NormalizeOptional(request.Benefits);
            tier.Status = request.IsActive ? LoyaltyTierStatusEnum.Active : LoyaltyTierStatusEnum.Inactive;
        }

        private static void ApplyRewardRequest(Reward reward, CreateRewardRequest request)
        {
            reward.RewardName = request.Name.Trim();
            reward.Description = NormalizeOptional(request.Description);
            reward.RewardType = Enum.Parse<RewardTypeEnum>(request.Type, true);
            reward.PointsRequired = request.PointsRequired;
            reward.RewardValue = request.Value;
            reward.ServiceID = request.ServiceId;
            reward.ValidFrom = request.ValidFrom;
            reward.ValidTo = request.ValidTo;
            reward.UsageLimitPerCustomer = request.UsageLimitPerCustomer;
            reward.Status = request.IsActive ? RewardStatusEnum.Active : RewardStatusEnum.Inactive;
        }

        private async Task<CampaignAudienceResolution> ResolveCampaignAudienceAsync(Promotion promotion, SendPromotionRequest request, bool excludeAlreadySent)
        {
            var now = DateTime.UtcNow;
            if (promotion.Status != PromotionStatusEnum.Active) return CampaignAudienceResolution.Fail("Promotion is not active.");
            if (promotion.StartDate > now) return CampaignAudienceResolution.Fail("Promotion has not started yet.");
            if (promotion.EndDate < now) return CampaignAudienceResolution.Fail("Promotion has expired.");
            if (promotion.TotalUsageLimit.HasValue && await _context.BookingPromotions.CountAsync(item => item.PromotionID == promotion.PromotionID) >= promotion.TotalUsageLimit.Value)
                return CampaignAudienceResolution.Fail("Promotion usage limit reached.");

            var requestedIds = request.CustomerIds.Where(id => id != Guid.Empty).Distinct().ToList();
            if (requestedIds.Count == 0)
            {
                if (string.IsNullOrWhiteSpace(request.Segment)) return CampaignAudienceResolution.Fail("At least one customer or segment is required.");
                requestedIds = (await GetSegmentCustomersAsync(request.Segment, request.InactiveDays, request.BranchId, request.TierId)).Select(item => item.CustomerId).ToList();
            }
            if (requestedIds.Count == 0) return new CampaignAudienceResolution(Array.Empty<Guid>(), 0, Array.Empty<string>(), null);

            var minTierRank = promotion.MinTierID.HasValue
                ? await _context.LoyaltyTiers.Where(tier => tier.TierID == promotion.MinTierID.Value).Select(tier => (int?)tier.TierRank).FirstOrDefaultAsync()
                : null;
            var candidates = await _context.Customers.AsNoTracking().Include(customer => customer.Tier)
                .Where(customer => requestedIds.Contains(customer.CustomerID)).ToListAsync();
            var eligible = candidates.Where(customer => !minTierRank.HasValue || customer.Tier is not null && customer.Tier.TierRank >= minTierRank.Value)
                .Select(customer => customer.CustomerID).ToList();
            var excluded = requestedIds.Count - eligible.Count;
            var reasons = excluded > 0 ? new[] { "Không đủ hạng thành viên hoặc khách hàng không tồn tại." } : Array.Empty<string>();

            if (excludeAlreadySent && eligible.Count > 0)
            {
                var alreadySent = await _context.PromotionCustomers.Where(item => item.PromotionID == promotion.PromotionID && eligible.Contains(item.CustomerID)).Select(item => item.CustomerID).ToListAsync();
                if (alreadySent.Count > 0)
                {
                    eligible = eligible.Except(alreadySent).ToList();
                    excluded += alreadySent.Count;
                    reasons = reasons.Append("Khách hàng đã nhận campaign này.").ToArray();
                }
            }
            return new CampaignAudienceResolution(eligible, excluded, reasons, null);
        }

        private sealed record CampaignAudienceResolution(IReadOnlyList<Guid> CustomerIds, int ExcludedCount, IReadOnlyList<string> ExclusionReasons, string? Error)
        {
            public static CampaignAudienceResolution Fail(string error) => new(Array.Empty<Guid>(), 0, Array.Empty<string>(), error);
        }

        private async Task<string?> ValidatePromotionRequestAsync(CreatePromotionRequest request, Guid? existingId)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return "Name is required.";
            }

            if (!Enum.TryParse<PromotionTypeEnum>(request.Type, true, out _))
            {
                return "Type must be PercentageDiscount, FixedDiscount, FreeService or BonusPoints.";
            }

            if (request.Value < 0 || request.MinimumSpend < 0 || request.BonusPoints < 0)
            {
                return "Value, MinimumSpend and BonusPoints must be greater than or equal to 0.";
            }

            if (request.EndDate <= request.StartDate)
            {
                return "EndDate must be later than StartDate.";
            }

            if (request.TotalUsageLimit.HasValue && request.TotalUsageLimit <= 0)
            {
                return "TotalUsageLimit must be greater than 0.";
            }

            if (request.UsageLimitPerCustomer.HasValue && request.UsageLimitPerCustomer <= 0)
            {
                return "UsageLimitPerCustomer must be greater than 0.";
            }

            var normalizedCode = NormalizeOptional(request.Code);
            if (normalizedCode is not null &&
                await _context.Promotions.AnyAsync(promotion => promotion.PromotionID != existingId && promotion.PromotionCode == normalizedCode))
            {
                return "Promotion code already exists.";
            }

            if (request.MinTierId.HasValue && !await _context.LoyaltyTiers.AnyAsync(tier => tier.TierID == request.MinTierId.Value))
            {
                return "Minimum tier not found.";
            }

            if (request.FreeServiceId.HasValue && !await _context.Services.AnyAsync(service => service.ServiceID == request.FreeServiceId.Value))
            {
                return "Free service not found.";
            }

            var serviceIds = request.ServiceIds.Where(id => id != Guid.Empty).Distinct().ToList();
            if (serviceIds.Count > 0)
            {
                var existingServiceCount = await _context.Services.CountAsync(service => serviceIds.Contains(service.ServiceID));
                if (existingServiceCount != serviceIds.Count)
                {
                    return "One or more services were not found.";
                }
            }

            return null;
        }

        private static void ApplyPromotionRequest(Promotion promotion, CreatePromotionRequest request)
        {
            promotion.PromotionName = request.Name.Trim();
            promotion.PromotionCode = NormalizeOptional(request.Code);
            promotion.Description = NormalizeOptional(request.Description);
            promotion.PromotionType = Enum.Parse<PromotionTypeEnum>(request.Type, true);
            promotion.PromotionValue = request.Value;
            promotion.MaxDiscountAmount = request.MaxDiscountAmount;
            promotion.BonusPoints = request.BonusPoints;
            promotion.FreeServiceID = request.FreeServiceId;
            promotion.MinimumSpend = request.MinimumSpend;
            promotion.StartDate = request.StartDate;
            promotion.EndDate = request.EndDate;
            promotion.MinTierID = request.MinTierId;
            promotion.TotalUsageLimit = request.TotalUsageLimit;
            promotion.UsageLimitPerCustomer = request.UsageLimitPerCustomer;
            promotion.Priority = request.Priority;
            promotion.IsStackable = request.IsStackable;
            promotion.Status = request.IsActive ? PromotionStatusEnum.Active : PromotionStatusEnum.Disabled;
        }

        private static void AddPromotionServices(Promotion promotion, IEnumerable<Guid> serviceIds)
        {
            foreach (var serviceId in serviceIds.Where(id => id != Guid.Empty).Distinct())
            {
                promotion.PromotionServices.Add(new PromotionService
                {
                    PromotionID = promotion.PromotionID,
                    ServiceID = serviceId
                });
            }
        }

        private async Task<string?> ValidatePromotionEligibilityAsync(Promotion promotion, Booking booking, string? code)
        {
            var now = DateTime.UtcNow;
            if (promotion.Status != PromotionStatusEnum.Active || promotion.StartDate > now || promotion.EndDate < now)
            {
                return "Promotion is not active.";
            }

            var wasSentToCustomer = promotion.PromotionCustomers.Any(item => item.CustomerID == booking.CustomerID);
            if (!wasSentToCustomer &&
                !string.IsNullOrWhiteSpace(promotion.PromotionCode) &&
                !string.Equals(promotion.PromotionCode, NormalizeOptional(code), StringComparison.OrdinalIgnoreCase))
            {
                return "Promotion code is invalid.";
            }

            if (promotion.MinimumSpend > booking.EstimatedTotalAmount)
            {
                return "Booking amount does not meet promotion minimum spend.";
            }

            var serviceId = booking.BookingDetails.FirstOrDefault()?.ServiceID;
            if (promotion.PromotionServices.Count > 0 &&
                (!serviceId.HasValue || promotion.PromotionServices.All(item => item.ServiceID != serviceId.Value)))
            {
                return "Promotion does not apply to this service.";
            }

            if (promotion.MinTierID.HasValue && booking.Customer.Tier is not null)
            {
                var minTier = await _context.LoyaltyTiers.AsNoTracking().FirstOrDefaultAsync(tier => tier.TierID == promotion.MinTierID.Value);
                if (minTier is not null && booking.Customer.Tier.TierRank < minTier.TierRank)
                {
                    return "Customer tier does not qualify for this promotion.";
                }
            }
            else if (promotion.MinTierID.HasValue)
            {
                return "Customer tier does not qualify for this promotion.";
            }

            if (promotion.TotalUsageLimit.HasValue)
            {
                var totalUsage = await _context.BookingPromotions.CountAsync(item => item.PromotionID == promotion.PromotionID);
                if (totalUsage >= promotion.TotalUsageLimit.Value)
                {
                    return "Promotion usage limit reached.";
                }
            }

            if (promotion.UsageLimitPerCustomer.HasValue)
            {
                var customerUsage = await _context.BookingPromotions
                    .CountAsync(item => item.PromotionID == promotion.PromotionID && item.Booking.CustomerID == booking.CustomerID);
                if (customerUsage >= promotion.UsageLimitPerCustomer.Value)
                {
                    return $"Mã khuyến mãi {promotion.PromotionCode ?? promotion.PromotionName} đã được khách hàng này sử dụng.";
                }
            }

            var sentPromotion = promotion.PromotionCustomers.FirstOrDefault(item => item.CustomerID == booking.CustomerID);
            if (sentPromotion is not null && sentPromotion.ExpiresAt.HasValue && sentPromotion.ExpiresAt.Value < now)
            {
                return "Promotion delivery expired for this customer.";
            }

            return null;
        }

        private static decimal CalculatePromotionDiscount(Promotion promotion, decimal amount)
        {
            var discount = promotion.PromotionType switch
            {
                PromotionTypeEnum.PercentageDiscount => amount * promotion.PromotionValue / 100m,
                PromotionTypeEnum.FixedDiscount => promotion.PromotionValue,
                _ => 0m
            };

            if (promotion.MaxDiscountAmount.HasValue)
            {
                discount = Math.Min(discount, promotion.MaxDiscountAmount.Value);
            }

            return Math.Min(amount, Math.Max(0, discount));
        }

        private static LoyaltyTierResponse MapTier(LoyaltyTier tier)
        {
            return new LoyaltyTierResponse
            {
                Id = tier.TierID,
                Name = tier.TierName,
                Rank = tier.TierRank,
                MinSpent = tier.MinSpent,
                MinVisits = tier.MinVisits,
                QualificationPeriodMonths = tier.QualificationPeriodMonths,
                QualificationMode = tier.QualificationMode.ToString(),
                BookingWindowDays = tier.BookingWindowDays,
                PriorityLevel = tier.PriorityLevel,
                PointMultiplier = tier.PointMultiplier,
                Benefits = tier.TierBenefits,
                Status = tier.Status.ToString()
            };
        }

        private static PromotionResponse MapPromotion(Promotion promotion)
        {
            return new PromotionResponse
            {
                Id = promotion.PromotionID,
                Name = promotion.PromotionName,
                Code = promotion.PromotionCode,
                Description = promotion.Description,
                Type = promotion.PromotionType.ToString(),
                Value = promotion.PromotionValue,
                MaxDiscountAmount = promotion.MaxDiscountAmount,
                BonusPoints = promotion.BonusPoints,
                FreeServiceId = promotion.FreeServiceID,
                MinimumSpend = promotion.MinimumSpend,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                MinTierId = promotion.MinTierID,
                TotalUsageLimit = promotion.TotalUsageLimit,
                UsageLimitPerCustomer = promotion.UsageLimitPerCustomer,
                Priority = promotion.Priority,
                IsStackable = promotion.IsStackable,
                Status = promotion.Status.ToString(),
                CreatedAt = promotion.CreatedAt,
                ServiceIds = promotion.PromotionServices.Select(item => item.ServiceID).ToList()
            };
        }

        private static PointBalanceResponse MapBalance(Customer customer)
        {
            return new PointBalanceResponse
            {
                CustomerId = customer.CustomerID,
                CurrentPoints = customer.CurrentPoints,
                LifetimePoints = customer.LifetimePoints,
                CurrentTier = customer.Tier?.TierName,
                TotalSpent = customer.TotalSpent,
                TotalVisits = customer.TotalVisits
            };
        }

        private static PointTransactionResponse MapPointTransaction(LoyaltyPointTransaction transaction)
        {
            return new PointTransactionResponse
            {
                Id = transaction.TransactionID,
                CustomerId = transaction.CustomerID,
                BookingId = transaction.BookingID,
                WashHistoryId = transaction.WashHistoryID,
                RedemptionId = transaction.RedemptionID,
                Points = transaction.Points,
                OriginalPoints = transaction.OriginalPoints,
                RemainingPoints = transaction.RemainingPoints,
                BalanceAfter = transaction.BalanceAfter,
                Type = transaction.TransactionType.ToString(),
                ExpiryDate = transaction.ExpiryDate,
                IdempotencyKey = transaction.IdempotencyKey,
                Description = transaction.Description,
                CreatedAt = transaction.CreatedAt
            };
        }

        private static WashHistoryResponse MapWashHistory(WashHistory history)
        {
            return new WashHistoryResponse
            {
                Id = history.WashHistoryID,
                BookingId = history.BookingID,
                WashDate = history.WashDate,
                ActualTotalAmount = history.ActualTotalAmount,
                DiscountAmount = history.DiscountAmount,
                FinalAmount = history.FinalAmount,
                PointsEarned = history.PointsEarned,
                RewardUsed = history.RewardUsed,
                CustomerRating = history.CustomerRating,
                Feedback = history.Feedback,
                CreatedAt = history.CreatedAt
            };
        }

        private static RewardResponse MapReward(Reward reward)
        {
            return new RewardResponse
            {
                Id = reward.RewardID,
                Name = reward.RewardName,
                Description = reward.Description,
                Type = reward.RewardType.ToString(),
                PointsRequired = reward.PointsRequired,
                Value = reward.RewardValue,
                ServiceId = reward.ServiceID,
                ValidFrom = reward.ValidFrom,
                ValidTo = reward.ValidTo,
                UsageLimitPerCustomer = reward.UsageLimitPerCustomer,
                Status = reward.Status.ToString(),
                CreatedAt = reward.CreatedAt
            };
        }

        private static RewardRedemptionResponse MapRedemption(RewardRedemption redemption)
        {
            return new RewardRedemptionResponse
            {
                Id = redemption.RedemptionID,
                CustomerId = redemption.CustomerID,
                RewardId = redemption.RewardID,
                BookingId = redemption.BookingID,
                PointsSpent = redemption.PointsSpent,
                Status = redemption.Status.ToString(),
                RedeemedAt = redemption.RedeemedAt,
                ExpiresAt = redemption.ExpiresAt,
                UsedAt = redemption.UsedAt,
                AppliedDiscountAmount = redemption.AppliedDiscountAmount
            };
        }

        private static LoyaltySegmentCustomerResponse MapSegmentCustomer(Customer customer, int expiringPoints, DateTime? nearestExpiryDate)
        {
            return new LoyaltySegmentCustomerResponse { CustomerId = customer.CustomerID, FullName = customer.User.FullName, PhoneNumber = customer.User.PhoneNumber, TierName = customer.Tier?.TierName, CurrentPoints = customer.CurrentPoints, LifetimePoints = customer.LifetimePoints, LastVisitDate = customer.LastVisitDate, NearestExpiryDate = nearestExpiryDate, ExpiringPoints = expiringPoints, TotalSpent = customer.TotalSpent };
        }

        private static decimal CalculateRewardDiscount(Reward reward, Booking booking)
        {
            return reward.RewardType switch
            {
                RewardTypeEnum.FixedDiscount => Math.Min(booking.EstimatedTotalAmount, reward.RewardValue),
                RewardTypeEnum.PercentageDiscount => Math.Min(booking.EstimatedTotalAmount, booking.EstimatedTotalAmount * reward.RewardValue / 100m),
                RewardTypeEnum.FreeService or RewardTypeEnum.AddOnService when reward.ServiceID.HasValue => booking.BookingDetails
                    .Where(item => item.ServiceID == reward.ServiceID.Value)
                    .Sum(item => item.UnitPrice * item.Quantity),
                _ => 0m
            };
        }

        private static TierHistoryResponse MapTierHistory(CustomerTierHistory history)
        {
            return new TierHistoryResponse
            {
                Id = history.CustomerTierHistoryID,
                CustomerId = history.CustomerID,
                PreviousTierId = history.PreviousTierID,
                NewTierId = history.NewTierID,
                ReviewPeriodStart = history.ReviewPeriodStart,
                ReviewPeriodEnd = history.ReviewPeriodEnd,
                QualifiedSpent = history.QualifiedSpent,
                QualifiedVisits = history.QualifiedVisits,
                ChangeReason = history.ChangeReason.ToString(),
                ChangedAt = history.ChangedAt,
                Notes = history.Notes
            };
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string? BuildTransactionIdempotencyKey(string? baseKey, int line)
        {
            if (string.IsNullOrWhiteSpace(baseKey))
            {
                return null;
            }

            var key = line <= 1 ? baseKey : $"{baseKey}:{line}";
            return key.Length <= 100 ? key : key[..100];
        }

        private static int NormalizePage(int page)
        {
            return page < 1 ? 1 : page;
        }

        private static int NormalizePageSize(int pageSize)
        {
            return pageSize switch
            {
                < 1 => 20,
                > 100 => 100,
                _ => pageSize
            };
        }
    }
}
