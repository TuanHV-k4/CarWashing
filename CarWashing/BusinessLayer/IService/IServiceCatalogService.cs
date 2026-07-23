using BusinessLayer.Dtos.AI;

namespace BusinessLayer.IService
{
    public interface IServiceCatalogService
    {
        Task<List<ServiceCatalogItemDto>> GetActiveServicesAsync();
        Task<bool> IsValidActiveServiceIdAsync(Guid serviceId);
        Task<List<ServiceCatalogItemDto>> GetServicesByIdsAsync(IEnumerable<Guid> serviceIds);
    }
}
