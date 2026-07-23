using BusinessLayer.Dtos.Operations;
using BusinessLayer.IService.Operations;
using DataAccessLayer.Context;
using DataAccessLayer.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service.Operations;

public class WorkloadReportService : IWorkloadReportService
{
    private readonly ApplicationDbContext _db;
    private readonly IBranchWorkforceService _workforce;
    public WorkloadReportService(ApplicationDbContext db, IBranchWorkforceService workforce) { _db = db; _workforce = workforce; }

    public async Task<OperationResult<IReadOnlyList<StaffWorkloadResponse>>> GetAsync(Guid actorId, Guid branchId, DateTime from, DateTime to)
    {
        if (branchId == Guid.Empty)
        {
            var context = await _workforce.GetManagerBranchContextAsync(actorId);
            if (context is null) return OperationResult<IReadOnlyList<StaffWorkloadResponse>>.Failure("You are not assigned to an active branch.", 403);
            branchId = context.BranchId;
        }
        if (from > to) return OperationResult<IReadOnlyList<StaffWorkloadResponse>>.Failure("A valid date range is required.", 400);
        if (!await _workforce.CanManageBranchAsync(actorId, branchId)) return OperationResult<IReadOnlyList<StaffWorkloadResponse>>.Failure("You cannot view this branch.", 403);

        var start = UtcDate(from);
        var end = UtcDate(to).AddDays(1);
        var staff = await _db.BranchStaffMemberships.AsNoTracking()
            .Include(item => item.User)
            .Where(item => item.BranchID == branchId && item.IsActive && item.EffectiveFrom <= UtcDate(to) && (!item.EffectiveTo.HasValue || item.EffectiveTo >= UtcDate(from)))
            .Select(item => new { item.UserID, item.User.FullName })
            .Distinct()
            .ToListAsync();

        var rows = await _db.BookingStaffWorks.AsNoTracking()
            .Where(item => item.Booking.BranchID == branchId && item.Booking.BookingStatus == BookingStatusEnum.Completed && item.Booking.CompletedAt >= start && item.Booking.CompletedAt < end)
            .Select(item => new { item.StaffUserID, item.ContributionPercent, item.BookingID, item.Booking.EstimatedTotalAmount })
            .ToListAsync();

        var report = staff.Select(person =>
        {
            var work = rows.Where(item => item.StaffUserID == person.UserID).ToList();
            return new StaffWorkloadResponse
            {
                StaffUserId = person.UserID,
                StaffName = person.FullName,
                VehiclesParticipated = work.Select(item => item.BookingID).Distinct().Count(),
                VehiclesCompleted = work.Select(item => item.BookingID).Distinct().Count(),
                EquivalentVehicles = work.Sum(item => item.ContributionPercent) / 100m,
                EquivalentRevenue = work.Sum(item => item.EstimatedTotalAmount * item.ContributionPercent / 100m)
            };
        }).OrderByDescending(item => item.EquivalentVehicles).ThenBy(item => item.StaffName).ToList();

        return OperationResult<IReadOnlyList<StaffWorkloadResponse>>.Success(report);
    }

    public async Task<OperationResult<StaffWorkloadResponse>> GetMineAsync(Guid staffUserId, DateTime from, DateTime to)
    {
        if (from > to) return OperationResult<StaffWorkloadResponse>.Failure("A valid date range is required.", 400);

        var staff = await _db.Users.AsNoTracking()
            .Where(item => item.UserID == staffUserId)
            .Select(item => new { item.UserID, item.FullName })
            .SingleOrDefaultAsync();
        if (staff is null) return OperationResult<StaffWorkloadResponse>.Failure("Staff member not found.", 404);

        var start = UtcDate(from);
        var end = UtcDate(to).AddDays(1);
        var work = await _db.BookingStaffWorks.AsNoTracking()
            .Where(item => item.StaffUserID == staffUserId && item.Booking.BookingStatus == BookingStatusEnum.Completed && item.Booking.CompletedAt >= start && item.Booking.CompletedAt < end)
            .Select(item => new { item.BookingID, item.ContributionPercent, item.Booking.EstimatedTotalAmount })
            .ToListAsync();

        return OperationResult<StaffWorkloadResponse>.Success(new StaffWorkloadResponse
        {
            StaffUserId = staff.UserID,
            StaffName = staff.FullName,
            VehiclesParticipated = work.Select(item => item.BookingID).Distinct().Count(),
            VehiclesCompleted = work.Select(item => item.BookingID).Distinct().Count(),
            EquivalentVehicles = work.Sum(item => item.ContributionPercent) / 100m,
            EquivalentRevenue = work.Sum(item => item.EstimatedTotalAmount * item.ContributionPercent / 100m)
        });
    }

    private static DateTime UtcDate(DateTime value) => DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
}
