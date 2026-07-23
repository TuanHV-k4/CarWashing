using BusinessLayer.Dtos.Vehicle;
using BusinessLayer.Helpers;
using BusinessLayer.IService;
using DataAccessLayer.Context;
using DataAccessLayer.Entity;
using DataAccessLayer.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service
{
    public class VehicleService : IVehicleService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentCustomerService _currentCustomer;
        private readonly IVehicleOwnershipValidator _ownershipValidator;
        private readonly IValidator<CreateVehicleRequestDto> _createValidator;
        private readonly IValidator<UpdateVehicleRequestDto> _updateValidator;

        public VehicleService(
            ApplicationDbContext context,
            ICurrentCustomerService currentCustomer,
            IVehicleOwnershipValidator ownershipValidator,
            IValidator<CreateVehicleRequestDto> createValidator,
            IValidator<UpdateVehicleRequestDto> updateValidator)
        {
            _context = context;
            _currentCustomer = currentCustomer;
            _ownershipValidator = ownershipValidator;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<List<VehicleResponseDto>> GetMyVehiclesAsync()
        {
            var customerId = await _currentCustomer.GetCurrentCustomerIdAsync();
            return await GetVehiclesByCustomerIdAsync(customerId);
        }

        public async Task<List<VehicleResponseDto>> GetVehiclesByCustomerIdAsync(Guid customerId)
        {
            return await _context.Vehicles
                .Where(v => v.CustomerID == customerId)
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => MapToDto(v))
                .ToListAsync();
        }

        public async Task<VehicleResponseDto> CreateVehicleAsync(CreateVehicleRequestDto request)
        {
            var validation = await _createValidator.ValidateAsync(request);
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var customerId = await _currentCustomer.GetCurrentCustomerIdAsync();
            var normalizedPlate = NormalizationHelper.NormalizeLicensePlate(request.LicensePlate);

            var plateExists = await _context.Vehicles.AnyAsync(v => v.LicensePlate == normalizedPlate);
            if (plateExists)
                throw new InvalidOperationException("This license plate is already registered.");

            var vehicle = new Vehicle
            {
                CustomerID = customerId,
                LicensePlate = normalizedPlate,
                VehicleType = request.VehicleType,
                Brand = request.Brand?.Trim(),
                Model = request.Model?.Trim(),
                Color = request.Color?.Trim(),
                Status = VehicleStatusEnum.Active
            };

            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();

            return MapToDto(vehicle);
        }

        public async Task<VehicleResponseDto> UpdateVehicleAsync(Guid vehicleId, UpdateVehicleRequestDto request)
        {
            var validation = await _updateValidator.ValidateAsync(request);
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var customerId = await _currentCustomer.GetCurrentCustomerIdAsync();
            await _ownershipValidator.EnsureOwnedByCustomerAsync(vehicleId, customerId);

            var vehicle = await _context.Vehicles.FindAsync(vehicleId)
                ?? throw new KeyNotFoundException("Vehicle not found.");

            if (request.VehicleType.HasValue) vehicle.VehicleType = request.VehicleType;
            if (request.Brand != null) vehicle.Brand = request.Brand.Trim();
            if (request.Model != null) vehicle.Model = request.Model.Trim();
            if (request.Color != null) vehicle.Color = request.Color.Trim();

            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();

            return MapToDto(vehicle);
        }

        public async Task<VehicleResponseDto> UpdateVehicleStatusAsync(Guid vehicleId, UpdateVehicleStatusRequestDto request)
        {
            var customerId = await _currentCustomer.GetCurrentCustomerIdAsync();
            await _ownershipValidator.EnsureOwnedByCustomerAsync(vehicleId, customerId);

            var vehicle = await _context.Vehicles.FindAsync(vehicleId)
                ?? throw new KeyNotFoundException("Vehicle not found.");

            vehicle.Status = request.Status;
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();

            return MapToDto(vehicle);
        }

        private static VehicleResponseDto MapToDto(Vehicle v) => new()
        {
            VehicleID = v.VehicleID,
            CustomerID = v.CustomerID,
            LicensePlate = v.LicensePlate,
            VehicleType = v.VehicleType,
            Brand = v.Brand,
            Model = v.Model,
            Color = v.Color,
            Status = v.Status,
            CreatedAt = v.CreatedAt
        };
    }
}
