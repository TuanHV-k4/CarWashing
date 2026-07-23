using BusinessLayer.Dtos.AI;
using BusinessLayer.IService;
using DataAccessLayer.Context;
using DataAccessLayer.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service
{
    public class AdminDashboardReadService : IAdminDashboardReadService
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardReadService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var tierDistribution = await _context.LoyaltyTiers
                .Where(t => t.Status == LoyaltyTierStatusEnum.Active)
                .Select(t => new TierSummaryDto
                {
                    TierName = t.TierName,
                    CustomerCount = t.Customers.Count
                })
                .ToListAsync();

            return new AdminDashboardStatsDto
            {
                TotalCustomers = await _context.Customers.CountAsync(),
                TotalActiveUsers = await _context.Users.CountAsync(u => u.Status == UserStatusEnum.Active),
                TotalVehicles = await _context.Vehicles.CountAsync(),
                TotalBookingsToday = await _context.Bookings.CountAsync(b => b.ScheduledStart >= today && b.ScheduledStart < tomorrow),
                TierDistribution = tierDistribution
            };
        }
    }
}
