using BusinessLayer.Dtos.Operations;
using BusinessLayer.IService.Operations;
using BusinessLayer.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Operations
{
    [ApiController]
    [Route("api/services")]
    public class ServicesController : OperationsControllerBase
    {
        private readonly IOperationsService _operationsService;
        private readonly ICurrentUserService _currentUser;

        public ServicesController(IOperationsService operationsService, ICurrentUserService currentUser)
        {
            _operationsService = operationsService;
            _currentUser = currentUser;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ServiceListItemResponse>>> GetServices(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool includeInactive = false)
        {
            if (includeInactive && !_currentUser.IsInRole("Admin")) return Forbid();
            return Ok(await _operationsService.GetServicesAsync(page, pageSize, includeInactive));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult> GetService(Guid id)
        {
            return FromResult(await _operationsService.GetServiceAsync(id));
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> CreateService(CreateServiceRequest request)
        {
            return FromResult(await _operationsService.CreateServiceAsync(request));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> UpdateService(Guid id, UpdateServiceRequest request)
        {
            return FromResult(await _operationsService.UpdateServiceAsync(id, request));
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> DeleteService(Guid id)
        {
            return FromResult(await _operationsService.DeleteServiceAsync(id));
        }
    }
}
