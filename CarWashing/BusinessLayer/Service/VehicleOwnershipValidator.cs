using BusinessLayer.IService;
using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service
{
    public class VehicleOwnershipValidator : IVehicleOwnershipValidator
    {
        private readonly ApplicationDbContext _context;

        public VehicleOwnershipValidator(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsOwnedByCustomerAsync(Guid vehicleId, Guid customerId)
        {
            return await _context.Vehicles
                .AnyAsync(v => v.VehicleID == vehicleId && v.CustomerID == customerId);
        }

        public async Task EnsureOwnedByCustomerAsync(Guid vehicleId, Guid customerId)
        {
            if (!await IsOwnedByCustomerAsync(vehicleId, customerId))
                throw new UnauthorizedAccessException("You do not own this vehicle.");
        }
    }
}
