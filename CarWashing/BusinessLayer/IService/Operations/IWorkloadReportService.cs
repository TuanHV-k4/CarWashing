using BusinessLayer.Dtos.Operations;

namespace BusinessLayer.IService.Operations;

public interface IWorkloadReportService
{
    Task<OperationResult<IReadOnlyList<StaffWorkloadResponse>>> GetAsync(Guid actorId, Guid branchId, DateTime from, DateTime to);
    Task<OperationResult<StaffWorkloadResponse>> GetMineAsync(Guid staffUserId, DateTime from, DateTime to);
}
