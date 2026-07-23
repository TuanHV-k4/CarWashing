namespace DataAccessLayer.Entity;

public class Refund
{
    public Guid RefundID { get; set; } = Guid.NewGuid();
    public Guid PaymentID { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = null!;
    public string Status { get; set; } = "Pending";
    public string? ReferenceNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public Payment Payment { get; set; } = null!;
}
