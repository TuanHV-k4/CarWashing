namespace DataAccessLayer.Entity;

/// <summary>Phần công của một nhân viên trên một lượt rửa. Tổng phần công của booking luôn bằng 100%.</summary>
public class BookingStaffWork
{
    public Guid BookingStaffWorkID { get; set; } = Guid.NewGuid();
    public Guid BookingID { get; set; }
    public Guid StaffUserID { get; set; }
    public decimal ContributionPercent { get; set; }
    public string? WorkRole { get; set; }
    public string? AdjustmentReason { get; set; }
    public Guid AssignedByUserID { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Booking Booking { get; set; } = null!;
    public User StaffUser { get; set; } = null!;
    public User AssignedByUser { get; set; } = null!;
}
