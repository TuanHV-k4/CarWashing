using BusinessLayer.IService.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace API.Controllers.Operations;
[ApiController, Route("api/admin/dashboard"), Authorize(Policy = "AdminOnly")]
public sealed class AdminDashboardController(IDashboardService dashboard) : ControllerBase
{ [HttpGet] public Task<IActionResult> Get([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Guid? branchId) => GetResult(from ?? DateTime.UtcNow.Date, to ?? DateTime.UtcNow.Date, branchId); private async Task<IActionResult> GetResult(DateTime from, DateTime to, Guid? branchId) => Ok(await dashboard.GetAdminAsync(from, to, branchId)); }
