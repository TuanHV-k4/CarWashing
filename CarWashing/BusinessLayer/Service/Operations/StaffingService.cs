using BusinessLayer.Dtos.Operations;
using BusinessLayer.IService.Operations;
using DataAccessLayer.Context;
using DataAccessLayer.Entity;
using DataAccessLayer.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service.Operations;

public class StaffingService : IStaffingService
{
    private readonly ApplicationDbContext _context;

    public StaffingService(ApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<StaffShiftResponse>> GetShiftsAsync(Guid? branchId, DateTime? from, DateTime? to)
    {
        var query = ShiftQuery().AsNoTracking();
        if (branchId.HasValue) query = query.Where(shift => shift.BranchID == branchId.Value);
        if (from.HasValue) query = query.Where(shift => shift.EndsAt >= from.Value);
        if (to.HasValue) query = query.Where(shift => shift.StartsAt <= to.Value);
        var shifts = await query.OrderBy(shift => shift.StartsAt).ToListAsync();
        return shifts.Select(Map).ToList();
    }

    public async Task<OperationResult<StaffShiftResponse>> GetShiftAsync(Guid id)
    {
        var shift = await ShiftQuery().AsNoTracking().FirstOrDefaultAsync(item => item.StaffShiftID == id);
        return shift is null ? OperationResult<StaffShiftResponse>.Failure("Shift not found.", 404) : OperationResult<StaffShiftResponse>.Success(Map(shift));
    }

    public async Task<OperationResult<StaffShiftResponse>> CreateShiftAsync(CreateStaffShiftRequest request)
    {
        var error = await ValidateShiftAsync(request.BranchId, request.StartsAt, request.EndsAt, request.Name);
        if (error is not null) return OperationResult<StaffShiftResponse>.Failure(error, 400);
        var shift = new StaffShift { BranchID = request.BranchId, StartsAt = request.StartsAt, EndsAt = request.EndsAt, Name = request.Name.Trim() };
        _context.StaffShifts.Add(shift);
        await _context.SaveChangesAsync();
        return OperationResult<StaffShiftResponse>.Success(Map(shift), 201);
    }

    public async Task<OperationResult<StaffShiftResponse>> UpdateShiftAsync(Guid id, UpdateStaffShiftRequest request)
    {
        var shift = await ShiftQuery().FirstOrDefaultAsync(item => item.StaffShiftID == id);
        if (shift is null) return OperationResult<StaffShiftResponse>.Failure("Shift not found.", 404);
        if (await _context.AttendanceRecords.AnyAsync(item => item.ShiftAssignment.StaffShiftID == id))
            return OperationResult<StaffShiftResponse>.Failure("A shift with attendance records cannot be edited. Adjust attendance instead.", 409);
        var error = await ValidateShiftAsync(shift.BranchID, request.StartsAt, request.EndsAt, request.Name);
        if (error is not null) return OperationResult<StaffShiftResponse>.Failure(error, 400);
        if (request.IsActive && shift.Assignments.Count > 0)
        {
            var staffIds = shift.Assignments.Select(item => item.UserID).ToList();
            var conflicts = await _context.ShiftAssignments
                .Include(item => item.StaffShift)
                .AnyAsync(item => staffIds.Contains(item.UserID) && item.StaffShift.IsActive && item.StaffShift.StaffShiftID != id && item.StaffShift.StartsAt < request.EndsAt && request.StartsAt < item.StaffShift.EndsAt);
            if (conflicts) return OperationResult<StaffShiftResponse>.Failure("Updated time would overlap an assigned staff member's active shift.", 409);
        }
        shift.StartsAt = request.StartsAt;
        shift.EndsAt = request.EndsAt;
        shift.Name = request.Name.Trim();
        shift.IsActive = request.IsActive;
        await _context.SaveChangesAsync();
        return OperationResult<StaffShiftResponse>.Success(Map(shift));
    }

    public async Task<OperationResult<bool>> DeactivateShiftAsync(Guid id)
    {
        var shift = await _context.StaffShifts.FirstOrDefaultAsync(item => item.StaffShiftID == id);
        if (shift is null) return OperationResult<bool>.Failure("Shift not found.", 404);
        if (await _context.AttendanceRecords.AnyAsync(item => item.ShiftAssignment.StaffShiftID == id))
            return OperationResult<bool>.Failure("A shift with attendance records cannot be deactivated.", 409);
        shift.IsActive = false;
        await _context.SaveChangesAsync();
        return OperationResult<bool>.Success(true);
    }

    public async Task<OperationResult<StaffShiftResponse>> AssignStaffAsync(Guid shiftId, AssignStaffToShiftRequest request)
    {
        var shift = await ShiftQuery().FirstOrDefaultAsync(item => item.StaffShiftID == shiftId);
        if (shift is null) return OperationResult<StaffShiftResponse>.Failure("Shift not found.", 404);
        if (!shift.IsActive) return OperationResult<StaffShiftResponse>.Failure("Cannot assign staff to an inactive shift.", 400);

        var staff = await _context.Users.Include(item => item.Role).FirstOrDefaultAsync(item => item.UserID == request.UserId);
        if (staff is null || staff.Status != UserStatusEnum.Active || !string.Equals(staff.Role.RoleName, "Staff", StringComparison.OrdinalIgnoreCase))
            return OperationResult<StaffShiftResponse>.Failure("Active staff user not found.", 400);

        if (request.WashBayId.HasValue)
        {
            var bayIsValid = await _context.WashBays.AnyAsync(item => item.WashBayID == request.WashBayId.Value && item.BranchID == shift.BranchID && item.Status == WashBayStatusEnum.Active);
            if (!bayIsValid) return OperationResult<StaffShiftResponse>.Failure("Wash bay must be active and belong to the shift branch.", 400);
        }

        var hasConflict = await _context.ShiftAssignments
            .Include(item => item.StaffShift)
            .AnyAsync(item => item.UserID == request.UserId && item.StaffShift.IsActive && item.StaffShift.StaffShiftID != shiftId && item.StaffShift.StartsAt < shift.EndsAt && shift.StartsAt < item.StaffShift.EndsAt);
        if (hasConflict) return OperationResult<StaffShiftResponse>.Failure("Staff member has an overlapping active shift.", 409);
        if (shift.Assignments.Any(item => item.UserID == request.UserId)) return OperationResult<StaffShiftResponse>.Failure("Staff member is already assigned to this shift.", 409);

        shift.Assignments.Add(new ShiftAssignment { UserID = request.UserId, WashBayID = request.WashBayId, User = staff });
        await _context.SaveChangesAsync();
        return OperationResult<StaffShiftResponse>.Success(Map(shift));
    }

    public async Task<OperationResult<StaffShiftResponse>> RemoveAssignmentAsync(Guid shiftId, Guid assignmentId)
    {
        var shift = await ShiftQuery().FirstOrDefaultAsync(item => item.StaffShiftID == shiftId);
        if (shift is null) return OperationResult<StaffShiftResponse>.Failure("Shift not found.", 404);
        var assignment = shift.Assignments.FirstOrDefault(item => item.ShiftAssignmentID == assignmentId);
        if (assignment is null) return OperationResult<StaffShiftResponse>.Failure("Shift assignment not found.", 404);
        if (await _context.AttendanceRecords.AnyAsync(item => item.ShiftAssignmentID == assignmentId))
            return OperationResult<StaffShiftResponse>.Failure("A shift assignment with attendance cannot be removed.", 409);
        _context.ShiftAssignments.Remove(assignment);
        await _context.SaveChangesAsync();
        return OperationResult<StaffShiftResponse>.Success(Map(shift));
    }

    public async Task<IReadOnlyList<AvailableStaffResponse>> GetAvailableStaffAsync(Guid branchId, DateTime startsAt, DateTime endsAt)
    {
        var conflictingStaffIds = await _context.ShiftAssignments.AsNoTracking()
            .Include(item => item.StaffShift)
            .Where(item => item.StaffShift.IsActive && item.StaffShift.StartsAt < endsAt && startsAt < item.StaffShift.EndsAt)
            .Select(item => item.UserID)
            .Distinct()
            .ToListAsync();

        return await _context.Users.AsNoTracking()
            .Include(item => item.Role)
            .Where(item => item.Status == UserStatusEnum.Active
                && item.Role.RoleName == "Staff"
                && !conflictingStaffIds.Contains(item.UserID))
            .OrderBy(item => item.FullName)
            .Select(item => new AvailableStaffResponse
            {
                UserId = item.UserID,
                FullName = item.FullName
            }).ToListAsync();
    }

    public async Task<OperationResult<StaffShiftCapacityResponse>> GetShiftCapacityAsync(Guid shiftId)
    {
        var shift = await _context.StaffShifts.AsNoTracking().Include(item => item.Assignments).FirstOrDefaultAsync(item => item.StaffShiftID == shiftId);
        if (shift is null) return OperationResult<StaffShiftCapacityResponse>.Failure("Shift not found.", 404);
        var activeBayCount = await _context.WashBays.CountAsync(item => item.BranchID == shift.BranchID && item.Status == WashBayStatusEnum.Active);
        var assignedBayCount = shift.Assignments.Where(item => item.WashBayID.HasValue).Select(item => item.WashBayID).Distinct().Count();
        return OperationResult<StaffShiftCapacityResponse>.Success(new StaffShiftCapacityResponse
        {
            ShiftId = shiftId,
            AssignedStaffCount = shift.Assignments.Count,
            AssignedBayCount = assignedBayCount,
            ActiveBayCount = activeBayCount,
            AvailableStaffCount = shift.IsActive ? shift.Assignments.Count : 0
        });
    }

    private IQueryable<StaffShift> ShiftQuery() => _context.StaffShifts
        .Include(shift => shift.Assignments).ThenInclude(assignment => assignment.User)
        .Include(shift => shift.Assignments).ThenInclude(assignment => assignment.WashBay);

    private async Task<string?> ValidateShiftAsync(Guid branchId, DateTime startsAt, DateTime endsAt, string name)
    {
        if (branchId == Guid.Empty) return "BranchId is required.";
        if (startsAt >= endsAt) return "EndsAt must be after StartsAt.";
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 120) return "Name is required and must be at most 120 characters.";
        var branchIsOpen = await _context.Branches.AnyAsync(branch => branch.BranchID == branchId && branch.Status == BranchStatusEnum.Open);
        return branchIsOpen ? null : "Active branch not found.";
    }

    private static StaffShiftResponse Map(StaffShift shift) => new()
    {
        Id = shift.StaffShiftID,
        BranchId = shift.BranchID,
        Name = shift.Name,
        StartsAt = shift.StartsAt,
        EndsAt = shift.EndsAt,
        IsActive = shift.IsActive,
        Assignments = shift.Assignments.Select(assignment => new ShiftAssignmentResponse
        {
            Id = assignment.ShiftAssignmentID,
            UserId = assignment.UserID,
            StaffName = assignment.User.FullName,
            WashBayId = assignment.WashBayID,
            WashBayName = assignment.WashBay?.BayName
        }).ToList()
    };
}
