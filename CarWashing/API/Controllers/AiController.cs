using System.Threading.RateLimiting;
using BusinessLayer.Dtos.AI;
using BusinessLayer.Dtos.Common;
using BusinessLayer.IService;
using BusinessLayer.IService.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly ICurrentCustomerService _currentCustomer;

        public AiController(IAIService aiService, ICurrentCustomerService currentCustomer)
        {
            _aiService = aiService;
            _currentCustomer = currentCustomer;
        }

        /// <summary>Customer AI chat assistant.</summary>
        [HttpPost("chat")]
        [Authorize(Policy = "CustomerOnly")]
        [EnableRateLimiting("AiCustomer")]
        [ProducesResponseType(typeof(ApiResponse<AiChatResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CustomerChat([FromBody] AiChatRequestDto request)
        {
            var customerId = await _currentCustomer.GetCurrentCustomerIdAsync();
            var result = await _aiService.CustomerChatAsync(customerId, request);
            return Ok(ApiResponse<AiChatResponseDto>.Ok(result));
        }

        /// <summary>AI-powered service suggestions for the current customer.</summary>
        [HttpPost("suggest-services")]
        [Authorize(Policy = "CustomerOnly")]
        [EnableRateLimiting("AiCustomer")]
        [ProducesResponseType(typeof(ApiResponse<AiSuggestServicesResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SuggestServices([FromBody] AiSuggestServicesRequestDto request)
        {
            var customerId = await _currentCustomer.GetCurrentCustomerIdAsync();
            var result = await _aiService.SuggestServicesAsync(customerId, request);
            return Ok(ApiResponse<AiSuggestServicesResponseDto>.Ok(result));
        }

        /// <summary>Admin AI operations assistant.</summary>
        [HttpPost("admin/chat")]
        [Authorize(Policy = "AdminOnly")]
        [EnableRateLimiting("AiAdmin")]
        [ProducesResponseType(typeof(ApiResponse<AiChatResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AdminChat([FromBody] AiAdminChatRequestDto request)
        {
            var result = await _aiService.AdminChatAsync(request);
            return Ok(ApiResponse<AiChatResponseDto>.Ok(result));
        }
    }
}
