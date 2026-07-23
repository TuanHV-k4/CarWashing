using BusinessLayer.Dtos.AI;

namespace BusinessLayer.IService
{
    public interface IAdminDashboardReadService
    {
        Task<AdminDashboardStatsDto> GetDashboardStatsAsync();
    }

    public class AdminDashboardStatsDto
    {
        public int TotalCustomers { get; set; }
        public int TotalActiveUsers { get; set; }
        public int TotalVehicles { get; set; }
        public int TotalBookingsToday { get; set; }
        public List<TierSummaryDto> TierDistribution { get; set; } = [];
    }
}
