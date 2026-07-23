using BusinessLayer.Dtos.Operations;
using BusinessLayer.IService.Operations;
using DataAccessLayer.Context;
using DataAccessLayer.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service.Operations;

public sealed class DashboardService(ApplicationDbContext db) : IDashboardService
{
    public async Task<ManagerDashboardResponse?> GetManagerAsync(Guid branchId, DateTime date)
    {
        var branch = await db.Branches.FindAsync(branchId);
        if (branch is null) return null;

        var day = date.Date;
        var dayEnd = day.AddDays(1);
        var weekStart = day.AddDays(-((7 + (int)day.DayOfWeek - (int)DayOfWeek.Monday) % 7));
        var monthStart = new DateTime(day.Year, day.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periods = new[]
        {
            ("day", "Hôm nay", day, dayEnd),
            ("week", "Tuần này", weekStart, weekStart.AddDays(7)),
            ("month", "Tháng này", monthStart, monthStart.AddMonths(1)),
        };

        var revenuePeriods = new List<ManagerRevenuePeriod>();
        foreach (var (period, label, from, to) in periods)
        {
            var gross = await db.Payments.Where(item => item.Booking.BranchID == branchId && item.PaymentStatus == PaymentStatusEnum.Paid && item.PaidAt >= from && item.PaidAt < to).SumAsync(item => (decimal?)item.Amount) ?? 0m;
            var refunds = await db.Refunds.Where(item => item.Payment.Booking.BranchID == branchId && item.Status == "Completed" && item.CompletedAt >= from && item.CompletedAt < to).SumAsync(item => (decimal?)item.Amount) ?? 0m;
            var completedVehicles = await db.Bookings.CountAsync(item => item.BranchID == branchId && item.BookingStatus == BookingStatusEnum.Completed && item.CompletedAt >= from && item.CompletedAt < to);
            revenuePeriods.Add(new ManagerRevenuePeriod { Period = period, Label = label, From = from, To = to, CompletedVehicles = completedVehicles, GrossRevenue = gross, RefundedAmount = refunds, NetRevenue = gross - refunds });
        }

        var bookings = await db.Bookings.Where(item => item.BranchID == branchId && item.ScheduledStart >= day && item.ScheduledStart < dayEnd).ToListAsync();
        var bays = await db.WashBays.CountAsync(item => item.BranchID == branchId && item.Status == WashBayStatusEnum.Active);
        var attendance = await db.AttendanceRecords.CountAsync(item => item.BranchID == branchId && item.WorkDate == day && item.CheckedInAt != null);
        var assigned = await db.BranchStaffMemberships.CountAsync(item => item.BranchID == branchId && item.IsActive);
        var actions = bookings.Where(item => item.BookingStatus == BookingStatusEnum.Pending || (item.ScheduledStart < DateTime.UtcNow && item.BookingStatus is BookingStatusEnum.Confirmed or BookingStatusEnum.Pending) || !item.WashBayID.HasValue || !item.AssignedStaffID.HasValue).Select(item => new DashboardException { Type = "booking", Severity = item.BookingStatus == BookingStatusEnum.Pending ? "warning" : "danger", Title = $"Booking {item.BookingStatus}", Detail = !item.WashBayID.HasValue || !item.AssignedStaffID.HasValue ? "Chưa đủ điều phối bãi hoặc nhân sự." : "Cần xử lý lịch hẹn.", BranchId = branchId, BranchName = branch.BranchName, ActionPath = "/manager/bookings", OccurredAt = item.ScheduledStart }).OrderByDescending(item => item.Severity == "danger").ThenBy(item => item.OccurredAt).Take(12).ToList();

        return new ManagerDashboardResponse
        {
            BranchId = branchId,
            BranchName = branch.BranchName,
            Date = day,
            RevenuePeriods = revenuePeriods,
            Metrics = [new() { Label = "Lịch hẹn", Value = bookings.Count }, new() { Label = "Đang xử lý", Value = bookings.Count(item => item.BookingStatus == BookingStatusEnum.InProgress) }, new() { Label = "Bãi hoạt động", Value = bays }, new() { Label = "Nhân sự check-in", Value = attendance, Format = $"of:{assigned}" }],
            Actions = actions,
        };
    }

    public async Task<AdminDashboardResponse> GetAdminAsync(DateTime from, DateTime to, Guid? branchId)
    {
        var start = from.Date;
        var end = to.Date.AddDays(1);
        var bookings = await db.Bookings.Include(item => item.Branch).Include(item => item.Payments).Where(item => item.ScheduledStart >= start && item.ScheduledStart < end && (!branchId.HasValue || item.BranchID == branchId)).ToListAsync();
        var branches = await db.Branches.Where(item => !branchId.HasValue || item.BranchID == branchId).ToListAsync();
        var exceptions = bookings.Where(item => item.BookingStatus == BookingStatusEnum.Pending || item.BookingStatus == BookingStatusEnum.NoShow || (!item.WashBayID.HasValue && item.BookingStatus == BookingStatusEnum.Confirmed) || item.Payments.Any(payment => payment.PaymentStatus == PaymentStatusEnum.Failed)).Select(item => new DashboardException { Type = item.Payments.Any(payment => payment.PaymentStatus == PaymentStatusEnum.Failed) ? "payment" : "booking", Severity = item.BookingStatus == BookingStatusEnum.NoShow ? "danger" : "warning", Title = item.BookingStatus == BookingStatusEnum.NoShow ? "No-show" : "Booking cần xử lý", Detail = item.Payments.Any(payment => payment.PaymentStatus == PaymentStatusEnum.Failed) ? "Thanh toán thất bại cần đối soát." : $"{item.Branch.BranchName} · {item.BookingStatus}", BranchId = item.BranchID, BranchName = item.Branch.BranchName, ActionPath = item.Payments.Any(payment => payment.PaymentStatus == PaymentStatusEnum.Failed) ? "/admin/reconciliation" : "/admin/dashboard", OccurredAt = item.ScheduledStart }).OrderByDescending(item => item.Severity == "danger").ThenBy(item => item.OccurredAt).Take(30).ToList();
        var performance = branches.Select(branch => { var list = bookings.Where(item => item.BranchID == branch.BranchID).ToList(); var total = list.Count; var paid = list.SelectMany(item => item.Payments).Where(payment => payment.PaymentStatus == PaymentStatusEnum.Paid).Sum(payment => (decimal?)payment.Amount) ?? 0; return new BranchPerformanceResponse { BranchId = branch.BranchID, BranchName = branch.BranchName, Bookings = total, CompletionRate = total == 0 ? 0 : Math.Round(100m * list.Count(item => item.BookingStatus == BookingStatusEnum.Completed) / total, 1), NoShowRate = total == 0 ? 0 : Math.Round(100m * list.Count(item => item.BookingStatus == BookingStatusEnum.NoShow) / total, 1), NetAmount = paid, BayUtilization = 0 }; }).OrderByDescending(item => item.NetAmount).ToList();
        return new AdminDashboardResponse { From = start, To = end.AddTicks(-1), Metrics = [new() { Label = "Tổng booking", Value = bookings.Count }, new() { Label = "Hoàn thành", Value = bookings.Count(item => item.BookingStatus == BookingStatusEnum.Completed) }, new() { Label = "Đã hủy lịch", Value = bookings.Count(item => item.BookingStatus == BookingStatusEnum.Cancelled) }, new() { Label = "No-show", Value = bookings.Count(item => item.BookingStatus == BookingStatusEnum.NoShow) }, new() { Label = "Đã thu", Value = bookings.SelectMany(item => item.Payments).Where(payment => payment.PaymentStatus == PaymentStatusEnum.Paid).Sum(payment => (decimal?)payment.Amount) ?? 0, Format = "currency" }], Exceptions = exceptions, Branches = performance };
    }
}
