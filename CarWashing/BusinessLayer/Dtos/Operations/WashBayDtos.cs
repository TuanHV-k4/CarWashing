namespace BusinessLayer.Dtos.Operations
{
    public class CreateWashBayRequest
    {
        public Guid BranchId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateWashBayRequest
    {
        public Guid BranchId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class WashBayResponse
    {
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class WashBayListItemResponse : WashBayResponse
    {
        public string BranchName { get; set; } = string.Empty;
        public int ActiveBookingCount { get; set; }
        public DateTime? NextBookingAt { get; set; }
    }
}
