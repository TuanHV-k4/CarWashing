using BusinessLayer.Dtos.Operations;

namespace BusinessLayer.IService.Operations
{
    public class BookingReadSnapshot
    {
        public Guid BookingId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid VehicleId { get; set; }
        public Guid BranchId { get; set; }
        public Guid ServiceId { get; set; }
        public DateTime ScheduledStart { get; set; }
        public DateTime ScheduledEnd { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string ServiceName { get; set; } = string.Empty;
    }

    public interface IBookingReadService
    {
        Task<OperationResult<BookingReadSnapshot>> GetSnapshotAsync(Guid bookingId);
    }
}
