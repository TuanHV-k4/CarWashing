namespace DataAccessLayer.Entity;

public class ShiftAssignment
{
    public Guid ShiftAssignmentID { get; set; } = Guid.NewGuid();
    public Guid StaffShiftID { get; set; }
    public Guid UserID { get; set; }
    public Guid? WashBayID { get; set; }
    public StaffShift StaffShift { get; set; } = null!;
    public User User { get; set; } = null!;
    public WashBay? WashBay { get; set; }
    public AttendanceRecord? AttendanceRecord { get; set; }
}
