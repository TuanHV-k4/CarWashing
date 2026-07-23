using System.Security.Claims;
using BusinessLayer.Dtos.Operations;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Operations
{
    public abstract class OperationsControllerBase : ControllerBase
    {
        protected ActionResult FromResult<T>(OperationResult<T> result)
        {
            if (result.Succeeded)
            {
                return StatusCode(result.StatusCode, result.Data);
            }

            return Problem(
                title: result.StatusCode == 400 ? "Validation Error" : "Request Error",
                detail: result.Error,
                statusCode: result.StatusCode);
        }

        protected Guid? CurrentCustomerId()
        {
            var values = new[]
            {
                User.FindFirstValue("customerId"),
                User.FindFirstValue("CustomerId"),
                User.FindFirstValue("CustomerID")
            };

            foreach (var value in values)
            {
                if (Guid.TryParse(value, out var customerId))
                {
                    return customerId;
                }
            }

            return null;
        }

        protected bool IsAdmin()
        {
            var role = User.FindFirstValue(ClaimTypes.Role) ??
                       User.FindFirstValue("role");

            return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        protected bool IsCustomer()
        {
            var role = User.FindFirstValue(ClaimTypes.Role) ??
                       User.FindFirstValue("role");

            return string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase);
        }
    }
}
