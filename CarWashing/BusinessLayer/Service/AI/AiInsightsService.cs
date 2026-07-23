using BusinessLayer.Dtos.AI;
using BusinessLayer.IService.AI;
using DataAccessLayer.Context;
using DataAccessLayer.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service.AI;

public sealed class AiInsightsService(ApplicationDbContext db) : IAiInsightsService
{
    public async Task<AiCustomerAssistantResponseDto> GetCustomerAssistantAsync(Guid customerId, AiCustomerAssistantRequestDto request, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var customer = await db.Customers.AsNoTracking().Include(x => x.Tier).FirstOrDefaultAsync(x => x.CustomerID == customerId, ct)
            ?? throw new InvalidOperationException("Customer was not found.");
        var lastWash = await db.WashHistories.AsNoTracking().Where(x => x.Booking.CustomerID == customerId).OrderByDescending(x => x.WashDate).FirstOrDefaultAsync(ct);
        var services = await db.Services.AsNoTracking().Where(x => x.Status == ServiceStatusEnum.Active).OrderBy(x => x.Price).Take(3).ToListAsync(ct);
        var response = new AiCustomerAssistantResponseDto
        {
            LoyaltySummary = $"Bạn có {customer.CurrentPoints:N0} điểm" + (customer.Tier is null ? "." : $", hạng {customer.Tier.TierName}."),
            CareTip = lastWash is null ? "Hãy chọn gói phù hợp nhu cầu hiện tại; bạn luôn có thể xác nhận lại trước khi đặt lịch." : $"Lần rửa gần nhất là {lastWash.WashDate:dd/MM/yyyy}; bạn có thể đặt lại hoặc chọn gói chăm sóc nâng cao.",
            Recommendations = services.Select((x, i) => new AiSuggestedServiceDto { ServiceId = x.ServiceID, ServiceName = x.ServiceName, Price = x.Price, Reason = i == 0 && lastWash is null ? "Phù hợp để bắt đầu chăm sóc xe" : !string.IsNullOrWhiteSpace(request.Preference) ? $"Phù hợp nhu cầu: {request.Preference}" : "Dựa trên danh mục dịch vụ đang hoạt động" }).ToList()
        };
        response.EligibleOffers = await db.PromotionCustomers.AsNoTracking().Where(x => x.CustomerID == customerId && !x.IsUsed && (x.ExpiresAt == null || x.ExpiresAt >= now) && x.Promotion.Status == PromotionStatusEnum.Active && x.Promotion.StartDate <= now && x.Promotion.EndDate >= now)
            .OrderBy(x => x.ExpiresAt).Take(3).Select(x => new AiOfferDto { PromotionId = x.PromotionID, Name = x.Promotion.PromotionName, Description = x.Promotion.Description, ExpiresAt = x.ExpiresAt ?? x.Promotion.EndDate, EligibilityNote = "Ưu đãi hiện đang khả dụng; hệ thống sẽ kiểm tra lại trước khi áp dụng." }).ToListAsync(ct);
        if (request.BranchId is not null && request.Date is not null)
            response.SuggestedSlots = await SuggestSlotsAsync(request, ct);
        return response;
    }

    public async Task<AiFeedbackInsightsResponseDto> GetFeedbackInsightsAsync(Guid? requestedBranchId, Guid userId, bool isAdmin, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var branchId = await ResolveBranchAsync(requestedBranchId, userId, isAdmin, ct);
        var query = db.WashHistories.AsNoTracking().Where(x => x.WashDate >= from && x.WashDate <= to && x.CustomerRating != null);
        if (branchId is not null) query = query.Where(x => x.Booking.BranchID == branchId);
        var rows = await query.OrderBy(x => x.CustomerRating).ThenByDescending(x => x.WashDate).Select(x => new { x.WashHistoryID, x.BookingID, Rating = x.CustomerRating!.Value, x.Feedback, x.WashDate }).ToListAsync(ct);
        var themeNames = new[] { "wash quality", "waiting time", "staff attitude", "price", "other" };
        string Theme(string? value) { var text = (value ?? string.Empty).ToLowerInvariant(); if (text.Contains("chờ") || text.Contains("doi") || text.Contains("lâu")) return "waiting time"; if (text.Contains("nhân viên") || text.Contains("thái độ") || text.Contains("staff")) return "staff attitude"; if (text.Contains("giá") || text.Contains("price") || text.Contains("đắt")) return "price"; if (text.Contains("rửa") || text.Contains("sạch") || text.Contains("bẩn") || text.Contains("wash")) return "wash quality"; return "other"; }
        return new AiFeedbackInsightsResponseDto { From = from, To = to, BranchId = branchId, FeedbackCount = rows.Count, RatingDistribution = rows.GroupBy(x => x.Rating).ToDictionary(x => x.Key, x => x.Count()), Themes = rows.Where(x => !string.IsNullOrWhiteSpace(x.Feedback)).GroupBy(x => Theme(x.Feedback)).OrderByDescending(x => x.Count()).Select(x => new AiFeedbackThemeDto { Theme = x.Key, Count = x.Count() }).ToList(), LowRatings = rows.Where(x => x.Rating <= 2).Take(20).Select(x => new AiFeedbackRecordDto { WashHistoryId = x.WashHistoryID, BookingId = x.BookingID, Rating = x.Rating, FeedbackPreview = x.Feedback is null ? null : x.Feedback.Length <= 180 ? x.Feedback : x.Feedback[..180] + "…", WashDate = x.WashDate }).ToList() };
    }

    public async Task<AiOperationsCopilotResponseDto> AskOperationsAsync(Guid userId, bool isAdmin, AiOperationsCopilotRequestDto request, CancellationToken ct = default)
    {
        if (request.To < request.From) throw new InvalidOperationException("The end date must not be before the start date.");
        var branchId = await ResolveBranchAsync(request.BranchId, userId, isAdmin, ct);
        var query = db.Bookings.AsNoTracking().Where(x => x.ScheduledStart >= request.From && x.ScheduledStart <= request.To);
        if (branchId is not null) query = query.Where(x => x.BranchID == branchId);
        var bookings = await query.Select(x => new { x.BookingStatus, x.EstimatedTotalAmount }).ToListAsync(ct);
        var completed = bookings.Count(x => x.BookingStatus == BookingStatusEnum.Completed);
        var noShows = bookings.Count(x => x.BookingStatus == BookingStatusEnum.NoShow || x.BookingStatus == BookingStatusEnum.Cancelled);
        var revenue = bookings.Where(x => x.BookingStatus == BookingStatusEnum.Completed).Sum(x => x.EstimatedTotalAmount);
        var text = request.Message.ToLowerInvariant();
        var answer = text.Contains("doanh thu") || text.Contains("revenue") ? $"Doanh thu ước tính của booking hoàn tất là {revenue:N0}₫ trong phạm vi đã chọn." : text.Contains("vắng") || text.Contains("hủy") || text.Contains("no-show") ? $"Có {noShows} booking hủy hoặc không đến trong phạm vi đã chọn." : $"Có {bookings.Count} booking, {completed} hoàn tất và {noShows} hủy/no-show trong phạm vi đã chọn.";
        return new AiOperationsCopilotResponseDto { Answer = answer, BranchId = branchId, Evidence = [new AiEvidenceDto { Label = "Booking", Value = bookings.Count.ToString(), Period = $"{request.From:dd/MM} - {request.To:dd/MM}" }, new AiEvidenceDto { Label = "Hoàn tất", Value = completed.ToString(), Period = "Cùng kỳ" }, new AiEvidenceDto { Label = "Hủy / no-show", Value = noShows.ToString(), Period = "Cùng kỳ" }], Actions = [new AiActionDto { Label = "Mở booking board", Path = "/manager/bookings" }, new AiActionDto { Label = "Xem dashboard", Path = isAdmin ? "/admin/dashboard" : "/manager/dashboard" }] };
    }

    private async Task<List<AiSlotSuggestionDto>> SuggestSlotsAsync(AiCustomerAssistantRequestDto request, CancellationToken ct)
    {
        var branch = await db.Branches.AsNoTracking().FirstOrDefaultAsync(x => x.BranchID == request.BranchId, ct);
        if (branch is null) return [];
        var duration = await db.Services.AsNoTracking().Where(x => request.ServiceIds.Contains(x.ServiceID)).SumAsync(x => (double?)x.EstimatedDuration!.Value.TotalMinutes, ct) ?? 60;
        var day = request.Date!.Value.ToDateTime(TimeOnly.FromTimeSpan(branch.OpenTime ?? TimeSpan.FromHours(8)), DateTimeKind.Utc);
        var close = request.Date!.Value.ToDateTime(TimeOnly.FromTimeSpan(branch.CloseTime ?? TimeSpan.FromHours(18)), DateTimeKind.Utc);
        var bays = await db.WashBays.CountAsync(x => x.BranchID == request.BranchId && x.Status == WashBayStatusEnum.Active, ct);
        if (bays == 0) return [];
        var candidates = new List<AiSlotSuggestionDto>();
        for (var start = day; start.AddMinutes(duration) <= close; start = start.AddHours(1))
        {
            var end = start.AddMinutes(duration);
            var busy = await db.Bookings.CountAsync(x => x.BranchID == request.BranchId && x.BookingStatus != BookingStatusEnum.Cancelled && x.BookingStatus != BookingStatusEnum.NoShow && x.ScheduledStart < end && x.ScheduledEnd > start, ct);
            if (busy < bays) candidates.Add(new AiSlotSuggestionDto { StartTime = start, EndTime = end, AvailableBayCount = bays - busy, Reason = "Khung giờ còn chỗ theo công suất bãi hiện tại" });
        }
        return candidates.OrderByDescending(x => x.AvailableBayCount).ThenBy(x => x.StartTime).Take(3).ToList();
    }

    private async Task<Guid?> ResolveBranchAsync(Guid? requestedBranchId, Guid userId, bool isAdmin, CancellationToken ct)
    {
        if (isAdmin) return requestedBranchId;
        var branch = await db.BranchManagerMemberships.AsNoTracking().Where(x => x.UserID == userId && x.IsActive).Select(x => (Guid?)x.BranchID).FirstOrDefaultAsync(ct);
        if (branch is null) throw new UnauthorizedAccessException("No active branch-manager membership was found.");
        return branch;
    }
}
