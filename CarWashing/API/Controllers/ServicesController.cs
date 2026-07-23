using BusinessLayer.Dtos;
using BusinessLayer.IService;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [NonController]
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceBusinessService _serviceBusiness;

        public ServicesController(IServiceBusinessService serviceBusiness)
        {
            _serviceBusiness = serviceBusiness;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceResponseDto>>> GetAll()
        {
            var services = await _serviceBusiness.GetAllAsync();
            return Ok(services);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ServiceResponseDto>> GetById(Guid id)
        {
            var service = await _serviceBusiness.GetByIdAsync(id);
            if (service is null)
                return NotFound();

            return Ok(service);
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponseDto>> Create([FromBody] CreateServiceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _serviceBusiness.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.ServiceID }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ServiceResponseDto>> Update(Guid id, [FromBody] UpdateServiceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _serviceBusiness.UpdateAsync(id, dto);
            if (updated is null)
                return NotFound();

            return Ok(updated);
        }

        [HttpPatch("{id:guid}")]
        public async Task<ActionResult<ServiceResponseDto>> Patch(Guid id, [FromBody] PatchServiceDto dto)
        {
            var updated = await _serviceBusiness.PatchAsync(id, dto);
            if (updated is null)
                return NotFound();

            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _serviceBusiness.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
