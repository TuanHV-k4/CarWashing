using BusinessLayer.Dtos.AI;
using BusinessLayer.IService;
using BusinessLayer.IService.AI;

namespace BusinessLayer.Service.AI
{
    public class AdminAIContextProvider : IAdminAIContextProvider
    {
        private readonly IServiceCatalogService _serviceCatalog;
        private readonly IAdminDashboardReadService _dashboardRead;

        public AdminAIContextProvider(
            IServiceCatalogService serviceCatalog,
            IAdminDashboardReadService dashboardRead)
        {
            _serviceCatalog = serviceCatalog;
            _dashboardRead = dashboardRead;
        }

        public async Task<AdminAiContextDto> GetContextAsync()
        {
            var stats = await _dashboardRead.GetDashboardStatsAsync();
            var services = await _serviceCatalog.GetActiveServicesAsync();

            return new AdminAiContextDto
            {
                TotalCustomers = stats.TotalCustomers,
                TotalActiveUsers = stats.TotalActiveUsers,
                TotalVehicles = stats.TotalVehicles,
                TotalBookingsToday = stats.TotalBookingsToday,
                TierDistribution = stats.TierDistribution,
                ActiveServices = services
            };
        }
    }
}
