using BusinessLayer.Dtos.Customer;
using BusinessLayer.IService;
using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentCustomerService _currentCustomer;

        public CustomerService(ApplicationDbContext context, ICurrentCustomerService currentCustomer)
        {
            _context = context;
            _currentCustomer = currentCustomer;
        }

        public async Task<CustomerProfileDto> GetMyProfileAsync()
        {
            var customerId = await _currentCustomer.GetCurrentCustomerIdAsync();
            return await GetProfileByCustomerIdAsync(customerId);
        }

        public async Task<CustomerProfileDto> GetProfileByCustomerIdAsync(Guid customerId)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .Include(c => c.Tier)
                .FirstOrDefaultAsync(c => c.CustomerID == customerId)
                ?? throw new KeyNotFoundException("Customer not found.");

            var perks = customer.TierID.HasValue
                ? await _context.TierBenefits
                    .Where(b => b.TierID == customer.TierID && b.IsActive)
                    .Select(b => b.BenefitName)
                    .ToListAsync()
                : [];

            return MapToDto(customer, perks);
        }

        public async Task<CustomerProfileDto> UpdateMyProfileAsync(UpdateCustomerProfileRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Full name and email are required.");
            var customerId = await _currentCustomer.GetCurrentCustomerIdAsync();
            var customer = await _context.Customers.Include(item => item.User).FirstOrDefaultAsync(item => item.CustomerID == customerId)
                ?? throw new KeyNotFoundException("Customer not found.");
            var email = request.Email.Trim();
            if (await _context.Users.AnyAsync(item => item.Email == email && item.UserID != customer.UserID))
                throw new InvalidOperationException("Email is already in use.");
            customer.User.FullName = request.FullName.Trim();
            customer.User.Email = email;
            customer.User.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
            await _context.SaveChangesAsync();
            return await GetProfileByCustomerIdAsync(customerId);
        }

        private static CustomerProfileDto MapToDto(DataAccessLayer.Entity.Customer customer, List<string> perks) => new()
        {
            CustomerID = customer.CustomerID,
            UserID = customer.UserID,
            Username = customer.User.Username,
            FullName = customer.User.FullName,
            Email = customer.User.Email,
            PhoneNumber = customer.User.PhoneNumber,
            CurrentPoints = customer.CurrentPoints,
            LifetimePoints = customer.LifetimePoints,
            TotalSpent = customer.TotalSpent,
            TotalVisits = customer.TotalVisits,
            LastVisitDate = customer.LastVisitDate,
            TierID = customer.TierID,
            TierName = customer.Tier?.TierName,
            TierRank = customer.Tier?.TierRank,
            CurrentTierSince = customer.CurrentTierSince,
            CreatedAt = customer.CreatedAt,
            TierPerks = perks
        };
    }
}
