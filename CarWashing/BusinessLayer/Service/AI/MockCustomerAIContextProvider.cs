using BusinessLayer.Dtos.AI;
using BusinessLayer.IService.AI;

namespace BusinessLayer.Service.AI
{
    /// <summary>Mock context for AI development when real customer data is unavailable.</summary>
    public class MockCustomerAIContextProvider : ICustomerAIContextProvider, IServiceSuggestionContextProvider
    {
        public Task<CustomerAiContextDto> GetContextAsync(Guid customerId)
        {
            return Task.FromResult(new CustomerAiContextDto
            {
                CustomerName = "Demo Customer",
                CurrentPoints = 450,
                TierName = "Silver",
                TotalVisits = 5,
                TotalSpent = 850000,
                Vehicles = ["51A12345 (Toyota Vios)"],
                Perks = ["5% discount on basic wash"],
                RecentWashes =
                [
                    new CustomerWashHistoryContextDto
                    {
                        WashDate = DateTime.UtcNow.AddDays(-7),
                        Services = "Basic Wash, Premium Wash",
                        FinalAmount = 230000
                    }
                ],
                AvailableServices =
                [
                    new ServiceCatalogItemDto { ServiceId = Guid.NewGuid(), ServiceName = "Basic Wash", Price = 80000, Description = "Exterior wash" },
                    new ServiceCatalogItemDto { ServiceId = Guid.NewGuid(), ServiceName = "Premium Wash", Price = 150000, Description = "Wash and wax" }
                ]
            });
        }
    }
}
