using BusinessLayer.IService;
using BusinessLayer.IService.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Operations;

[ApiController]
[Route("api/staff/workload")]
[Authorize(Policy = "StaffOrAdmin")]
public sealed class StaffWorkloadController(IWorkloadReportService workload, ICurrentUserService currentUser) : OperationsControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        if (!currentUser.UserId.HasValue) return Unauthorized();
        return FromResult(await workload.GetMineAsync(currentUser.UserId.Value, from, to));
    }
}
