using BusinessLayer.IService;
using BusinessLayer.IService.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Operations;

[ApiController]
[Route("api/manager/workload")]
[Authorize(Policy = "BranchManagerOrAdmin")]
public class WorkloadController : OperationsControllerBase
{
    private readonly IWorkloadReportService _service;
    private readonly IBranchWorkforceService _workforce;
    private readonly ICurrentUserService _currentUser;
    public WorkloadController(IWorkloadReportService service, IBranchWorkforceService workforce, ICurrentUserService currentUser) { _service = service; _workforce = workforce; _currentUser = currentUser; }

    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] Guid? branchId)
    {
        if (!_currentUser.UserId.HasValue) return Unauthorized();
        var scope = await _workforce.ResolveManagerReportBranchAsync(_currentUser.UserId.Value, _currentUser.IsInRole("Admin"), branchId);
        if (!scope.Succeeded) return Problem(title: "Request Error", detail: scope.Error, statusCode: scope.StatusCode);
        return FromResult(await _service.GetAsync(_currentUser.UserId.Value, scope.Data!, from, to));
    }
}
