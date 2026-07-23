namespace BusinessLayer.Dtos.Operations;

public class AttendanceResponse
{
    public Guid Id { get; set; }
    public Guid ShiftAssignmentId { get; set; }
    public Guid StaffUserId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public Guid ShiftId { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string? WashBayName { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
    public int LateMinutes { get; set; }
    public int EarlyLeaveMinutes { get; set; }
    public int WorkedMinutes { get; set; }
    public bool IsLocked { get; set; }
    public string? AdminNote { get; set; }
}

public class AttendanceAdjustmentRequest
{
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
    public string? Status { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? AdminNote { get; set; }
}

public class AttendanceActionRequest { public string Reason { get; set; } = string.Empty; }

public class AttendanceSummaryResponse
{
    public string GroupKey { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int AssignedShiftCount { get; set; }
    public int CompletedShiftCount { get; set; }
    public int LateCount { get; set; }
    public int AbsentCount { get; set; }
    public int PlannedMinutes { get; set; }
    public int WorkedMinutes { get; set; }
}
