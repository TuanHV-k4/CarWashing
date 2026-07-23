using BusinessLayer.Dtos.Operations;

namespace BusinessLayer.IService.Operations;

public interface IAttendanceService
{
    Task<IReadOnlyList<AttendanceResponse>> GetMyAsync(Guid userId, DateTime date);
    Task<OperationResult<AttendanceResponse>> CheckInAsync(Guid assignmentId, Guid userId);
    Task<OperationResult<AttendanceResponse>> CheckOutAsync(Guid assignmentId, Guid userId);
    Task<IReadOnlyList<AttendanceResponse>> GetAsync(Guid? branchId, DateTime from, DateTime to, Guid? staffId, string? status);
    Task<OperationResult<AttendanceResponse>> AdjustAsync(Guid id, AttendanceAdjustmentRequest request, Guid adminUserId);
    Task<OperationResult<AttendanceResponse>> LockAsync(Guid id, AttendanceActionRequest request, Guid adminUserId);
    Task<OperationResult<AttendanceResponse>> ReopenAsync(Guid id, AttendanceActionRequest request, Guid adminUserId);
    Task<IReadOnlyList<AttendanceSummaryResponse>> GetSummaryAsync(Guid? branchId, DateTime from, DateTime to, string groupBy);
}
