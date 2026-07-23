using BusinessLayer.Dtos.Admin;
using BusinessLayer.Dtos.Common;
using BusinessLayer.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/admin/behavioral-logs")]
    [Authorize(Policy = "AdminOnly")]
    public class BehavioralLogsController : ControllerBase
    {
        private readonly IBehavioralLogService _behavioralLogService;

        public BehavioralLogsController(IBehavioralLogService behavioralLogService)
        {
            _behavioralLogService = behavioralLogService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<BehavioralLogItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLogs([FromQuery] BehavioralLogFilterDto filter)
        {
            var result = await _behavioralLogService.GetLogsAsync(filter);
            return Ok(ApiResponse<PagedResult<BehavioralLogItemDto>>.Ok(result));
        }

        [HttpGet("export")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportLogs([FromQuery] BehavioralLogFilterDto filter)
        {
            var bytes = await _behavioralLogService.ExportLogsAsync(filter);
            return File(bytes, "text/csv", $"behavioral-logs-{DateTime.UtcNow:yyyyMMdd}.csv");
        }
    }
}
