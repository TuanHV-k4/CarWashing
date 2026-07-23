namespace DataAccessLayer.Entity;

public class BookingRescheduleHistory
{
    public Guid BookingRescheduleHistoryID { get; set; } = Guid.NewGuid();
    public Guid BookingID { get; set; }
    public DateTime PreviousStart { get; set; }
    public DateTime PreviousEnd { get; set; }
    public Guid? PreviousWashBayID { get; set; }
    public DateTime NewStart { get; set; }
    public DateTime NewEnd { get; set; }
    public Guid? NewWashBayID { get; set; }
    public Guid? ChangedByCustomerID { get; set; }
    public string? Note { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public Booking Booking { get; set; } = null!;
}
