using BusinessLayer.IService;
using BusinessLayer.IService.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Operations;

[ApiController, Route("api/manager/branch-context"), Authorize(Policy = "BranchManagerOrAdmin")]
public sealed class ManagerBranchContextController(IBranchWorkforceService workforce, ICurrentUserService current) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        if (!current.UserId.HasValue) return Unauthorized();
        var context = await workforce.GetManagerBranchContextAsync(current.UserId.Value);
        return context is null ? Forbid() : Ok(context);
    }
}
