using DataAccessLayer.Enums;

namespace BusinessLayer.Dtos
{
    public class ServiceResponseDto
    {
        public Guid ServiceID { get; set; }
        public string ServiceName { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public TimeSpan? EstimatedDuration { get; set; }
        public ServiceStatusEnum Status { get; set; }
    }

    public class CreateServiceDto
    {
        public string ServiceName { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public TimeSpan? EstimatedDuration { get; set; }
        public ServiceStatusEnum Status { get; set; } = ServiceStatusEnum.Active;
    }

    public class UpdateServiceDto
    {
        public string ServiceName { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public TimeSpan? EstimatedDuration { get; set; }
        public ServiceStatusEnum Status { get; set; }
    }

    public class PatchServiceDto
    {
        public string? ServiceName { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public TimeSpan? EstimatedDuration { get; set; }
        public ServiceStatusEnum? Status { get; set; }
    }
}
