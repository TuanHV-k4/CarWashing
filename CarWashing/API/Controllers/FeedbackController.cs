using BusinessLayer.Dtos.History;
using BusinessLayer.IService;
using BusinessLayer.IService.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/feedback")]
[Authorize(Policy = "StaffManagerOrAdmin")]
public sealed class FeedbackController(IWashHistoryService washHistoryService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] CustomerFeedbackFilter filter)
    {
        if (!currentUser.UserId.HasValue) return Unauthorized();
        var result = await washHistoryService.GetOperationalFeedbackAsync(currentUser.UserId.Value, User.IsInRole("Admin"), User.IsInRole("BranchManager"), filter);
        if (result.Succeeded) return StatusCode(result.StatusCode, result.Data);
        return Problem(title: result.StatusCode == 400 ? "Validation Error" : "Request Error", detail: result.Error, statusCode: result.StatusCode);
    }
}
