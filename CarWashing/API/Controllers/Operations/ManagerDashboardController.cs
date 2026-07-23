using BusinessLayer.IService;
using BusinessLayer.IService.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace API.Controllers.Operations;
[ApiController, Route("api/manager/dashboard"), Authorize(Policy = "BranchManagerOrAdmin")]
public sealed class ManagerDashboardController(IDashboardService dashboard, IBranchWorkforceService workforce, ICurrentUserService current) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateTime? date, [FromQuery] Guid? branchId)
    {
        if (!current.UserId.HasValue) return Unauthorized();
        var scope = await workforce.ResolveManagerReportBranchAsync(current.UserId.Value, current.IsInRole("Admin"), branchId);
        if (!scope.Succeeded) return Problem(title: "Request Error", detail: scope.Error, statusCode: scope.StatusCode);
        var result = await dashboard.GetManagerAsync(scope.Data!, date ?? DateTime.UtcNow);
        return result is null ? NotFound() : Ok(result);
    }
}
