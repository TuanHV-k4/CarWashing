using BusinessLayer.Dtos.Operations;
using BusinessLayer.IService.Operations;
using BusinessLayer.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Operations
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingsController : OperationsControllerBase
    {
        private readonly IOperationsService _operationsService;
        private readonly ICurrentUserService _currentUser;
        private readonly IBranchWorkforceService _workforce;

        public BookingsController(IOperationsService operationsService, ICurrentUserService currentUser, IBranchWorkforceService workforce)
        {
            _operationsService = operationsService;
            _currentUser = currentUser;
            _workforce = workforce;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<PagedResult<BookingListItemResponse>>> GetBookings(
            [FromQuery] string? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] Guid? branchId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var allowedBranches = await StaffBranchesAsync();
            if (allowedBranches is not null && branchId.HasValue && !allowedBranches.Contains(branchId.Value)) return Forbid();
            var canReadOperations = IsAdmin() || User.IsInRole("Staff");
            return Ok(await _operationsService.GetBookingsAsync(CurrentCustomerId(), canReadOperations, status, fromDate, toDate, branchId, page, pageSize, allowedBranches));
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<ActionResult> GetBooking(Guid id)
        {
            if ((User.IsInRole("Staff") || User.IsInRole("BranchManager")) && !await CanAccessBookingAsync(id)) return Forbid();
            return FromResult(await _operationsService.GetBookingAsync(id, CurrentCustomerId(), IsAdmin() || User.IsInRole("Staff")));
        }

        [HttpPost]
        [Authorize(Policy = "CustomerOnly")]
        public async Task<ActionResult> CreateBooking(CreateBookingRequest request)
        {
            return FromResult(await _operationsService.CreateBookingAsync(request, CurrentCustomerId()));
        }

        [HttpPost("{id:guid}/cancel")]
        [Authorize]
        public async Task<ActionResult> CancelBooking(Guid id, CancelBookingRequest request)
        {
            return FromResult(await _operationsService.CancelBookingAsync(id, request, CurrentCustomerId(), IsAdmin()));
        }

        [HttpPost("{id:guid}/confirm")]
        [Authorize(Policy = "BranchManagerOrAdmin")]
        public async Task<ActionResult> ConfirmBooking(Guid id)
        {
            if (!_currentUser.UserId.HasValue) return Unauthorized();
            return FromResult(await _operationsService.ConfirmBookingAsync(id, _currentUser.UserId.Value));
        }

        [HttpGet("manager/pending")]
        [Authorize(Policy = "BranchManagerOrAdmin")]
        public async Task<ActionResult> GetPendingBookingsForManager()
        {
            if (!_currentUser.UserId.HasValue) return Unauthorized();
            return FromResult(await _operationsService.GetPendingBookingsForManagerAsync(_currentUser.UserId.Value, User.IsInRole("Admin")));
        }

        [HttpGet("manager")]
        [Authorize(Policy = "BranchManagerOrAdmin")]
        public async Task<ActionResult> GetManagerBookings([FromQuery] DateOnly? date = null)
        {
            if (!_currentUser.UserId.HasValue) return Unauthorized();
            return FromResult(await _operationsService.GetManagerBookingsAsync(_currentUser.UserId.Value, User.IsInRole("Admin"), date));
        }

        [HttpPost("{id:guid}/start")]
        [Authorize(Policy = "StaffManagerOrAdmin")]
        public async Task<ActionResult> StartBooking(Guid id)
        {
            if (!await CanAccessBookingAsync(id)) return Forbid();
            return FromResult(await _operationsService.StartBookingAsync(id, User.IsInRole("BranchManager") ? _currentUser.UserId : null));
        }

        [HttpPost("{id:guid}/complete")]
        [Authorize(Policy = "StaffManagerOrAdmin")]
        public async Task<ActionResult> CompleteBooking(Guid id)
        {
            if (!await CanAccessBookingAsync(id)) return Forbid();
            return FromResult(await _operationsService.CompleteBookingAsync(id, User.IsInRole("BranchManager") ? _currentUser.UserId : null));
        }

        [HttpGet("availability")]
        public async Task<ActionResult> GetAvailability([FromQuery] Guid branchId, [FromQuery] Guid serviceId, [FromQuery] DateTime date)
        {
            return FromResult(await _operationsService.GetAvailabilityAsync(branchId, serviceId, date));
        }

        [HttpPost("availability")]
        public async Task<ActionResult> GetAvailabilityForItems(BookingAvailabilityRequest request)
        {
            return FromResult(await _operationsService.GetAvailabilityAsync(request));
        }

        [HttpPost("{id:guid}/reschedule")]
        [Authorize]
        public async Task<ActionResult> RescheduleBooking(Guid id, RescheduleBookingRequest request)
        {
            return FromResult(await _operationsService.RescheduleBookingAsync(id, request, CurrentCustomerId(), IsAdmin()));
        }

        [HttpPost("{id:guid}/check-in")]
        [Authorize(Policy = "StaffManagerOrAdmin")]
        public async Task<ActionResult> CheckInBooking(Guid id)
        {
            if (!await CanAccessBookingAsync(id)) return Forbid();
            return FromResult(await _operationsService.CheckInBookingAsync(id, User.IsInRole("BranchManager") ? _currentUser.UserId : null));
        }

        [HttpPost("{id:guid}/no-show")]
        [Authorize(Policy = "StaffOrAdmin")]
        public async Task<ActionResult> MarkNoShow(Guid id)
        {
            if (!await CanAccessBookingAsync(id)) return Forbid();
            return FromResult(await _operationsService.MarkNoShowAsync(id));
        }

        [HttpGet("queue")]
        [Authorize(Policy = "StaffOrAdmin")]
        public async Task<ActionResult<IReadOnlyList<QueueBookingResponse>>> GetQueue([FromQuery] Guid branchId, [FromQuery] Guid? washBayId = null)
        {
            if (branchId == Guid.Empty) return BadRequest(new { detail = "branchId is required." });
            var allowedBranches = await StaffBranchesAsync();
            if (allowedBranches is not null && !allowedBranches.Contains(branchId)) return Forbid();
            return Ok(await _operationsService.GetQueueAsync(branchId, washBayId));
        }

        [HttpGet("accessible-branches")]
        [Authorize(Policy = "StaffOrAdmin")]
        public async Task<ActionResult<IReadOnlyList<BranchListItemResponse>>> GetAccessibleBranches()
        {
            var branches = await _operationsService.GetBranchesAsync(1, 100, false);
            var allowedBranches = await StaffBranchesAsync();
            if (allowedBranches is null) return Ok(branches.Items);

            return Ok(branches.Items.Where(branch => allowedBranches.Contains(branch.Id)).ToList());
        }

        [HttpPost("{id:guid}/dispatch")]
        [Authorize(Policy = "StaffManagerOrAdmin")]
        public async Task<ActionResult> DispatchBooking(Guid id, DispatchBookingRequest request)
        {
            if (!await CanAccessBookingAsync(id)) return Forbid();
            return FromResult(await _operationsService.DispatchBookingAsync(id, request, User.IsInRole("BranchManager") ? _currentUser.UserId : null));
        }

        [HttpPost("{id:guid}/assigned-staff")]
        [Authorize(Policy = "BranchManagerOrAdmin")]
        public async Task<ActionResult> AssignStaff(Guid id, AssignBookingStaffRequest request)
        {
            return FromResult(await _operationsService.AssignBookingStaffAsync(id, request, User.IsInRole("BranchManager") ? _currentUser.UserId : null));
        }

        private async Task<IReadOnlyCollection<Guid>?> StaffBranchesAsync()
        {
            if (!User.IsInRole("Staff")) return null;
            if (!_currentUser.UserId.HasValue) return Array.Empty<Guid>();
            return await _workforce.GetStaffBranchIdsAsync(_currentUser.UserId.Value);
        }

        private async Task<bool> CanAccessBookingAsync(Guid bookingId)
        {
            if (IsAdmin()) return true;
            var booking = await _operationsService.GetBookingAsync(bookingId, null, true);
            if (!booking.Succeeded || booking.Data is null) return false;
            if (User.IsInRole("BranchManager")) return _currentUser.UserId.HasValue && await _workforce.CanManageBranchAsync(_currentUser.UserId.Value, booking.Data.BranchId);
            return User.IsInRole("Staff") && _currentUser.UserId.HasValue && await _workforce.CanWorkAtBranchAsync(_currentUser.UserId.Value, booking.Data.BranchId);
        }

        [HttpGet("{id:guid}/eligible-staff")]
        [Authorize(Policy = "BranchManagerOrAdmin")]
        public async Task<ActionResult> GetEligibleStaff(Guid id)
        {
            if (!_currentUser.UserId.HasValue) return Unauthorized();
            return Ok(await _operationsService.GetEligibleBookingStaffAsync(id, _currentUser.UserId.Value));
        }

        [HttpPut("{id:guid}/staff-work")]
        [Authorize(Policy = "BranchManagerOrAdmin")]
        public async Task<ActionResult> SetStaffWork(Guid id, SetBookingStaffWorkRequest request)
        {
            if (!_currentUser.UserId.HasValue) return Unauthorized();
            return FromResult(await _operationsService.SetBookingStaffWorkAsync(id, request, _currentUser.UserId.Value));
        }

        [HttpGet("{id:guid}/reschedule-history")]
        [Authorize]
        public async Task<ActionResult> GetRescheduleHistory(Guid id)
        {
            return FromResult(await _operationsService.GetRescheduleHistoryAsync(id, CurrentCustomerId(), IsAdmin()));
        }
    }
}
