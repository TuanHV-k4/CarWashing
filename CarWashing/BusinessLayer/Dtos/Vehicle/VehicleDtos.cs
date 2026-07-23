using DataAccessLayer.Enums;

namespace BusinessLayer.Dtos.Vehicle
{
    public class VehicleResponseDto
    {
        public Guid VehicleID { get; set; }
        public Guid CustomerID { get; set; }
        public string LicensePlate { get; set; } = null!;
        public VehicleTypeEnum? VehicleType { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Color { get; set; }
        public VehicleStatusEnum Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateVehicleRequestDto
    {
        public string LicensePlate { get; set; } = null!;
        public VehicleTypeEnum? VehicleType { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Color { get; set; }
    }

    public class UpdateVehicleRequestDto
    {
        public VehicleTypeEnum? VehicleType { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Color { get; set; }
    }

    public class UpdateVehicleStatusRequestDto
    {
        public VehicleStatusEnum Status { get; set; }
    }
}
