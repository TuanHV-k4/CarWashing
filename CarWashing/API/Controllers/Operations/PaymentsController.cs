using BusinessLayer.Dtos.Operations;
using BusinessLayer.IService.Operations;
using BusinessLayer.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Operations
{
    [ApiController]
    [Route("api/payments")]
    [Authorize(Policy = "StaffOrAdmin")]
    public class PaymentsController : OperationsControllerBase
    {
        private readonly IOperationsService _operationsService;
        private readonly IBranchWorkforceService _workforce;
        private readonly ICurrentUserService _currentUser;

        public PaymentsController(IOperationsService operationsService, IBranchWorkforceService workforce, ICurrentUserService currentUser)
        {
            _operationsService = operationsService;
            _workforce = workforce;
            _currentUser = currentUser;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult> GetPayment(Guid id)
        {
            var result = await _operationsService.GetPaymentAsync(id);
            if (result.Succeeded && result.Data is not null && !await CanAccessBranchAsync(result.Data.BranchId)) return Forbid();
            return FromResult(result);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<PaymentListItemResponse>>> GetPayments([FromQuery] Guid? branchId, [FromQuery] string? status, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var branches = await StaffBranchesAsync();
            if (branches is not null && branchId.HasValue && !branches.Contains(branchId.Value)) return Forbid();
            return Ok(await _operationsService.GetPaymentsAsync(branchId, status, from, to, page, pageSize, branches));
        }

        [HttpPost]
        public async Task<ActionResult> CreatePayment(CreatePaymentRequest request)
        {
            var booking = await _operationsService.GetBookingAsync(request.BookingId, null, true);
            if (!booking.Succeeded || booking.Data is null) return FromResult(booking);
            if (!await CanAccessBranchAsync(booking.Data.BranchId)) return Forbid();
            return FromResult(await _operationsService.CreatePaymentAsync(request));
        }

        [HttpPost("{id:guid}/paid")]
        public async Task<ActionResult> MarkPaymentPaid(Guid id, MarkPaymentPaidRequest request)
        {
            if (!await CanAccessPaymentAsync(id)) return Forbid();
            return FromResult(await _operationsService.MarkPaymentPaidAsync(id, request));
        }

        [HttpPost("{id:guid}/void")]
        public async Task<ActionResult> VoidPayment(Guid id, VoidPaymentRequest request)
        {
            if (!await CanAccessPaymentAsync(id)) return Forbid();
            return FromResult(await _operationsService.VoidPaymentAsync(id, request));
        }

        [HttpPost("{id:guid}/refunds")]
        public async Task<ActionResult> CreateRefund(Guid id, CreateRefundRequest request)
        {
            if (!await CanAccessPaymentAsync(id)) return Forbid();
            return FromResult(await _operationsService.CreateRefundAsync(id, request));
        }

        [HttpGet("refunds/{id:guid}")]
        public async Task<ActionResult> GetRefund(Guid id)
        {
            var refund = await _operationsService.GetRefundAsync(id);
            if (!refund.Succeeded || refund.Data is null) return FromResult(refund);
            if (!await CanAccessPaymentAsync(refund.Data.PaymentId)) return Forbid();
            return FromResult(refund);
        }

        private async Task<IReadOnlyCollection<Guid>?> StaffBranchesAsync()
        {
            if (!User.IsInRole("Staff")) return null;
            if (!_currentUser.UserId.HasValue) return Array.Empty<Guid>();
            return await _workforce.GetStaffBranchIdsAsync(_currentUser.UserId.Value);
        }

        private async Task<bool> CanAccessPaymentAsync(Guid paymentId)
        {
            var payment = await _operationsService.GetPaymentAsync(paymentId);
            return payment.Succeeded && payment.Data is not null && await CanAccessBranchAsync(payment.Data.BranchId);
        }

        private async Task<bool> CanAccessBranchAsync(Guid branchId)
        {
            if (IsAdmin()) return true;
            if (!_currentUser.UserId.HasValue) return false;
            if (User.IsInRole("BranchManager")) return await _workforce.CanManageBranchAsync(_currentUser.UserId.Value, branchId);
            return User.IsInRole("Staff") && await _workforce.CanWorkAtBranchAsync(_currentUser.UserId.Value, branchId);
        }

        [HttpGet("reconciliation")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> GetReconciliation([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] Guid? branchId = null)
        {
            return FromResult(await _operationsService.GetPaymentReconciliationAsync(from, to, branchId));
        }

        [HttpGet("reconciliation/export")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ExportReconciliation([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] Guid? branchId = null)
        {
            var report = await _operationsService.GetPaymentReconciliationAsync(from, to, branchId);
            if (!report.Succeeded || report.Data is null) return FromResult(report);
            var row = report.Data;
            var csv = $"From,To,BranchId,PaymentCount,PaidAmount,RefundedAmount,NetAmount{Environment.NewLine}{row.From:O},{row.To:O},{row.BranchId},{row.PaymentCount},{row.PaidAmount},{row.RefundedAmount},{row.NetAmount}{Environment.NewLine}";
            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "payment-reconciliation.csv");
        }
    }
}
