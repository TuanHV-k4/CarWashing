using BusinessLayer.Dtos.Operations;
using BusinessLayer.IService;
using BusinessLayer.IService.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Operations;

[ApiController]
[Route("api/attendance")]
[Authorize(Policy = "StaffOrAdmin")]
public class AttendanceController : OperationsControllerBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly ICurrentUserService _currentUser;
    public AttendanceController(IAttendanceService attendanceService, ICurrentUserService currentUser) { _attendanceService = attendanceService; _currentUser = currentUser; }

    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<AttendanceResponse>>> GetMine([FromQuery] DateTime? date)
    {
        if (!_currentUser.UserId.HasValue) return Unauthorized();
        return Ok(await _attendanceService.GetMyAsync(_currentUser.UserId.Value, date ?? DateTime.UtcNow));
    }

    [HttpPost("assignments/{assignmentId:guid}/check-in")]
    public async Task<ActionResult> CheckIn(Guid assignmentId)
    {
        if (!_currentUser.UserId.HasValue) return Unauthorized();
        return FromResult(await _attendanceService.CheckInAsync(assignmentId, _currentUser.UserId.Value));
    }

    [HttpPost("assignments/{assignmentId:guid}/check-out")]
    public async Task<ActionResult> CheckOut(Guid assignmentId)
    {
        if (!_currentUser.UserId.HasValue) return Unauthorized();
        return FromResult(await _attendanceService.CheckOutAsync(assignmentId, _currentUser.UserId.Value));
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IReadOnlyList<AttendanceResponse>>> Get([FromQuery] Guid? branchId, [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] Guid? staffId, [FromQuery] string? status)
    {
        if (from >= to) return BadRequest(new { detail = "from must be before to." });
        return Ok(await _attendanceService.GetAsync(branchId, from, to, staffId, status));
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> Adjust(Guid id, AttendanceAdjustmentRequest request)
    {
        if (!_currentUser.UserId.HasValue) return Unauthorized();
        return FromResult(await _attendanceService.AdjustAsync(id, request, _currentUser.UserId.Value));
    }

    [HttpPost("{id:guid}/lock")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> Lock(Guid id, AttendanceActionRequest request)
    {
        if (!_currentUser.UserId.HasValue) return Unauthorized();
        return FromResult(await _attendanceService.LockAsync(id, request, _currentUser.UserId.Value));
    }

    [HttpPost("{id:guid}/reopen")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> Reopen(Guid id, AttendanceActionRequest request)
    {
        if (!_currentUser.UserId.HasValue) return Unauthorized();
        return FromResult(await _attendanceService.ReopenAsync(id, request, _currentUser.UserId.Value));
    }

    [HttpGet("summary")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IReadOnlyList<AttendanceSummaryResponse>>> Summary([FromQuery] Guid? branchId, [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] string groupBy = "staff")
    {
        if (from >= to || (groupBy != "staff" && groupBy != "day")) return BadRequest(new { detail = "Valid from, to and groupBy are required." });
        return Ok(await _attendanceService.GetSummaryAsync(branchId, from, to, groupBy));
    }
}
