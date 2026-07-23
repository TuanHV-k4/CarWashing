using BusinessLayer.Dtos.Admin;
using BusinessLayer.Dtos.Common;

namespace BusinessLayer.IService
{
    public interface IBehavioralLogService
    {
        Task<PagedResult<BehavioralLogItemDto>> GetLogsAsync(BehavioralLogFilterDto filter);
        Task<byte[]> ExportLogsAsync(BehavioralLogFilterDto filter);
    }
}
