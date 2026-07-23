using BusinessLayer.Dtos.Operations;
using BusinessLayer.IService;
using BusinessLayer.IService.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Operations;

[ApiController]
[Route("api/manager/attendance")]
[Authorize(Policy = "BranchManagerOrAdmin")]
public class ManagerAttendanceController : OperationsControllerBase
{
    private readonly IBranchWorkforceService _service;
    private readonly ICurrentUserService _current;
    public ManagerAttendanceController(IBranchWorkforceService service, ICurrentUserService current) => (_service, _current) = (service, current);

    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] DateTime workDate, [FromQuery] Guid? branchId)
    {
        if (!_current.UserId.HasValue) return Unauthorized();
        var scope = await _service.ResolveManagerReportBranchAsync(_current.UserId.Value, _current.IsInRole("Admin"), branchId);
        if (!scope.Succeeded) return Problem(title: "Request Error", detail: scope.Error, statusCode: scope.StatusCode);
        return Ok(await _service.GetManagerAttendanceAsync(_current.UserId.Value, scope.Data!, workDate));
    }
    [HttpPost("{membershipId:guid}/check-in")]
    public async Task<ActionResult> CheckIn(Guid membershipId) { if (!_current.UserId.HasValue) return Unauthorized(); return FromResult(await _service.CheckInAsync(_current.UserId.Value, membershipId)); }
    [HttpPost("{membershipId:guid}/check-out")]
    public async Task<ActionResult> CheckOut(Guid membershipId) { if (!_current.UserId.HasValue) return Unauthorized(); return FromResult(await _service.CheckOutAsync(_current.UserId.Value, membershipId)); }
    [HttpPost("{membershipId:guid}/mark-absent")]
    public async Task<ActionResult> MarkAbsent(Guid membershipId, AttendanceExceptionRequest request) { if (!_current.UserId.HasValue) return Unauthorized(); return FromResult(await _service.MarkAbsentAsync(_current.UserId.Value, membershipId, request)); }
    [HttpPost("{membershipId:guid}/reinstate-check-in")]
    public async Task<ActionResult> Reinstate(Guid membershipId, AttendanceExceptionRequest request) { if (!_current.UserId.HasValue) return Unauthorized(); return FromResult(await _service.ReinstateAndCheckInAsync(_current.UserId.Value, membershipId, request)); }
}
