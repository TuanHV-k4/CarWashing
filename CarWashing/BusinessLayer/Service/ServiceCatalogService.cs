using BusinessLayer.Dtos.AI;
using BusinessLayer.IService;
using DataAccessLayer.Context;
using DataAccessLayer.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service
{
    public class ServiceCatalogService : IServiceCatalogService
    {
        private readonly ApplicationDbContext _context;

        public ServiceCatalogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ServiceCatalogItemDto>> GetActiveServicesAsync()
        {
            return await _context.Services
                .Where(s => s.Status == ServiceStatusEnum.Active)
                .OrderBy(s => s.Price)
                .Select(s => new ServiceCatalogItemDto
                {
                    ServiceId = s.ServiceID,
                    ServiceName = s.ServiceName,
                    Description = s.Description,
                    Price = s.Price
                })
                .ToListAsync();
        }

        public async Task<bool> IsValidActiveServiceIdAsync(Guid serviceId)
        {
            return await _context.Services
                .AnyAsync(s => s.ServiceID == serviceId && s.Status == ServiceStatusEnum.Active);
        }

        public async Task<List<ServiceCatalogItemDto>> GetServicesByIdsAsync(IEnumerable<Guid> serviceIds)
        {
            var ids = serviceIds.ToList();
            return await _context.Services
                .Where(s => ids.Contains(s.ServiceID) && s.Status == ServiceStatusEnum.Active)
                .Select(s => new ServiceCatalogItemDto
                {
                    ServiceId = s.ServiceID,
                    ServiceName = s.ServiceName,
                    Description = s.Description,
                    Price = s.Price
                })
                .ToListAsync();
        }
    }
}
