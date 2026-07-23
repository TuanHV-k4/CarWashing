using DataAccessLayer.Enums;

namespace DataAccessLayer.Entity;

public class AttendanceRecord
{
    public Guid AttendanceRecordID { get; set; } = Guid.NewGuid();
    public Guid? ShiftAssignmentID { get; set; }
    public Guid? BranchID { get; set; }
    public Guid? BranchStaffMembershipID { get; set; }
    public DateTime? WorkDate { get; set; }
    public Guid? CheckedInByUserID { get; set; }
    public Guid? CheckedOutByUserID { get; set; }
    public AttendanceStatusEnum Status { get; set; } = AttendanceStatusEnum.CheckedIn;
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
    public AttendanceSourceEnum? CheckInSource { get; set; }
    public AttendanceSourceEnum? CheckOutSource { get; set; }
    public int LateMinutes { get; set; }
    public int EarlyLeaveMinutes { get; set; }
    public int WorkedMinutes { get; set; }
    public int OvertimeMinutes { get; set; }
    public string? AdminNote { get; set; }
    public DateTime? LockedAt { get; set; }
    public Guid? LockedByUserID { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ShiftAssignment? ShiftAssignment { get; set; }
    public Branch? Branch { get; set; }
    public BranchStaffMembership? BranchStaffMembership { get; set; }
    public User? CheckedInByUser { get; set; }
    public User? CheckedOutByUser { get; set; }
    public User? LockedByUser { get; set; }
    public ICollection<AttendanceAdjustment> Adjustments { get; set; } = new HashSet<AttendanceAdjustment>();
}
