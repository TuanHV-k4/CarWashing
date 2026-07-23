using BusinessLayer.Dtos.Loyalty;
using BusinessLayer.Dtos.Operations;
using BusinessLayer.IService;
using BusinessLayer.IService.Loyalty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Loyalty
{
    [ApiController]
    [Route("api/loyalty")]
    public class LoyaltyController : ControllerBase
    {
        private readonly ILoyaltyService _loyaltyService;
        private readonly ICurrentCustomerService _currentCustomerService;

        public LoyaltyController(ILoyaltyService loyaltyService, ICurrentCustomerService currentCustomerService)
        {
            _loyaltyService = loyaltyService;
            _currentCustomerService = currentCustomerService;
        }

        [Authorize(Policy = "CustomerOnly")]
        [HttpGet("me/vouchers")]
        public async Task<ActionResult<IReadOnlyList<CustomerVoucherResponse>>> GetMyVouchers()
        {
            return Ok(await _loyaltyService.GetCustomerVouchersAsync(await _currentCustomerService.GetCurrentCustomerIdAsync()));
        }

        [Authorize(Policy = "CustomerOnly")]
        [HttpGet("me/overview")]
        public async Task<ActionResult<CustomerLoyaltyOverviewResponse>> GetMyOverview()
        {
            return Ok(await _loyaltyService.GetMyLoyaltyOverviewAsync(await _currentCustomerService.GetCurrentCustomerIdAsync()));
        }

        [Authorize(Policy = "CustomerOnly")]
        [HttpPost("me/rewards/{id:guid}/redeem")]
        public async Task<ActionResult> RedeemMyReward(Guid id, RedeemRewardRequest request)
        {
            request.CustomerId = await _currentCustomerService.GetCurrentCustomerIdAsync();
            return FromResult(await _loyaltyService.RedeemRewardAsync(id, request));
        }

        [Authorize(Policy = "CustomerOnly")]
        [HttpPost("me/promotions/{id:guid}/apply")]
        public async Task<ActionResult> ApplyMyPromotion(Guid id, ApplyPromotionRequest request)
        {
            request.CustomerId = await _currentCustomerService.GetCurrentCustomerIdAsync();
            return FromResult(await _loyaltyService.ApplyPromotionAsync(id, request));
        }

        [Authorize(Policy = "CustomerOnly")]
        [HttpPost("me/reward-redemptions/{id:guid}/apply")]
        public async Task<ActionResult> ApplyMyRewardRedemption(Guid id, ApplyRewardRedemptionRequest request)
        {
            request.CustomerId = await _currentCustomerService.GetCurrentCustomerIdAsync();
            return FromResult(await _loyaltyService.ApplyRewardRedemptionAsync(id, request));
        }

        [Authorize(Policy = "CustomerOnly")]
        [HttpDelete("me/promotions/{id:guid}/bookings/{bookingId:guid}")]
        public async Task<ActionResult> RemoveMyPromotion(Guid id, Guid bookingId)
        {
            return FromResult(await _loyaltyService.RemovePromotionAsync(id, bookingId, await _currentCustomerService.GetCurrentCustomerIdAsync()));
        }

        [Authorize(Policy = "CustomerOnly")]
        [HttpDelete("me/reward-redemptions/{id:guid}/bookings/{bookingId:guid}")]
        public async Task<ActionResult> RemoveMyRewardRedemption(Guid id, Guid bookingId)
        {
            return FromResult(await _loyaltyService.RemoveRewardRedemptionAsync(id, bookingId, await _currentCustomerService.GetCurrentCustomerIdAsync()));
        }

        [HttpGet("settings")]
        public async Task<ActionResult<LoyaltySettingsResponse>> GetSettings()
        {
            return Ok(await _loyaltyService.GetSettingsAsync());
        }

        [HttpGet("tiers")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<PagedResult<LoyaltyTierResponse>>> GetTiers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool includeInactive = false)
        {
            return Ok(await _loyaltyService.GetTiersAsync(page, pageSize, includeInactive));
        }

        [HttpGet("tiers/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> GetTier(Guid id)
        {
            return FromResult(await _loyaltyService.GetTierAsync(id));
        }

        [HttpPost("tiers")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> CreateTier(CreateLoyaltyTierRequest request)
        {
            return FromResult(await _loyaltyService.CreateTierAsync(request));
        }

        [HttpPut("tiers/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> UpdateTier(Guid id, UpdateLoyaltyTierRequest request)
        {
            return FromResult(await _loyaltyService.UpdateTierAsync(id, request));
        }

        [HttpDelete("tiers/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> DeleteTier(Guid id)
        {
            return FromResult(await _loyaltyService.DeleteTierAsync(id));
        }

        [HttpGet("customers/{customerId:guid}/points/balance")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> GetPointBalance(Guid customerId)
        {
            return FromResult(await _loyaltyService.GetPointBalanceAsync(customerId));
        }

        [HttpGet("customers/{customerId:guid}/points/history")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<PagedResult<PointTransactionResponse>>> GetPointHistory(
            Guid customerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            return Ok(await _loyaltyService.GetPointHistoryAsync(customerId, page, pageSize));
        }

        [HttpGet("wash-history")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<PagedResult<WashHistoryResponse>>> GetWashHistory(
            [FromQuery] Guid? customerId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            return Ok(await _loyaltyService.GetWashHistoryAsync(customerId, page, pageSize));
        }

        [HttpGet("rewards")]
        public async Task<ActionResult<PagedResult<RewardResponse>>> GetRewards(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool includeInactive = false)
        {
            return Ok(await _loyaltyService.GetRewardsAsync(page, pageSize, includeInactive));
        }

        [HttpGet("rewards/{id:guid}")]
        public async Task<ActionResult> GetReward(Guid id)
        {
            return FromResult(await _loyaltyService.GetRewardAsync(id));
        }

        [HttpPost("rewards")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> CreateReward(CreateRewardRequest request)
        {
            return FromResult(await _loyaltyService.CreateRewardAsync(request));
        }

        [HttpPut("rewards/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> UpdateReward(Guid id, UpdateRewardRequest request)
        {
            return FromResult(await _loyaltyService.UpdateRewardAsync(id, request));
        }

        [HttpDelete("rewards/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> DeleteReward(Guid id)
        {
            return FromResult(await _loyaltyService.DeleteRewardAsync(id));
        }

        [HttpPost("rewards/{id:guid}/redeem")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> RedeemReward(Guid id, RedeemRewardRequest request)
        {
            if (request.CustomerId == Guid.Empty)
            {
                return BadRequest("CustomerId is required.");
            }

            return FromResult(await _loyaltyService.RedeemRewardAsync(id, request));
        }

        [HttpPost("customers/{customerId:guid}/tier/evaluate")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> EvaluateTier(Guid customerId)
        {
            return FromResult(await _loyaltyService.EvaluateTierAsync(customerId));
        }

        [HttpGet("customers/{customerId:guid}/tier/history")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<PagedResult<TierHistoryResponse>>> GetTierHistory(
            Guid customerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            return Ok(await _loyaltyService.GetTierHistoryAsync(customerId, page, pageSize));
        }

        [HttpGet("dashboard")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<LoyaltyDashboardResponse>> GetDashboard(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            return Ok(await _loyaltyService.GetDashboardAsync(fromDate, toDate));
        }

        [HttpGet("dashboard/point-activity")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<PagedResult<LoyaltyPointActivityResponse>>> GetDashboardPointActivity(
            [FromQuery] string activity,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            if (activity is not ("issued" or "redeemed")) return BadRequest(new ProblemDetails { Detail = "activity must be issued or redeemed." });
            return Ok(await _loyaltyService.GetDashboardPointActivityAsync(activity, page, pageSize, fromDate, toDate));
        }

        [HttpGet("promotions")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<PagedResult<PromotionResponse>>> GetPromotions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool includeInactive = false)
        {
            return Ok(await _loyaltyService.GetPromotionsAsync(page, pageSize, includeInactive));
        }

        [HttpGet("promotions/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> GetPromotion(Guid id)
        {
            return FromResult(await _loyaltyService.GetPromotionAsync(id));
        }

        [HttpPost("promotions")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> CreatePromotion(CreatePromotionRequest request)
        {
            return FromResult(await _loyaltyService.CreatePromotionAsync(request));
        }

        [HttpPost("campaigns/preview")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> PreviewCampaign(CampaignRequest request)
        {
            return FromResult(await _loyaltyService.PreviewCampaignAsync(request));
        }

        [HttpPost("campaigns")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> CreateCampaign(CampaignRequest request)
        {
            return FromResult(await _loyaltyService.CreateCampaignAsync(request));
        }

        [HttpPut("promotions/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> UpdatePromotion(Guid id, UpdatePromotionRequest request)
        {
            return FromResult(await _loyaltyService.UpdatePromotionAsync(id, request));
        }

        [HttpDelete("promotions/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> DeletePromotion(Guid id)
        {
            return FromResult(await _loyaltyService.DeletePromotionAsync(id));
        }

        [HttpPost("promotions/{id:guid}/send")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> SendPromotion(Guid id, SendPromotionRequest request)
        {
            return FromResult(await _loyaltyService.SendPromotionAsync(id, request));
        }

        [HttpPost("promotions/{id:guid}/audience-preview")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> PreviewPromotionAudience(Guid id, SendPromotionRequest request)
        {
            return FromResult(await _loyaltyService.PreviewPromotionAudienceAsync(id, request));
        }

        [HttpGet("promotions/{id:guid}/analytics")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> GetPromotionAnalytics(Guid id)
        {
            return FromResult(await _loyaltyService.GetPromotionAnalyticsAsync(id));
        }

        [HttpGet("segments")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IReadOnlyList<LoyaltySegmentCustomerResponse>>> GetSegments(
            [FromQuery] string segment = "at-risk", [FromQuery] int inactiveDays = 45, [FromQuery] Guid? branchId = null, [FromQuery] Guid? tierId = null)
        {
            return Ok(await _loyaltyService.GetSegmentCustomersAsync(segment, inactiveDays, branchId, tierId));
        }

        [HttpGet("customers/{customerId:guid}/360")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> GetCustomer360(Guid customerId)
        {
            return FromResult(await _loyaltyService.GetCustomer360Async(customerId));
        }

        [HttpGet("promotions/{id:guid}/customers/{customerId:guid}/usage")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> GetPromotionUsage(Guid id, Guid customerId)
        {
            return FromResult(await _loyaltyService.GetPromotionUsageAsync(id, customerId));
        }

        [HttpPost("promotions/{id:guid}/apply")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> ApplyPromotion(Guid id, ApplyPromotionRequest request)
        {
            if (request.CustomerId == Guid.Empty)
            {
                return BadRequest("CustomerId is required.");
            }

            return FromResult(await _loyaltyService.ApplyPromotionAsync(id, request));
        }

        private ActionResult FromResult<T>(OperationResult<T> result)
        {
            if (result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Data);
            }

            return Problem(
                title: result.StatusCode == 400 ? "Validation Error" : "Request Error",
                detail: result.Error,
                statusCode: result.StatusCode);
        }

    }
}
