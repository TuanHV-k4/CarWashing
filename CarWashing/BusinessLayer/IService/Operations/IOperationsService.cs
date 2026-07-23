using BusinessLayer.Dtos.Operations;

namespace BusinessLayer.IService.Operations
{
    public interface IOperationsService
    {
        Task<PagedResult<ServiceListItemResponse>> GetServicesAsync(int page, int pageSize, bool includeInactive);
        Task<OperationResult<ServiceResponse>> GetServiceAsync(Guid id);
        Task<OperationResult<ServiceResponse>> CreateServiceAsync(CreateServiceRequest request);
        Task<OperationResult<ServiceResponse>> UpdateServiceAsync(Guid id, UpdateServiceRequest request);
        Task<OperationResult<bool>> DeleteServiceAsync(Guid id);

        Task<PagedResult<BranchListItemResponse>> GetBranchesAsync(int page, int pageSize, bool includeInactive);
        Task<OperationResult<BranchResponse>> GetBranchAsync(Guid id);
        Task<OperationResult<BranchResponse>> CreateBranchAsync(CreateBranchRequest request);
        Task<OperationResult<BranchResponse>> UpdateBranchAsync(Guid id, UpdateBranchRequest request);
        Task<OperationResult<bool>> DeleteBranchAsync(Guid id);

        Task<PagedResult<WashBayListItemResponse>> GetWashBaysAsync(int page, int pageSize, Guid? branchId, bool includeInactive, IReadOnlyCollection<Guid>? allowedBranchIds = null);
        Task<OperationResult<WashBayResponse>> GetWashBayAsync(Guid id);
        Task<OperationResult<WashBayResponse>> CreateWashBayAsync(CreateWashBayRequest request);
        Task<OperationResult<WashBayResponse>> UpdateWashBayAsync(Guid id, UpdateWashBayRequest request);
        Task<OperationResult<bool>> DeleteWashBayAsync(Guid id);

        Task<PagedResult<BookingListItemResponse>> GetBookingsAsync(
            Guid? currentCustomerId,
            bool isAdmin,
            string? status,
            DateTime? fromDate,
            DateTime? toDate,
            Guid? branchId,
            int page,
            int pageSize, IReadOnlyCollection<Guid>? allowedBranchIds = null);
        Task<OperationResult<IReadOnlyList<BookingListItemResponse>>> GetPendingBookingsForManagerAsync(Guid managerUserId, bool isAdmin);
        Task<OperationResult<IReadOnlyList<BookingListItemResponse>>> GetManagerBookingsAsync(Guid managerUserId, bool isAdmin, DateOnly? date = null);
        Task<OperationResult<BookingDetailResponse>> GetBookingAsync(Guid id, Guid? currentCustomerId, bool isAdmin);
        Task<OperationResult<BookingResponse>> CreateBookingAsync(CreateBookingRequest request, Guid? currentCustomerId);
        Task<OperationResult<BookingAvailabilityResponse>> GetAvailabilityAsync(Guid branchId, Guid serviceId, DateTime date);
        Task<OperationResult<BookingAvailabilityResponse>> GetAvailabilityAsync(BookingAvailabilityRequest request);
        Task<OperationResult<BookingResponse>> CancelBookingAsync(Guid id, CancelBookingRequest request, Guid? currentCustomerId, bool isAdmin);
        Task<OperationResult<BookingResponse>> RescheduleBookingAsync(Guid id, RescheduleBookingRequest request, Guid? currentCustomerId, bool isAdmin);
        Task<OperationResult<BookingResponse>> ConfirmBookingAsync(Guid id, Guid managerUserId);
        Task<OperationResult<BookingResponse>> StartBookingAsync(Guid id, Guid? managerUserId = null);
        Task<OperationResult<BookingResponse>> CompleteBookingAsync(Guid id, Guid? managerUserId = null);
        Task<OperationResult<BookingResponse>> CheckInBookingAsync(Guid id, Guid? managerUserId = null);
        Task<OperationResult<BookingResponse>> MarkNoShowAsync(Guid id);
        Task<IReadOnlyList<QueueBookingResponse>> GetQueueAsync(Guid branchId, Guid? washBayId);
        Task<OperationResult<BookingResponse>> DispatchBookingAsync(Guid id, DispatchBookingRequest request, Guid? managerUserId = null);
        Task<OperationResult<BookingResponse>> AssignBookingStaffAsync(Guid id, AssignBookingStaffRequest request, Guid? managerUserId = null);
        Task<IReadOnlyList<EligibleBookingStaffResponse>> GetEligibleBookingStaffAsync(Guid id, Guid actorId);
        Task<OperationResult<BookingResponse>> SetBookingStaffWorkAsync(Guid id, SetBookingStaffWorkRequest request, Guid actorId);
        Task<OperationResult<IReadOnlyList<BookingRescheduleHistoryResponse>>> GetRescheduleHistoryAsync(Guid id, Guid? currentCustomerId, bool isAdmin);

        Task<OperationResult<PaymentResponse>> GetPaymentAsync(Guid id);
        Task<PagedResult<PaymentListItemResponse>> GetPaymentsAsync(Guid? branchId, string? status, DateTime? from, DateTime? to, int page, int pageSize, IReadOnlyCollection<Guid>? allowedBranchIds = null);
        Task<OperationResult<PaymentResponse>> CreatePaymentAsync(CreatePaymentRequest request);
        Task<OperationResult<PaymentResponse>> MarkPaymentPaidAsync(Guid id, MarkPaymentPaidRequest request);
        Task<OperationResult<PaymentResponse>> VoidPaymentAsync(Guid id, VoidPaymentRequest request);
        Task<OperationResult<RefundResponse>> CreateRefundAsync(Guid paymentId, CreateRefundRequest request);
        Task<OperationResult<RefundResponse>> GetRefundAsync(Guid refundId);
        Task<OperationResult<PaymentReconciliationResponse>> GetPaymentReconciliationAsync(DateTime from, DateTime to, Guid? branchId);
    }
}
