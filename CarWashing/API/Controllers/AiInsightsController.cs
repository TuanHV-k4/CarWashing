using System.Security.Claims;
using BusinessLayer.Dtos.AI;
using BusinessLayer.Dtos.Common;
using BusinessLayer.IService;
using BusinessLayer.IService.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/ai")]
public sealed class AiInsightsController(IAiInsightsService insights, ICurrentCustomerService currentCustomer) : ControllerBase
{
    [HttpPost("customer/assistant")]
    [Authorize(Policy = "CustomerOnly")]
    [EnableRateLimiting("AiCustomer")]
    public async Task<ActionResult<ApiResponse<AiCustomerAssistantResponseDto>>> CustomerAssistant([FromBody] AiCustomerAssistantRequestDto request, CancellationToken cancellationToken)
    {
        var customerId = await currentCustomer.GetCurrentCustomerIdAsync();
        return Ok(ApiResponse<AiCustomerAssistantResponseDto>.Ok(await insights.GetCustomerAssistantAsync(customerId, request, cancellationToken)));
    }

    [HttpGet("feedback-insights")]
    [Authorize(Policy = "BranchManagerOrAdmin")]
    [EnableRateLimiting("AiAdmin")]
    public async Task<ActionResult<ApiResponse<AiFeedbackInsightsResponseDto>>> FeedbackInsights([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] Guid? branchId, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        var result = await insights.GetFeedbackInsightsAsync(branchId, userId, User.IsInRole("Admin"), from, to, cancellationToken);
        return Ok(ApiResponse<AiFeedbackInsightsResponseDto>.Ok(result));
    }

    [HttpPost("operations-copilot")]
    [Authorize(Policy = "BranchManagerOrAdmin")]
    [EnableRateLimiting("AiAdmin")]
    public async Task<ActionResult<ApiResponse<AiOperationsCopilotResponseDto>>> OperationsCopilot([FromBody] AiOperationsCopilotRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message)) return BadRequest("Message is required.");
        var result = await insights.AskOperationsAsync(CurrentUserId(), User.IsInRole("Admin"), request, cancellationToken);
        return Ok(ApiResponse<AiOperationsCopilotResponseDto>.Ok(result));
    }

    private Guid CurrentUserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : throw new UnauthorizedAccessException("Current user is required.");
}
