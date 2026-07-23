using BusinessLayer.Dtos.Loyalty;
using BusinessLayer.Dtos.Operations;

namespace BusinessLayer.IService.Loyalty
{
    public interface ILoyaltyTierQuery
    {
        Task<PagedResult<LoyaltyTierResponse>> GetTiersAsync(int page, int pageSize, bool includeInactive);
        Task<OperationResult<LoyaltyTierResponse>> GetTierAsync(Guid id);
    }
}
