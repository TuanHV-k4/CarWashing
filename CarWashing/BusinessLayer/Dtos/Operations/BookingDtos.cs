namespace BusinessLayer.Dtos.Operations
{
    public class CreateBookingRequest
    {
        public Guid VehicleId { get; set; }
        public Guid BranchId { get; set; }
        public Guid ServiceId { get; set; }
        public DateTimeOffset BookingStartTime { get; set; }
        public string? Note { get; set; }
        public IReadOnlyList<BookingServiceItemRequest> Items { get; set; } = Array.Empty<BookingServiceItemRequest>();
    }

    public class CancelBookingRequest
    {
        public string? Reason { get; set; }
    }

    public class RescheduleBookingRequest
    {
        public DateTimeOffset BookingStartTime { get; set; }
        public Guid? WashBayId { get; set; }
        public string? Note { get; set; }
        public uint? ExpectedVersion { get; set; }
    }

    public class BookingServiceItemRequest { public Guid ServiceId { get; set; } public int Quantity { get; set; } = 1; }

    public class BookingAvailabilityResponse
    {
        public Guid BranchId { get; set; }
        public Guid ServiceId { get; set; }
        public int DurationMinutes { get; set; }
        public IReadOnlyList<DateTime> AvailableStartTimes { get; set; } = Array.Empty<DateTime>();
        public IReadOnlyList<BookingAvailabilitySlotResponse> Slots { get; set; } = Array.Empty<BookingAvailabilitySlotResponse>();
    }

    public class BookingAvailabilityRequest
    {
        public Guid BranchId { get; set; }
        public DateOnly Date { get; set; }
        public IReadOnlyList<BookingServiceItemRequest> Items { get; set; } = Array.Empty<BookingServiceItemRequest>();
    }

    public class BookingAvailabilitySlotResponse
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int AvailableBayCount { get; set; }
    }

    public class BookingLineItemResponse
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public int DurationMinutesPerUnit { get; set; }
    }

    public class DispatchBookingRequest
    {
        public Guid WashBayId { get; set; }
    }

    public class AssignBookingStaffRequest
    {
        public Guid StaffUserId { get; set; }
        public uint? ExpectedVersion { get; set; }
    }

    public class BookingStaffWorkItemRequest
    {
        public Guid StaffUserId { get; set; }
        public decimal ContributionPercent { get; set; }
        public string? WorkRole { get; set; }
    }

    public class SetBookingStaffWorkRequest
    {
        public IReadOnlyList<BookingStaffWorkItemRequest> Staff { get; set; } = Array.Empty<BookingStaffWorkItemRequest>();
        public string? AdjustmentReason { get; set; }
        public uint? ExpectedVersion { get; set; }
    }

    public class BookingStaffWorkResponse
    {
        public Guid StaffUserId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public decimal ContributionPercent { get; set; }
        public string? WorkRole { get; set; }
    }

    public class EligibleBookingStaffResponse
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public Guid MembershipId { get; set; }
    }

    public class BookingRescheduleHistoryResponse
    {
        public Guid Id { get; set; }
        public DateTime PreviousStart { get; set; }
        public DateTime NewStart { get; set; }
        public Guid? PreviousWashBayId { get; set; }
        public Guid? NewWashBayId { get; set; }
        public string? Note { get; set; }
        public DateTime ChangedAt { get; set; }
    }

    public class QueueBookingResponse
    {
        public Guid BookingId { get; set; }
        public Guid BranchId { get; set; }
        public Guid? WashBayId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public DateTime ScheduledStart { get; set; }
        public DateTime CheckedInAt { get; set; }
        public int Priority { get; set; }
        public int Position { get; set; }
        public DateTime EstimatedStart { get; set; }
    }

    public class BookingResponse
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid VehicleId { get; set; }
        public Guid BranchId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid? WashBayId { get; set; }
        public Guid? AssignedStaffId { get; set; }
        public string? AssignedStaffName { get; set; }
        public IReadOnlyList<BookingStaffWorkResponse> StaffWork { get; set; } = Array.Empty<BookingStaffWorkResponse>();
        public uint Version { get; set; }
        public DateTime BookingStartTime { get; set; }
        public DateTime BookingEndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string ServiceNameSnapshot { get; set; } = string.Empty;
        public int DurationMinutesSnapshot { get; set; }
        public decimal PriceSnapshot { get; set; }
        public string? TierSnapshot { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public string? Note { get; set; }
        public IReadOnlyList<BookingLineItemResponse> Items { get; set; } = Array.Empty<BookingLineItemResponse>();
    }

    public class BookingListItemResponse : BookingResponse
    {
        public string CustomerName { get; set; } = string.Empty;
        public string VehiclePlate { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
    }

    public class BookingDetailResponse : BookingResponse
    {
        public string BranchName { get; set; } = string.Empty;
        public string? WashBayName { get; set; }
    }
}
