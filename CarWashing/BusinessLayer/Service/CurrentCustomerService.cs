using BusinessLayer.IService;
using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service
{
    public class CurrentCustomerService : ICurrentCustomerService
    {
        private readonly ICurrentUserService _currentUser;
        private readonly ApplicationDbContext _context;

        public CurrentCustomerService(ICurrentUserService currentUser, ApplicationDbContext context)
        {
            _currentUser = currentUser;
            _context = context;
        }

        public async Task<Guid> GetCurrentCustomerIdAsync()
        {
            if (_currentUser.CustomerId.HasValue)
                return _currentUser.CustomerId.Value;

            if (!_currentUser.UserId.HasValue)
                throw new UnauthorizedAccessException("User is not authenticated.");

            var customerId = await _context.Customers
                .Where(c => c.UserID == _currentUser.UserId.Value)
                .Select(c => c.CustomerID)
                .FirstOrDefaultAsync();

            if (customerId == Guid.Empty)
                throw new KeyNotFoundException("Customer profile not found for the current user.");

            return customerId;
        }
    }
}
