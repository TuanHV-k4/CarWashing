namespace DataAccessLayer.Entity;

public class StaffShift
{
    public Guid StaffShiftID { get; set; } = Guid.NewGuid();
    public Guid BranchID { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public Branch Branch { get; set; } = null!;
    public ICollection<ShiftAssignment> Assignments { get; set; } = new HashSet<ShiftAssignment>();
}
