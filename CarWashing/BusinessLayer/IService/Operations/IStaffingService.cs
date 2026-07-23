using BusinessLayer.Dtos.Operations;

namespace BusinessLayer.IService.Operations;

public interface IStaffingService
{
    Task<IReadOnlyList<StaffShiftResponse>> GetShiftsAsync(Guid? branchId, DateTime? from, DateTime? to);
    Task<OperationResult<StaffShiftResponse>> GetShiftAsync(Guid id);
    Task<OperationResult<StaffShiftResponse>> CreateShiftAsync(CreateStaffShiftRequest request);
    Task<OperationResult<StaffShiftResponse>> UpdateShiftAsync(Guid id, UpdateStaffShiftRequest request);
    Task<OperationResult<bool>> DeactivateShiftAsync(Guid id);
    Task<OperationResult<StaffShiftResponse>> AssignStaffAsync(Guid shiftId, AssignStaffToShiftRequest request);
    Task<OperationResult<StaffShiftResponse>> RemoveAssignmentAsync(Guid shiftId, Guid assignmentId);
    Task<IReadOnlyList<AvailableStaffResponse>> GetAvailableStaffAsync(Guid branchId, DateTime startsAt, DateTime endsAt);
    Task<OperationResult<StaffShiftCapacityResponse>> GetShiftCapacityAsync(Guid shiftId);
}
