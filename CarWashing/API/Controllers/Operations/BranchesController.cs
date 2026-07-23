using BusinessLayer.Dtos.Operations;
using BusinessLayer.IService.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Operations
{
    [ApiController]
    [Route("api/branches")]
    public class BranchesController : OperationsControllerBase
    {
        private readonly IOperationsService _operationsService;

        public BranchesController(IOperationsService operationsService)
        {
            _operationsService = operationsService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<BranchListItemResponse>>> GetBranches(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool includeInactive = false)
        {
            return Ok(await _operationsService.GetBranchesAsync(page, pageSize, includeInactive));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult> GetBranch(Guid id)
        {
            return FromResult(await _operationsService.GetBranchAsync(id));
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> CreateBranch(CreateBranchRequest request)
        {
            return FromResult(await _operationsService.CreateBranchAsync(request));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> UpdateBranch(Guid id, UpdateBranchRequest request)
        {
            return FromResult(await _operationsService.UpdateBranchAsync(id, request));
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> DeleteBranch(Guid id)
        {
            return FromResult(await _operationsService.DeleteBranchAsync(id));
        }
    }
}
