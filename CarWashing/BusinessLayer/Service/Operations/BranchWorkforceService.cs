using BusinessLayer.Dtos.Operations;
using BusinessLayer.IService.Operations;
using DataAccessLayer.Context;
using DataAccessLayer.Entity;
using DataAccessLayer.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service.Operations;

public class BranchWorkforceService : IBranchWorkforceService
{
    private readonly ApplicationDbContext _db;
    public BranchWorkforceService(ApplicationDbContext db) => _db = db;

    public async Task<bool> CanManageBranchAsync(Guid userId, Guid branchId)
    {
        if (await _db.Users.Include(user => user.Role).AnyAsync(user => user.UserID == userId && user.Role.RoleName == "Admin")) return true;
        return await _db.BranchManagerMemberships.AnyAsync(item => item.UserID == userId && item.BranchID == branchId && item.IsActive);
    }

    public async Task<IReadOnlyList<Guid>> GetManagedBranchIdsAsync(Guid userId)
    {
        return await _db.BranchManagerMemberships.AsNoTracking()
            .Where(item => item.UserID == userId && item.IsActive)
            .Select(item => item.BranchID)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Guid>> GetStaffBranchIdsAsync(Guid userId)
    {
        var today = UtcDate(BangkokNow());
        return await _db.BranchStaffMemberships.AsNoTracking()
            .Where(item => item.UserID == userId && item.IsActive && item.EffectiveFrom <= today && (!item.EffectiveTo.HasValue || item.EffectiveTo >= today))
            .Select(item => item.BranchID).Distinct().ToListAsync();
    }

    public async Task<bool> CanWorkAtBranchAsync(Guid userId, Guid branchId) =>
        (await GetStaffBranchIdsAsync(userId)).Contains(branchId);

    public async Task<ManagerBranchContextResponse?> GetManagerBranchContextAsync(Guid userId)
    {
        return await _db.BranchManagerMemberships.AsNoTracking()
            .Where(item => item.UserID == userId && item.IsActive)
            .Select(item => new ManagerBranchContextResponse { BranchId = item.BranchID, BranchName = item.Branch.BranchName })
            .SingleOrDefaultAsync();
    }

    public async Task<OperationResult<Guid>> ResolveManagerReportBranchAsync(Guid userId, bool isAdmin, Guid? requestedBranchId)
    {
        if (isAdmin)
        {
            if (!requestedBranchId.HasValue || requestedBranchId.Value == Guid.Empty)
                return OperationResult<Guid>.Failure("branchId is required for administrators.", 400);
            if (!await _db.Branches.AnyAsync(item => item.BranchID == requestedBranchId.Value))
                return OperationResult<Guid>.Failure("Branch not found.", 404);
            return OperationResult<Guid>.Success(requestedBranchId.Value);
        }

        var context = await GetManagerBranchContextAsync(userId);
        if (context is null) return OperationResult<Guid>.Failure("You are not assigned to an active branch.", 403);
        if (requestedBranchId.HasValue && requestedBranchId.Value != context.BranchId)
            return OperationResult<Guid>.Failure("You cannot view another branch.", 403);
        return OperationResult<Guid>.Success(context.BranchId);
    }

    public async Task<IReadOnlyList<BranchMembershipDetailResponse>> GetMembershipsAsync(Guid userId)
    {
        var managers = await _db.BranchManagerMemberships.AsNoTracking().Include(item => item.Branch)
            .Where(item => item.UserID == userId)
            .Select(item => new BranchMembershipDetailResponse { Id = item.BranchManagerMembershipID, MembershipType = "Manager", BranchId = item.BranchID, BranchName = item.Branch.BranchName, UserId = item.UserID, IsActive = item.IsActive })
            .ToListAsync();
        var staff = await _db.BranchStaffMemberships.AsNoTracking().Include(item => item.Branch)
            .Where(item => item.UserID == userId)
            .Select(item => new BranchMembershipDetailResponse { Id = item.BranchStaffMembershipID, MembershipType = "Staff", BranchId = item.BranchID, BranchName = item.Branch.BranchName, UserId = item.UserID, IsActive = item.IsActive, EffectiveFrom = item.EffectiveFrom, EffectiveTo = item.EffectiveTo })
            .ToListAsync();
        return managers.Concat(staff).OrderByDescending(item => item.IsActive).ThenBy(item => item.BranchName).ToList();
    }

    public Task<OperationResult<BranchMembershipResponse>> AddStaffAsync(CreateBranchMembershipRequest request) => AddAsync(request, false);
    public Task<OperationResult<BranchMembershipResponse>> AddManagerAsync(CreateBranchMembershipRequest request) => AddAsync(request, true);

    private async Task<OperationResult<BranchMembershipResponse>> AddAsync(CreateBranchMembershipRequest request, bool manager)
    {
        var user = await _db.Users.Include(item => item.Role).FirstOrDefaultAsync(item => item.UserID == request.UserId && item.Status == UserStatusEnum.Active);
        if (user is null || (manager ? user.Role.RoleName != "BranchManager" : user.Role.RoleName != "Staff")) return OperationResult<BranchMembershipResponse>.Failure("Active user with the required role was not found.", 400);
        if (!await _db.Branches.AnyAsync(item => item.BranchID == request.BranchId && item.Status == BranchStatusEnum.Open)) return OperationResult<BranchMembershipResponse>.Failure("Open branch not found.", 404);

        if (manager)
        {
            var activeManager = await _db.BranchManagerMemberships.FirstOrDefaultAsync(item => item.UserID == request.UserId && item.IsActive);
            if (activeManager is not null && activeManager.BranchID != request.BranchId)
                return OperationResult<BranchMembershipResponse>.Failure("Manager is already assigned to another branch. Deactivate the current assignment before assigning a new branch.", 409);
            var existingManager = await _db.BranchManagerMemberships.FirstOrDefaultAsync(item => item.BranchID == request.BranchId && item.UserID == request.UserId);
            if (existingManager is not null)
            {
                if (existingManager.IsActive) return OperationResult<BranchMembershipResponse>.Failure("Manager is already assigned.", 409);
                existingManager.IsActive = true;
                await _db.SaveChangesAsync();
                return OperationResult<BranchMembershipResponse>.Success(new BranchMembershipResponse { Id = existingManager.BranchManagerMembershipID, BranchId = existingManager.BranchID, UserId = existingManager.UserID, UserName = user.FullName, IsActive = true });
            }
            var membership = new BranchManagerMembership { BranchID = request.BranchId, UserID = request.UserId };
            _db.BranchManagerMemberships.Add(membership);
            await _db.SaveChangesAsync();
            return OperationResult<BranchMembershipResponse>.Success(new BranchMembershipResponse { Id = membership.BranchManagerMembershipID, BranchId = membership.BranchID, UserId = membership.UserID, UserName = user.FullName, IsActive = true }, 201);
        }

        if (await _db.BranchStaffMemberships.AnyAsync(item => item.BranchID == request.BranchId && item.UserID == request.UserId && item.IsActive)) return OperationResult<BranchMembershipResponse>.Failure("Staff is already assigned.", 409);
        var staffMembership = new BranchStaffMembership { BranchID = request.BranchId, UserID = request.UserId, EffectiveFrom = UtcDate(request.EffectiveFrom) };
        _db.BranchStaffMemberships.Add(staffMembership);
        await _db.SaveChangesAsync();
        return OperationResult<BranchMembershipResponse>.Success(new BranchMembershipResponse { Id = staffMembership.BranchStaffMembershipID, BranchId = staffMembership.BranchID, UserId = staffMembership.UserID, UserName = user.FullName, IsActive = true }, 201);
    }

    public async Task<OperationResult<BranchMembershipDetailResponse>> EndStaffAsync(Guid membershipId, EndBranchMembershipRequest request)
    {
        var membership = await _db.BranchStaffMemberships.Include(item => item.Branch).FirstOrDefaultAsync(item => item.BranchStaffMembershipID == membershipId);
        if (membership is null) return OperationResult<BranchMembershipDetailResponse>.Failure("Staff membership not found.", 404);
        if (!membership.IsActive) return OperationResult<BranchMembershipDetailResponse>.Failure("Staff membership is already inactive.", 409);
        var effectiveTo = UtcDate(request.EffectiveTo ?? DateTime.UtcNow);
        if (effectiveTo < membership.EffectiveFrom) return OperationResult<BranchMembershipDetailResponse>.Failure("Effective end date cannot be before the start date.", 400);
        membership.EffectiveTo = effectiveTo;
        membership.IsActive = false;
        await _db.SaveChangesAsync();
        return OperationResult<BranchMembershipDetailResponse>.Success(MapMembership(membership));
    }

    public async Task<OperationResult<BranchMembershipDetailResponse>> DeactivateManagerAsync(Guid membershipId)
    {
        var membership = await _db.BranchManagerMemberships.Include(item => item.Branch).FirstOrDefaultAsync(item => item.BranchManagerMembershipID == membershipId);
        if (membership is null) return OperationResult<BranchMembershipDetailResponse>.Failure("Manager membership not found.", 404);
        if (!membership.IsActive) return OperationResult<BranchMembershipDetailResponse>.Failure("Manager membership is already inactive.", 409);
        membership.IsActive = false;
        await _db.SaveChangesAsync();
        return OperationResult<BranchMembershipDetailResponse>.Success(MapMembership(membership));
    }

    public async Task<IReadOnlyList<ManagerAttendanceResponse>> GetManagerAttendanceAsync(Guid actorId, Guid branchId, DateTime workDate)
    {
        if (!await CanManageBranchAsync(actorId, branchId)) return Array.Empty<ManagerAttendanceResponse>();
        var date = UtcDate(workDate);
        var members = await _db.BranchStaffMemberships.Include(item => item.User)
            .Where(item => item.BranchID == branchId && item.IsActive && item.EffectiveFrom <= date && (!item.EffectiveTo.HasValue || item.EffectiveTo >= date)).ToListAsync();
        var records = await _db.AttendanceRecords.Where(item => item.BranchID == branchId && item.WorkDate == date).ToListAsync();
        return members.Select(member => Map(member, records.FirstOrDefault(record => record.BranchStaffMembershipID == member.BranchStaffMembershipID))).ToList();
    }

    public async Task<OperationResult<ManagerAttendanceResponse>> CheckInAsync(Guid actorId, Guid membershipId)
    {
        var membership = await _db.BranchStaffMemberships.Include(item => item.User).FirstOrDefaultAsync(item => item.BranchStaffMembershipID == membershipId && item.IsActive);
        if (membership is null) return OperationResult<ManagerAttendanceResponse>.Failure("Staff membership not found.", 404);
        if (!await CanManageBranchAsync(actorId, membership.BranchID)) return OperationResult<ManagerAttendanceResponse>.Failure("You cannot manage this branch.", 403);
        var localNow = BangkokNow();
        var workDate = UtcDate(localNow);
        var record = await _db.AttendanceRecords.FirstOrDefaultAsync(item => item.BranchStaffMembershipID == membershipId && item.WorkDate == workDate);
        if (record?.Status == AttendanceStatusEnum.Absent)
            return OperationResult<ManagerAttendanceResponse>.Failure("This staff member is marked absent. Reinstate the attendance record before checking in.", 409);
        if (record is null)
        {
            record = new AttendanceRecord { BranchID = membership.BranchID, BranchStaffMembershipID = membershipId, WorkDate = workDate, CheckedInAt = DateTime.UtcNow, CheckedInByUserID = actorId, LateMinutes = Math.Max(0, (int)(localNow.TimeOfDay - TimeSpan.FromHours(8)).TotalMinutes), Status = localNow.TimeOfDay > TimeSpan.FromHours(8) ? AttendanceStatusEnum.Late : AttendanceStatusEnum.OnTime };
            _db.AttendanceRecords.Add(record);
            await _db.SaveChangesAsync();
        }
        return OperationResult<ManagerAttendanceResponse>.Success(Map(membership, record));
    }

    public async Task<OperationResult<ManagerAttendanceResponse>> MarkAbsentAsync(Guid actorId, Guid membershipId, AttendanceExceptionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason)) return OperationResult<ManagerAttendanceResponse>.Failure("A reason is required when marking absence.", 400);
        var membership = await _db.BranchStaffMemberships.Include(item => item.User).FirstOrDefaultAsync(item => item.BranchStaffMembershipID == membershipId && item.IsActive);
        if (membership is null) return OperationResult<ManagerAttendanceResponse>.Failure("Staff membership not found.", 404);
        if (!await CanManageBranchAsync(actorId, membership.BranchID)) return OperationResult<ManagerAttendanceResponse>.Failure("You cannot manage this branch.", 403);
        var workDate = UtcDate(BangkokNow());
        var record = await _db.AttendanceRecords.FirstOrDefaultAsync(item => item.BranchStaffMembershipID == membershipId && item.WorkDate == workDate);
        if (record?.LockedAt.HasValue == true) return OperationResult<ManagerAttendanceResponse>.Failure("Attendance record is locked.", 409);
        if (record?.CheckedInAt.HasValue == true) return OperationResult<ManagerAttendanceResponse>.Failure("Checked-in attendance cannot be marked absent.", 409);
        if (record?.Status == AttendanceStatusEnum.Absent) return OperationResult<ManagerAttendanceResponse>.Success(Map(membership, record));
        if (record is null) { record = new AttendanceRecord { BranchID = membership.BranchID, BranchStaffMembershipID = membershipId, WorkDate = workDate }; _db.AttendanceRecords.Add(record); }
        var before = System.Text.Json.JsonSerializer.Serialize(new { record.Status, record.CheckedInAt, record.AdminNote });
        record.Status = AttendanceStatusEnum.Absent; record.AdminNote = request.Reason.Trim(); record.UpdatedAt = DateTime.UtcNow;
        _db.AttendanceAdjustments.Add(new AttendanceAdjustment { AttendanceRecord = record, Action = AttendanceAdjustmentActionEnum.MarkAbsent, PreviousValues = before, NewValues = System.Text.Json.JsonSerializer.Serialize(new { record.Status, record.AdminNote }), Reason = record.AdminNote, AdjustedByUserID = actorId });
        await _db.SaveChangesAsync();
        return OperationResult<ManagerAttendanceResponse>.Success(Map(membership, record));
    }

    public async Task<OperationResult<ManagerAttendanceResponse>> ReinstateAndCheckInAsync(Guid actorId, Guid membershipId, AttendanceExceptionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason)) return OperationResult<ManagerAttendanceResponse>.Failure("A reason is required when reinstating attendance.", 400);
        var membership = await _db.BranchStaffMemberships.Include(item => item.User).FirstOrDefaultAsync(item => item.BranchStaffMembershipID == membershipId && item.IsActive);
        if (membership is null) return OperationResult<ManagerAttendanceResponse>.Failure("Staff membership not found.", 404);
        if (!await CanManageBranchAsync(actorId, membership.BranchID)) return OperationResult<ManagerAttendanceResponse>.Failure("You cannot manage this branch.", 403);
        var localNow = BangkokNow(); var record = await _db.AttendanceRecords.FirstOrDefaultAsync(item => item.BranchStaffMembershipID == membershipId && item.WorkDate == UtcDate(localNow));
        if (record is null || record.Status != AttendanceStatusEnum.Absent) return OperationResult<ManagerAttendanceResponse>.Failure("Only an absent record can be reinstated.", 409);
        if (record.LockedAt.HasValue) return OperationResult<ManagerAttendanceResponse>.Failure("Attendance record is locked.", 409);
        var before = System.Text.Json.JsonSerializer.Serialize(new { record.Status, record.AdminNote });
        record.CheckedInAt = DateTime.UtcNow; record.CheckedInByUserID = actorId; record.LateMinutes = Math.Max(0, (int)(localNow.TimeOfDay - TimeSpan.FromHours(8)).TotalMinutes); record.Status = AttendanceStatusEnum.Late; record.AdminNote = request.Reason.Trim(); record.UpdatedAt = DateTime.UtcNow;
        _db.AttendanceAdjustments.Add(new AttendanceAdjustment { AttendanceRecordID = record.AttendanceRecordID, Action = AttendanceAdjustmentActionEnum.ReinstateFromAbsent, PreviousValues = before, NewValues = System.Text.Json.JsonSerializer.Serialize(new { record.Status, record.CheckedInAt, record.AdminNote }), Reason = record.AdminNote, AdjustedByUserID = actorId });
        await _db.SaveChangesAsync(); return OperationResult<ManagerAttendanceResponse>.Success(Map(membership, record));
    }

    public async Task<OperationResult<ManagerAttendanceResponse>> CheckOutAsync(Guid actorId, Guid membershipId)
    {
        var membership = await _db.BranchStaffMemberships.Include(item => item.User).FirstOrDefaultAsync(item => item.BranchStaffMembershipID == membershipId);
        if (membership is null) return OperationResult<ManagerAttendanceResponse>.Failure("Staff membership not found.", 404);
        if (!await CanManageBranchAsync(actorId, membership.BranchID)) return OperationResult<ManagerAttendanceResponse>.Failure("You cannot manage this branch.", 403);
        var record = await _db.AttendanceRecords.FirstOrDefaultAsync(item => item.BranchStaffMembershipID == membershipId && item.WorkDate == UtcDate(BangkokNow()));
        if (record is null || !record.CheckedInAt.HasValue) return OperationResult<ManagerAttendanceResponse>.Failure("Check in before checkout.", 400);
        if (!record.CheckedOutAt.HasValue)
        {
            var now = DateTime.UtcNow;
            var localTime = BangkokNow().TimeOfDay;
            record.CheckedOutAt = now;
            record.CheckedOutByUserID = actorId;
            record.WorkedMinutes = (int)(now - record.CheckedInAt.Value).TotalMinutes;
            record.EarlyLeaveMinutes = Math.Max(0, (int)(TimeSpan.FromHours(19) - localTime).TotalMinutes);
            record.OvertimeMinutes = localTime >= TimeSpan.FromHours(19.25) ? Math.Max(0, (int)(localTime - TimeSpan.FromHours(19)).TotalMinutes) : 0;
            record.Status = record.OvertimeMinutes > 0 ? AttendanceStatusEnum.Overtime : record.EarlyLeaveMinutes > 0 ? AttendanceStatusEnum.EarlyLeave : AttendanceStatusEnum.OnTime;
            await _db.SaveChangesAsync();
        }
        return OperationResult<ManagerAttendanceResponse>.Success(Map(membership, record));
    }

    private static DateTime UtcDate(DateTime value) => DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
    private static BranchMembershipDetailResponse MapMembership(BranchStaffMembership membership) => new() { Id = membership.BranchStaffMembershipID, MembershipType = "Staff", BranchId = membership.BranchID, BranchName = membership.Branch.BranchName, UserId = membership.UserID, IsActive = membership.IsActive, EffectiveFrom = membership.EffectiveFrom, EffectiveTo = membership.EffectiveTo };
    private static BranchMembershipDetailResponse MapMembership(BranchManagerMembership membership) => new() { Id = membership.BranchManagerMembershipID, MembershipType = "Manager", BranchId = membership.BranchID, BranchName = membership.Branch.BranchName, UserId = membership.UserID, IsActive = membership.IsActive };
    private static DateTime BangkokNow() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Bangkok"));
    private static ManagerAttendanceResponse Map(BranchStaffMembership membership, AttendanceRecord? record) => new() { Id = record?.AttendanceRecordID ?? Guid.Empty, MembershipId = membership.BranchStaffMembershipID, StaffName = membership.User.FullName, Status = record?.Status.ToString() ?? "NotCheckedIn", Note = record?.AdminNote, CheckedInAt = record?.CheckedInAt, CheckedOutAt = record?.CheckedOutAt, LateMinutes = record?.LateMinutes ?? 0, EarlyLeaveMinutes = record?.EarlyLeaveMinutes ?? 0, OvertimeMinutes = record?.OvertimeMinutes ?? 0 };
}
