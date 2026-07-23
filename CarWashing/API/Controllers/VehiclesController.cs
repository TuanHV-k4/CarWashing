using BusinessLayer.Dtos.Common;
using BusinessLayer.Dtos.Vehicle;
using BusinessLayer.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/vehicles")]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet("me")]
        [Authorize(Policy = "CustomerOnly")]
        [ProducesResponseType(typeof(ApiResponse<List<VehicleResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyVehicles()
        {
            var result = await _vehicleService.GetMyVehiclesAsync();
            return Ok(ApiResponse<List<VehicleResponseDto>>.Ok(result));
        }

        [HttpPost]
        [Authorize(Policy = "CustomerOnly")]
        [ProducesResponseType(typeof(ApiResponse<VehicleResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleRequestDto request)
        {
            var result = await _vehicleService.CreateVehicleAsync(request);
            return Ok(ApiResponse<VehicleResponseDto>.Ok(result));
        }

        [HttpPut("{vehicleId:guid}")]
        [Authorize(Policy = "CustomerOnly")]
        [ProducesResponseType(typeof(ApiResponse<VehicleResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateVehicle(Guid vehicleId, [FromBody] UpdateVehicleRequestDto request)
        {
            var result = await _vehicleService.UpdateVehicleAsync(vehicleId, request);
            return Ok(ApiResponse<VehicleResponseDto>.Ok(result));
        }

        [HttpPut("{vehicleId:guid}/status")]
        [Authorize(Policy = "CustomerOnly")]
        [ProducesResponseType(typeof(ApiResponse<VehicleResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateVehicleStatus(Guid vehicleId, [FromBody] UpdateVehicleStatusRequestDto request)
        {
            var result = await _vehicleService.UpdateVehicleStatusAsync(vehicleId, request);
            return Ok(ApiResponse<VehicleResponseDto>.Ok(result));
        }
    }
}