using BusinessLayer.Dtos.AI;
using BusinessLayer.IService;
using BusinessLayer.IService.AI;

namespace BusinessLayer.Service.AI
{
    public class CustomerAIContextProvider : ICustomerAIContextProvider, IServiceSuggestionContextProvider
    {
        private readonly ICustomerService _customerService;
        private readonly IWashHistoryService _washHistoryService;
        private readonly IServiceCatalogService _serviceCatalog;
        private readonly IVehicleService _vehicleService;

        public CustomerAIContextProvider(
            ICustomerService customerService,
            IWashHistoryService washHistoryService,
            IServiceCatalogService serviceCatalog,
            IVehicleService vehicleService)
        {
            _customerService = customerService;
            _washHistoryService = washHistoryService;
            _serviceCatalog = serviceCatalog;
            _vehicleService = vehicleService;
        }

        public async Task<CustomerAiContextDto> GetContextAsync(Guid customerId)
        {
            var profile = await _customerService.GetProfileByCustomerIdAsync(customerId);
            var history = await _washHistoryService.GetHistoryByCustomerIdAsync(customerId, 1, 5);
            var services = await _serviceCatalog.GetActiveServicesAsync();
            var vehicles = await _vehicleService.GetVehiclesByCustomerIdAsync(customerId);

            return new CustomerAiContextDto
            {
                CustomerName = profile.FullName,
                CurrentPoints = profile.CurrentPoints,
                TierName = profile.TierName,
                TotalVisits = profile.TotalVisits,
                TotalSpent = profile.TotalSpent,
                Vehicles = vehicles
                    .Where(v => v.Status == DataAccessLayer.Enums.VehicleStatusEnum.Active)
                    .Select(v => v.LicensePlate + (v.Brand != null ? $" ({v.Brand} {v.Model})" : ""))
                    .ToList(),
                Perks = profile.TierPerks,
                RecentWashes = history.Items.Select(h => new CustomerWashHistoryContextDto
                {
                    WashDate = h.WashDate,
                    Services = h.Services.Count > 0 ? string.Join(", ", h.Services) : null,
                    FinalAmount = h.FinalAmount
                }).ToList(),
                AvailableServices = services
            };
        }
    }
}
