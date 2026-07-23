using DataAccessLayer.Enums;

namespace DataAccessLayer.Entity;

public class AttendanceAdjustment
{
    public Guid AttendanceAdjustmentID { get; set; } = Guid.NewGuid();
    public Guid AttendanceRecordID { get; set; }
    public AttendanceAdjustmentActionEnum Action { get; set; }
    public string PreviousValues { get; set; } = "{}";
    public string NewValues { get; set; } = "{}";
    public string Reason { get; set; } = null!;
    public Guid AdjustedByUserID { get; set; }
    public DateTime AdjustedAt { get; set; } = DateTime.UtcNow;
    public AttendanceRecord AttendanceRecord { get; set; } = null!;
    public User AdjustedByUser { get; set; } = null!;
}
