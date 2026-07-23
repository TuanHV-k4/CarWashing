using System.Security.Claims;
using BusinessLayer.IService;
using Microsoft.AspNetCore.Http;

namespace BusinessLayer.Service
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        public Guid? UserId => Guid.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

        public string? Username => User?.FindFirst(ClaimTypes.Name)?.Value;

        public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;

        public string? Role => User?.FindFirst(ClaimTypes.Role)?.Value;

        public string? FullName => User?.FindFirst("FullName")?.Value;

        public Guid? CustomerId => Guid.TryParse(User?.FindFirst("CustomerID")?.Value, out var id) ? id : null;

        public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
    }
}
