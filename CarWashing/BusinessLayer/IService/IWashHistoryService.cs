using BusinessLayer.Dtos.Common;
using BusinessLayer.Dtos.History;

namespace BusinessLayer.IService
{
    public interface IWashHistoryService
    {
        Task<PagedResult<WashHistoryListItemDto>> GetMyHistoryAsync(int page, int pageSize);
        Task<PagedResult<WashHistoryListItemDto>> GetHistoryByCustomerIdAsync(Guid customerId, int page, int pageSize);
        Task<WashHistoryDetailDto> GetMyHistoryDetailAsync(Guid washHistoryId);
        Task<WashHistoryDetailDto> SubmitMyFeedbackAsync(Guid washHistoryId, SubmitWashFeedbackRequest request);
        Task<BusinessLayer.Dtos.Operations.OperationResult<PagedResult<CustomerFeedbackItemDto>>> GetOperationalFeedbackAsync(Guid actorId, bool isAdmin, bool isManager, CustomerFeedbackFilter filter);
        Task<BusinessLayer.Dtos.Operations.OperationResult<PagedResult<OperationalWashHistoryItemDto>>> GetOperationalHistoryAsync(Guid actorId, bool isAdmin, OperationalWashHistoryFilter filter);
    }
}
