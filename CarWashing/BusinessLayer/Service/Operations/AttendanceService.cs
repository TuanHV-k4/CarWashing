using System.Text.Json;
using BusinessLayer.Dtos.Operations;
using BusinessLayer.Helpers;
using BusinessLayer.IService.Operations;
using DataAccessLayer.Context;
using DataAccessLayer.Entity;
using DataAccessLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BusinessLayer.Service.Operations;

public class AttendanceService : IAttendanceService
{
    private readonly ApplicationDbContext _context;
    private readonly AttendanceSettings _settings;

    public AttendanceService(ApplicationDbContext context, IOptions<AttendanceSettings> settings)
    {
        _context = context;
        _settings = settings.Value;
    }

    public async Task<IReadOnlyList<AttendanceResponse>> GetMyAsync(Guid userId, DateTime date)
    {
        var start = UtcDate(date);
        var end = start.AddDays(1);
        var assignments = await AssignmentQuery()
            .Where(item => item.UserID == userId && item.StaffShift.StartsAt < end && item.StaffShift.EndsAt > start)
            .OrderBy(item => item.StaffShift.StartsAt).ToListAsync();
        return assignments.Select(item => Map(item, item.AttendanceRecord, DateTime.UtcNow)).ToList();
    }

    public async Task<OperationResult<AttendanceResponse>> CheckInAsync(Guid assignmentId, Guid userId)
    {
        var assignment = await AssignmentQuery().FirstOrDefaultAsync(item => item.ShiftAssignmentID == assignmentId);
        if (assignment is null) return OperationResult<AttendanceResponse>.Failure("Shift assignment not found.", 404);
        if (assignment.UserID != userId) return OperationResult<AttendanceResponse>.Failure("You can only check in to your own shift.", 403);
        if (!assignment.StaffShift.IsActive) return OperationResult<AttendanceResponse>.Failure("This shift is inactive.", 400);

        var now = DateTime.UtcNow;
        if (assignment.AttendanceRecord is not null) return OperationResult<AttendanceResponse>.Success(Map(assignment, assignment.AttendanceRecord, now));
        if (now < assignment.StaffShift.StartsAt.AddMinutes(-_settings.CheckInEarlyMinutes) || now > assignment.StaffShift.StartsAt.AddMinutes(_settings.CheckInLateWindowMinutes))
            return OperationResult<AttendanceResponse>.Failure("Check-in is outside the allowed time window.", 400);

        var lateMinutes = Math.Max(0, (int)Math.Floor((now - assignment.StaffShift.StartsAt).TotalMinutes));
        var record = new AttendanceRecord
        {
            ShiftAssignmentID = assignmentId,
            CheckedInAt = now,
            CheckInSource = AttendanceSourceEnum.SelfService,
            LateMinutes = lateMinutes,
            Status = lateMinutes >= _settings.LateThresholdMinutes ? AttendanceStatusEnum.Late : AttendanceStatusEnum.CheckedIn
        };
        _context.AttendanceRecords.Add(record);
        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            var existing = await _context.AttendanceRecords.AsNoTracking().FirstOrDefaultAsync(item => item.ShiftAssignmentID == assignmentId);
            if (existing is not null) return OperationResult<AttendanceResponse>.Success(Map(assignment, existing, now));
            throw;
        }
        assignment.AttendanceRecord = record;
        return OperationResult<AttendanceResponse>.Success(Map(assignment, record, now), 201);
    }

    public async Task<OperationResult<AttendanceResponse>> CheckOutAsync(Guid assignmentId, Guid userId)
    {
        var assignment = await AssignmentQuery().FirstOrDefaultAsync(item => item.ShiftAssignmentID == assignmentId);
        if (assignment is null) return OperationResult<AttendanceResponse>.Failure("Shift assignment not found.", 404);
        if (assignment.UserID != userId) return OperationResult<AttendanceResponse>.Failure("You can only check out from your own shift.", 403);
        var record = assignment.AttendanceRecord;
        if (record is null || !record.CheckedInAt.HasValue) return OperationResult<AttendanceResponse>.Failure("Check in before checking out.", 400);
        if (record.LockedAt.HasValue) return OperationResult<AttendanceResponse>.Failure("Attendance is locked by an administrator.", 409);
        if (record.CheckedOutAt.HasValue) return OperationResult<AttendanceResponse>.Success(Map(assignment, record, DateTime.UtcNow));

        var now = DateTime.UtcNow;
        record.CheckedOutAt = now;
        record.CheckOutSource = AttendanceSourceEnum.SelfService;
        record.WorkedMinutes = Math.Max(0, (int)Math.Floor((now - record.CheckedInAt.Value).TotalMinutes));
        record.EarlyLeaveMinutes = Math.Max(0, (int)Math.Ceiling((assignment.StaffShift.EndsAt - now).TotalMinutes));
        record.Status = record.EarlyLeaveMinutes >= _settings.EarlyLeaveThresholdMinutes ? AttendanceStatusEnum.EarlyLeave : AttendanceStatusEnum.CheckedOut;
        record.UpdatedAt = now;
        await _context.SaveChangesAsync();
        return OperationResult<AttendanceResponse>.Success(Map(assignment, record, now));
    }

    public async Task<IReadOnlyList<AttendanceResponse>> GetAsync(Guid? branchId, DateTime from, DateTime to, Guid? staffId, string? status)
    {
        from = NormalizeUtc(from);
        to = NormalizeUtc(to);
        var assignments = await AssignmentQuery()
            .Where(item => (!branchId.HasValue || item.StaffShift.BranchID == branchId.Value) && (!staffId.HasValue || item.UserID == staffId.Value) && item.StaffShift.StartsAt < to && item.StaffShift.EndsAt > from)
            .OrderBy(item => item.StaffShift.StartsAt).ThenBy(item => item.User.FullName).ToListAsync();
        var now = DateTime.UtcNow;
        return assignments.Select(item => Map(item, item.AttendanceRecord, now))
            .Where(item => string.IsNullOrWhiteSpace(status) || string.Equals(item.Status, status, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<OperationResult<AttendanceResponse>> AdjustAsync(Guid id, AttendanceAdjustmentRequest request, Guid adminUserId)
    {
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length > 500) return OperationResult<AttendanceResponse>.Failure("Reason is required and must be at most 500 characters.", 400);
        var record = await RecordQuery().FirstOrDefaultAsync(item => item.AttendanceRecordID == id);
        if (record is null) return OperationResult<AttendanceResponse>.Failure("Attendance record not found.", 404);
        if (record.LockedAt.HasValue) return OperationResult<AttendanceResponse>.Failure("Reopen attendance before making an adjustment.", 409);
        var before = Snapshot(record);
        if (request.CheckedInAt.HasValue) record.CheckedInAt = request.CheckedInAt.Value.ToUniversalTime();
        if (request.CheckedOutAt.HasValue) record.CheckedOutAt = request.CheckedOutAt.Value.ToUniversalTime();
        if (record.CheckedOutAt.HasValue && (!record.CheckedInAt.HasValue || record.CheckedOutAt < record.CheckedInAt)) return OperationResult<AttendanceResponse>.Failure("Check-out must be after check-in.", 400);
        AttendanceStatusEnum parsedStatus = record.Status;
        if (!string.IsNullOrWhiteSpace(request.Status) && !Enum.TryParse(request.Status, true, out parsedStatus)) return OperationResult<AttendanceResponse>.Failure("Invalid attendance status.", 400);
        if (!string.IsNullOrWhiteSpace(request.Status)) record.Status = parsedStatus;
        record.AdminNote = string.IsNullOrWhiteSpace(request.AdminNote) ? record.AdminNote : request.AdminNote.Trim();
        Recalculate(record);
        record.UpdatedAt = DateTime.UtcNow;
        AddAudit(record, AttendanceAdjustmentActionEnum.Adjusted, before, request.Reason, adminUserId);
        await _context.SaveChangesAsync();
        return OperationResult<AttendanceResponse>.Success(Map(record.ShiftAssignment, record, DateTime.UtcNow));
    }

    public Task<OperationResult<AttendanceResponse>> LockAsync(Guid id, AttendanceActionRequest request, Guid adminUserId) => SetLockAsync(id, request, adminUserId, true);
    public Task<OperationResult<AttendanceResponse>> ReopenAsync(Guid id, AttendanceActionRequest request, Guid adminUserId) => SetLockAsync(id, request, adminUserId, false);

    public async Task<IReadOnlyList<AttendanceSummaryResponse>> GetSummaryAsync(Guid? branchId, DateTime from, DateTime to, string groupBy)
    {
        var items = await GetAsync(branchId, from, to, null, null);
        return string.Equals(groupBy, "day", StringComparison.OrdinalIgnoreCase)
            ? items.GroupBy(item => item.StartsAt.Date).OrderBy(item => item.Key).Select(item => Summary(item.Key.ToString("yyyy-MM-dd"), item.Key.ToString("yyyy-MM-dd"), item)).ToList()
            : items.GroupBy(item => new { item.StaffUserId, item.StaffName }).OrderBy(item => item.Key.StaffName).Select(item => Summary(item.Key.StaffUserId.ToString(), item.Key.StaffName, item)).ToList();
    }

    private async Task<OperationResult<AttendanceResponse>> SetLockAsync(Guid id, AttendanceActionRequest request, Guid adminUserId, bool locked)
    {
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length > 500) return OperationResult<AttendanceResponse>.Failure("Reason is required and must be at most 500 characters.", 400);
        var record = await RecordQuery().FirstOrDefaultAsync(item => item.AttendanceRecordID == id);
        if (record is null) return OperationResult<AttendanceResponse>.Failure("Attendance record not found.", 404);
        var before = Snapshot(record);
        record.LockedAt = locked ? DateTime.UtcNow : null;
        record.LockedByUserID = locked ? adminUserId : null;
        record.UpdatedAt = DateTime.UtcNow;
        AddAudit(record, locked ? AttendanceAdjustmentActionEnum.Locked : AttendanceAdjustmentActionEnum.Reopened, before, request.Reason, adminUserId);
        await _context.SaveChangesAsync();
        return OperationResult<AttendanceResponse>.Success(Map(record.ShiftAssignment, record, DateTime.UtcNow));
    }

    private IQueryable<ShiftAssignment> AssignmentQuery() => _context.ShiftAssignments
        .Include(item => item.User).Include(item => item.WashBay).Include(item => item.StaffShift)
        .Include(item => item.AttendanceRecord);
    private IQueryable<AttendanceRecord> RecordQuery() => _context.AttendanceRecords.Include(item => item.ShiftAssignment).ThenInclude(item => item.User).Include(item => item.ShiftAssignment).ThenInclude(item => item.WashBay).Include(item => item.ShiftAssignment).ThenInclude(item => item.StaffShift);

    private AttendanceResponse Map(ShiftAssignment assignment, AttendanceRecord? record, DateTime now) => new()
    {
        Id = record?.AttendanceRecordID ?? Guid.Empty, ShiftAssignmentId = assignment.ShiftAssignmentID, StaffUserId = assignment.UserID, StaffName = assignment.User.FullName,
        ShiftId = assignment.StaffShiftID, ShiftName = assignment.StaffShift.Name, BranchId = assignment.StaffShift.BranchID, WashBayName = assignment.WashBay?.BayName,
        StartsAt = assignment.StaffShift.StartsAt, EndsAt = assignment.StaffShift.EndsAt, Status = DisplayStatus(assignment, record, now), CheckedInAt = record?.CheckedInAt, CheckedOutAt = record?.CheckedOutAt,
        LateMinutes = record?.LateMinutes ?? 0, EarlyLeaveMinutes = record?.EarlyLeaveMinutes ?? 0, WorkedMinutes = record?.WorkedMinutes ?? 0, IsLocked = record?.LockedAt.HasValue ?? false, AdminNote = record?.AdminNote
    };
    private string DisplayStatus(ShiftAssignment assignment, AttendanceRecord? record, DateTime now)
    {
        if (record is not null) return record.Status.ToString();
        return now > assignment.StaffShift.StartsAt.AddMinutes(_settings.AbsentAfterStartMinutes) ? AttendanceStatusEnum.Absent.ToString() : "NotCheckedIn";
    }
    private void Recalculate(AttendanceRecord record)
    {
        var shift = record.ShiftAssignment.StaffShift;
        record.LateMinutes = record.CheckedInAt.HasValue ? Math.Max(0, (int)Math.Floor((record.CheckedInAt.Value - shift.StartsAt).TotalMinutes)) : 0;
        record.EarlyLeaveMinutes = record.CheckedOutAt.HasValue ? Math.Max(0, (int)Math.Ceiling((shift.EndsAt - record.CheckedOutAt.Value).TotalMinutes)) : 0;
        record.WorkedMinutes = record.CheckedInAt.HasValue && record.CheckedOutAt.HasValue ? Math.Max(0, (int)Math.Floor((record.CheckedOutAt.Value - record.CheckedInAt.Value).TotalMinutes)) : 0;
    }
    private void AddAudit(AttendanceRecord record, AttendanceAdjustmentActionEnum action, string before, string reason, Guid userId) => _context.AttendanceAdjustments.Add(new AttendanceAdjustment { AttendanceRecordID = record.AttendanceRecordID, Action = action, PreviousValues = before, NewValues = Snapshot(record), Reason = reason.Trim(), AdjustedByUserID = userId });
    private static string Snapshot(AttendanceRecord record) => JsonSerializer.Serialize(new { record.Status, record.CheckedInAt, record.CheckedOutAt, record.LateMinutes, record.EarlyLeaveMinutes, record.WorkedMinutes, record.LockedAt, record.LockedByUserID });
    private static DateTime UtcDate(DateTime value) => DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
    private static DateTime NormalizeUtc(DateTime value) => value.Kind == DateTimeKind.Utc ? value : value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    private static AttendanceSummaryResponse Summary(string key, string label, IEnumerable<AttendanceResponse> items)
    {
        var list = items.ToList();
        return new AttendanceSummaryResponse { GroupKey = key, Label = label, AssignedShiftCount = list.Count, CompletedShiftCount = list.Count(item => item.CheckedOutAt.HasValue), LateCount = list.Count(item => item.LateMinutes > 0), AbsentCount = list.Count(item => item.Status == "Absent"), PlannedMinutes = list.Sum(item => Math.Max(0, (int)(item.EndsAt - item.StartsAt).TotalMinutes)), WorkedMinutes = list.Sum(item => item.WorkedMinutes) };
    }
}
