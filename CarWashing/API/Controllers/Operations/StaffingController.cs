using BusinessLayer.Dtos.Operations;
using BusinessLayer.IService.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Operations;

[ApiController]
[Route("api/staffing/shifts")]
[Authorize(Policy = "AdminOnly")]
public class StaffingController : OperationsControllerBase
{
    private readonly IStaffingService _staffingService;
    public StaffingController(IStaffingService staffingService) => _staffingService = staffingService;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StaffShiftResponse>>> GetShifts([FromQuery] Guid? branchId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        => Ok(await _staffingService.GetShiftsAsync(branchId, from, to));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetShift(Guid id) => FromResult(await _staffingService.GetShiftAsync(id));

    [HttpGet("available")]
    public async Task<ActionResult<IReadOnlyList<AvailableStaffResponse>>> GetAvailableStaff([FromQuery] Guid branchId, [FromQuery] DateTime startsAt, [FromQuery] DateTime endsAt)
    {
        if (branchId == Guid.Empty || startsAt >= endsAt) return BadRequest(new { detail = "Valid branchId, startsAt and endsAt are required." });
        return Ok(await _staffingService.GetAvailableStaffAsync(branchId, startsAt, endsAt));
    }

    [HttpGet("{id:guid}/capacity")]
    public async Task<ActionResult> GetShiftCapacity(Guid id) => FromResult(await _staffingService.GetShiftCapacityAsync(id));

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public ActionResult CreateShift(CreateStaffShiftRequest request) => StatusCode(StatusCodes.Status410Gone, new { detail = "Manual shifts are retired. Use branch membership, manager attendance and booking staff work." });

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public ActionResult UpdateShift(Guid id, UpdateStaffShiftRequest request) => StatusCode(StatusCodes.Status410Gone, new { detail = "Manual shifts are retired." });

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public ActionResult DeactivateShift(Guid id) => StatusCode(StatusCodes.Status410Gone, new { detail = "Manual shifts are retired." });

    [HttpPost("{id:guid}/assignments")]
    [Authorize(Policy = "AdminOnly")]
    public ActionResult AssignStaff(Guid id, AssignStaffToShiftRequest request) => StatusCode(StatusCodes.Status410Gone, new { detail = "Assign staff through booking staff work instead." });

    [HttpDelete("{id:guid}/assignments/{assignmentId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public ActionResult RemoveAssignment(Guid id, Guid assignmentId) => StatusCode(StatusCodes.Status410Gone, new { detail = "Manual shift assignments are retired." });
}
