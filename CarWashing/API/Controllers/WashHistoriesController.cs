using BusinessLayer.Dtos.Common;
using BusinessLayer.Dtos.History;
using BusinessLayer.IService;
using BusinessLayer.IService.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/wash-histories")]
    public class WashHistoriesController : ControllerBase
    {
        private readonly IWashHistoryService _washHistoryService;
        private readonly ICurrentUserService _currentUser;

        public WashHistoriesController(IWashHistoryService washHistoryService, ICurrentUserService currentUser)
        {
            _washHistoryService = washHistoryService;
            _currentUser = currentUser;
        }

        [HttpGet("me")]
        [Authorize(Policy = "CustomerOnly")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WashHistoryListItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _washHistoryService.GetMyHistoryAsync(page, pageSize);
            return Ok(ApiResponse<PagedResult<WashHistoryListItemDto>>.Ok(result));
        }

        [HttpGet("me/{washHistoryId:guid}")]
        [Authorize(Policy = "CustomerOnly")]
        [ProducesResponseType(typeof(ApiResponse<WashHistoryDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyHistoryDetail(Guid washHistoryId)
        {
            var result = await _washHistoryService.GetMyHistoryDetailAsync(washHistoryId);
            return Ok(ApiResponse<WashHistoryDetailDto>.Ok(result));
        }

        [HttpPost("me/{washHistoryId:guid}/feedback")]
        [Authorize(Policy = "CustomerOnly")]
        public async Task<IActionResult> SubmitMyFeedback(Guid washHistoryId, SubmitWashFeedbackRequest request)
        {
            return Ok(ApiResponse<WashHistoryDetailDto>.Ok(await _washHistoryService.SubmitMyFeedbackAsync(washHistoryId, request)));
        }

        [HttpGet("operations")]
        [Authorize(Policy = "BranchManagerOrAdmin")]
        [ProducesResponseType(typeof(PagedResult<OperationalWashHistoryItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetOperationalHistory([FromQuery] OperationalWashHistoryFilter filter)
        {
            if (!_currentUser.UserId.HasValue) return Unauthorized();
            var result = await _washHistoryService.GetOperationalHistoryAsync(_currentUser.UserId.Value, User.IsInRole("Admin"), filter);
            if (result.Succeeded) return StatusCode(result.StatusCode, result.Data);
            return Problem(title: result.StatusCode == 400 ? "Validation Error" : "Request Error", detail: result.Error, statusCode: result.StatusCode);
        }

        [HttpGet("customer/{customerId:guid}")]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WashHistoryListItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetHistoryByCustomerId(
            Guid customerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _washHistoryService.GetHistoryByCustomerIdAsync(customerId, page, pageSize);
            return Ok(ApiResponse<PagedResult<WashHistoryListItemDto>>.Ok(result));
        }
    }
}
