namespace BusinessLayer.Dtos.Operations
{
    public class CreatePaymentRequest
    {
        public Guid BookingId { get; set; }
        public Guid BranchId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class MarkPaymentPaidRequest
    {
        public string? ReferenceNumber { get; set; }
        public string? Note { get; set; }
    }

    public class VoidPaymentRequest
    {
        public string? Note { get; set; }
    }

    public class PaymentResponse
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public Guid BranchId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Note { get; set; }
    }

    public class PaymentListItemResponse : PaymentResponse
    {
        public decimal RefundedAmount { get; set; }
        public decimal RefundableAmount { get; set; }
    }

    public class CreateRefundRequest
    {
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
    }

    public class RefundResponse
    {
        public Guid Id { get; set; }
        public Guid PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class PaymentReconciliationResponse
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public Guid? BranchId { get; set; }
        public int PaymentCount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RefundedAmount { get; set; }
        public decimal NetAmount { get; set; }
    }
}
