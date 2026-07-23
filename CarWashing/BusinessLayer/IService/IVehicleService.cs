using BusinessLayer.Dtos.Vehicle;

namespace BusinessLayer.IService
{
    public interface IVehicleService
    {
        Task<List<VehicleResponseDto>> GetMyVehiclesAsync();
        Task<VehicleResponseDto> CreateVehicleAsync(CreateVehicleRequestDto request);
        Task<VehicleResponseDto> UpdateVehicleAsync(Guid vehicleId, UpdateVehicleRequestDto request);
        Task<VehicleResponseDto> UpdateVehicleStatusAsync(Guid vehicleId, UpdateVehicleStatusRequestDto request);
        Task<List<VehicleResponseDto>> GetVehiclesByCustomerIdAsync(Guid customerId);
    }
}
