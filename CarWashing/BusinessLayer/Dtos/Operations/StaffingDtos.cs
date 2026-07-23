namespace BusinessLayer.Dtos.Operations;

public class CreateStaffShiftRequest
{
    public Guid BranchId { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UpdateStaffShiftRequest
{
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class AssignStaffToShiftRequest
{
    public Guid UserId { get; set; }
    public Guid? WashBayId { get; set; }
}

public class ShiftAssignmentResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public Guid? WashBayId { get; set; }
    public string? WashBayName { get; set; }
}

public class StaffShiftResponse
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<ShiftAssignmentResponse> Assignments { get; set; } = Array.Empty<ShiftAssignmentResponse>();
}

public class AvailableStaffResponse
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public Guid ShiftId { get; set; }
    public Guid? WashBayId { get; set; }
    public string? WashBayName { get; set; }
}

public class StaffShiftCapacityResponse
{
    public Guid ShiftId { get; set; }
    public int AssignedStaffCount { get; set; }
    public int AssignedBayCount { get; set; }
    public int ActiveBayCount { get; set; }
    public int AvailableStaffCount { get; set; }
}
