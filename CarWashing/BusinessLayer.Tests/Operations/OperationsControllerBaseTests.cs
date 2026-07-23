using System.Security.Claims;
using API.Controllers.Operations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BusinessLayer.Tests.Operations
{
    public class OperationsControllerBaseTests
    {
        [Fact]
        public void CurrentCustomerId_DoesNotTrustHeaderFallback()
        {
            var controller = new TestOperationsController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.Request.Headers["X-Customer-Id"] = Guid.NewGuid().ToString();

            Assert.Null(controller.ExposedCurrentCustomerId());
        }

        [Fact]
        public void CurrentCustomerId_ReadsJwtClaim()
        {
            var customerId = Guid.NewGuid();
            var controller = CreateControllerWithClaims(new Claim("customerId", customerId.ToString()));

            Assert.Equal(customerId, controller.ExposedCurrentCustomerId());
        }

        [Theory]
        [InlineData("Admin", true)]
        [InlineData("Staff", false)]
        public void IsAdmin_OnlyAllowsAdministrators(string role, bool expected)
        {
            var controller = CreateControllerWithClaims(new Claim(ClaimTypes.Role, role));

            Assert.Equal(expected, controller.ExposedIsAdmin());
        }

        [Fact]
        public void IsAdmin_DoesNotTrustHeaderFallback()
        {
            var controller = new TestOperationsController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.Request.Headers["X-Role"] = "Admin";

            Assert.False(controller.ExposedIsAdmin());
        }

        private static TestOperationsController CreateControllerWithClaims(params Claim[] claims)
        {
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
            };

            return new TestOperationsController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };
        }

        private sealed class TestOperationsController : OperationsControllerBase
        {
            public Guid? ExposedCurrentCustomerId() => CurrentCustomerId();

            public bool ExposedIsAdmin() => IsAdmin();
        }
    }
}
