using BusinessLayer.Dtos.Operations;
using BusinessLayer.IService.Operations;
using BusinessLayer.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Operations
{
    [ApiController]
    [Route("api/wash-bays")]
    public class WashBaysController : OperationsControllerBase
    {
        private readonly IOperationsService _operationsService;
        private readonly IBranchWorkforceService _branchWorkforceService;
        private readonly ICurrentUserService _currentUser;

        public WashBaysController(IOperationsService operationsService, IBranchWorkforceService branchWorkforceService, ICurrentUserService currentUser)
        {
            _operationsService = operationsService;
            _branchWorkforceService = branchWorkforceService;
            _currentUser = currentUser;
        }

        [HttpGet]
        [Authorize(Policy = "CatalogRead")]
        public async Task<ActionResult<PagedResult<WashBayListItemResponse>>> GetWashBays(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] Guid? branchId = null,
            [FromQuery] bool includeInactive = false)
        {
            IReadOnlyCollection<Guid>? allowedBranchIds = null;
            if (_currentUser.IsInRole("BranchManager"))
            {
                if (!_currentUser.UserId.HasValue) return Unauthorized();
                allowedBranchIds = await _branchWorkforceService.GetManagedBranchIdsAsync(_currentUser.UserId.Value);
                if (allowedBranchIds.Count == 0) return Forbid();
                if (branchId.HasValue && !allowedBranchIds.Contains(branchId.Value)) return Forbid();
            }

            return Ok(await _operationsService.GetWashBaysAsync(page, pageSize, branchId, includeInactive, allowedBranchIds));
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = "CatalogRead")]
        public async Task<ActionResult> GetWashBay(Guid id)
        {
            var result = await _operationsService.GetWashBayAsync(id);
            if (!result.Succeeded) return FromResult(result);
            if (_currentUser.IsInRole("BranchManager"))
            {
                if (!_currentUser.UserId.HasValue) return Unauthorized();
                var allowedBranchIds = await _branchWorkforceService.GetManagedBranchIdsAsync(_currentUser.UserId.Value);
                if (!allowedBranchIds.Contains(result.Data!.BranchId)) return Forbid();
            }
            return FromResult(result);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> CreateWashBay(CreateWashBayRequest request)
        {
            return FromResult(await _operationsService.CreateWashBayAsync(request));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> UpdateWashBay(Guid id, UpdateWashBayRequest request)
        {
            return FromResult(await _operationsService.UpdateWashBayAsync(id, request));
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> DeleteWashBay(Guid id)
        {
            return FromResult(await _operationsService.DeleteWashBayAsync(id));
        }
    }
}
