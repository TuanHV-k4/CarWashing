using BusinessLayer.Dtos.Common;
using BusinessLayer.Dtos.Customer;
using BusinessLayer.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet("me")]
        [Authorize(Policy = "CustomerOnly")]
        [ProducesResponseType(typeof(ApiResponse<CustomerProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyProfile()
        {
            var result = await _customerService.GetMyProfileAsync();
            return Ok(ApiResponse<CustomerProfileDto>.Ok(result));
        }

        [HttpPut("me")]
        [Authorize(Policy = "CustomerOnly")]
        public async Task<IActionResult> UpdateMyProfile(UpdateCustomerProfileRequest request)
        {
            return Ok(ApiResponse<CustomerProfileDto>.Ok(await _customerService.UpdateMyProfileAsync(request)));
        }
    }
}
