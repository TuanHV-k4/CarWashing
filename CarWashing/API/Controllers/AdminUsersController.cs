using BusinessLayer.Dtos.Admin;
using BusinessLayer.Dtos.Common;
using BusinessLayer.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.Enums;

namespace API.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUsersController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromServices] ICurrentUserService current,
            [FromQuery] string? query,
            [FromQuery] string? role,
            [FromQuery] UserStatusEnum? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (!current.UserId.HasValue) return Unauthorized();
            return Ok(await _adminUserService.GetUsersAsync(current.UserId.Value, query, role, status, page, pageSize));
        }

        [HttpPut("{userId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<UserSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateUserStatus(Guid userId, [FromServices] ICurrentUserService current, [FromBody] UpdateUserStatusRequestDto request)
        {
            if (!current.UserId.HasValue) return Unauthorized();
            var result = await _adminUserService.UpdateUserStatusAsync(current.UserId.Value, userId, request);
            return Ok(ApiResponse<UserSummaryDto>.Ok(result));
        }

        [HttpPut("{userId:guid}/role")]
        public async Task<IActionResult> UpdateUserRole(Guid userId, [FromServices] ICurrentUserService current, [FromBody] UpdateUserRoleRequestDto request)
        {
            if (!current.UserId.HasValue) return Unauthorized();
            var result = await _adminUserService.UpdateUserRoleAsync(current.UserId.Value, userId, request);
            return Ok(ApiResponse<UserSummaryDto>.Ok(result));
        }
    }
}
