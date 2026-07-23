namespace BusinessLayer.IService.Operations
{
    public class WashCompletionPayload
    {
        public Guid BookingId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid VehicleId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid BranchId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CompletedAt { get; set; }
    }

    public interface IWashCompletionService
    {
        Task CompleteWashAsync(WashCompletionPayload payload);
    }
}
