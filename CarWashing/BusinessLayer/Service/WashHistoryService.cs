using BusinessLayer.Dtos.Common;
using BusinessLayer.Dtos.History;
using BusinessLayer.IService;
using BusinessLayer.IService.Operations;
using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service
{
    public class WashHistoryService : IWashHistoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentCustomerService _currentCustomer;
        private readonly IBranchWorkforceService _branchWorkforce;

        public WashHistoryService(ApplicationDbContext context, ICurrentCustomerService currentCustomer, IBranchWorkforceService branchWorkforce)
        {
            _context = context;
            _currentCustomer = currentCustomer;
            _branchWorkforce = branchWorkforce;
        }

        public async Task<PagedResult<WashHistoryListItemDto>> GetMyHistoryAsync(int page, int pageSize)
        {
            var customerId = await _currentCustomer.GetCurrentCustomerIdAsync();
            return await GetHistoryByCustomerIdAsync(customerId, page, pageSize);
        }

        public async Task<PagedResult<WashHistoryListItemDto>> GetHistoryByCustomerIdAsync(Guid customerId, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = _context.WashHistories
                .Include(w => w.Booking)
                    .ThenInclude(b => b.Vehicle)
                .Include(w => w.Booking)
                    .ThenInclude(b => b.Branch)
                .Include(w => w.Booking)
                    .ThenInclude(b => b.BookingDetails)
                        .ThenInclude(d => d.Service)
                .Where(w => w.Booking.CustomerID == customerId)
                .OrderByDescending(w => w.WashDate);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new WashHistoryListItemDto
                {
                    WashHistoryID = w.WashHistoryID,
                    BookingID = w.BookingID,
                    WashDate = w.WashDate,
                    FinalAmount = w.FinalAmount,
                    PointsEarned = w.PointsEarned,
                    CustomerRating = w.CustomerRating,
                    VehiclePlate = w.Booking.Vehicle.LicensePlate,
                    BranchName = w.Booking.Branch.BranchName,
                    Services = w.Booking.BookingDetails.Select(d => d.Service.ServiceName).ToList()
                })
                .ToListAsync();

            return new PagedResult<WashHistoryListItemDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public async Task<WashHistoryDetailDto> GetMyHistoryDetailAsync(Guid washHistoryId)
        {
            var customerId = await _currentCustomer.GetCurrentCustomerIdAsync();

            var history = await _context.WashHistories
                .Include(w => w.Booking)
                    .ThenInclude(b => b.Vehicle)
                .Include(w => w.Booking)
                    .ThenInclude(b => b.Branch)
                .Include(w => w.Booking)
                    .ThenInclude(b => b.BookingDetails)
                        .ThenInclude(d => d.Service)
                .FirstOrDefaultAsync(w => w.WashHistoryID == washHistoryId && w.Booking.CustomerID == customerId)
                ?? throw new KeyNotFoundException("Wash history not found.");

            return new WashHistoryDetailDto
            {
                WashHistoryID = history.WashHistoryID,
                BookingID = history.BookingID,
                WashDate = history.WashDate,
                ActualTotalAmount = history.ActualTotalAmount,
                DiscountAmount = history.DiscountAmount,
                FinalAmount = history.FinalAmount,
                PointsEarned = history.PointsEarned,
                RewardUsed = history.RewardUsed,
                CustomerRating = history.CustomerRating,
                Feedback = history.Feedback,
                VehiclePlate = history.Booking.Vehicle.LicensePlate,
                BranchName = history.Booking.Branch.BranchName,
                Services = history.Booking.BookingDetails.Select(d => new WashHistoryServiceDto
                {
                    ServiceID = d.ServiceID,
                    ServiceName = d.Service.ServiceName,
                    Price = d.Service.Price
                }).ToList()
            };
        }

        public async Task<WashHistoryDetailDto> SubmitMyFeedbackAsync(Guid washHistoryId, SubmitWashFeedbackRequest request)
        {
            if (request.Rating is < 1 or > 5) throw new ArgumentException("Rating must be between 1 and 5.");
            var customerId = await _currentCustomer.GetCurrentCustomerIdAsync();
            var history = await _context.WashHistories.Include(item => item.Booking).FirstOrDefaultAsync(item => item.WashHistoryID == washHistoryId && item.Booking.CustomerID == customerId)
                ?? throw new KeyNotFoundException("Wash history not found.");
            if (history.CustomerRating.HasValue) throw new InvalidOperationException("Feedback has already been submitted.");
            history.CustomerRating = request.Rating;
            history.Feedback = string.IsNullOrWhiteSpace(request.Feedback) ? null : request.Feedback.Trim();
            await _context.SaveChangesAsync();
            return await GetMyHistoryDetailAsync(washHistoryId);
        }

        public async Task<BusinessLayer.Dtos.Operations.OperationResult<PagedResult<CustomerFeedbackItemDto>>> GetOperationalFeedbackAsync(Guid actorId, bool isAdmin, bool isManager, CustomerFeedbackFilter filter)
        {
            if (filter.Rating is < 1 or > 5) return BusinessLayer.Dtos.Operations.OperationResult<PagedResult<CustomerFeedbackItemDto>>.Failure("Rating must be between 1 and 5.", 400);
            if (filter.From.HasValue && filter.To.HasValue && filter.From > filter.To) return BusinessLayer.Dtos.Operations.OperationResult<PagedResult<CustomerFeedbackItemDto>>.Failure("From date must not be later than to date.", 400);

            IReadOnlyList<Guid> allowedBranches;
            if (isAdmin)
            {
                if (filter.BranchId.HasValue && !await _context.Branches.AnyAsync(branch => branch.BranchID == filter.BranchId.Value))
                    return BusinessLayer.Dtos.Operations.OperationResult<PagedResult<CustomerFeedbackItemDto>>.Failure("Branch not found.", 404);
                allowedBranches = filter.BranchId.HasValue ? [filter.BranchId.Value] : [];
            }
            else
            {
                allowedBranches = isManager
                    ? await _branchWorkforce.GetManagedBranchIdsAsync(actorId)
                    : await _branchWorkforce.GetStaffBranchIdsAsync(actorId);
                if (allowedBranches.Count == 0) return BusinessLayer.Dtos.Operations.OperationResult<PagedResult<CustomerFeedbackItemDto>>.Failure("You are not assigned to an active branch.", 403);
                if (filter.BranchId.HasValue && !allowedBranches.Contains(filter.BranchId.Value))
                    return BusinessLayer.Dtos.Operations.OperationResult<PagedResult<CustomerFeedbackItemDto>>.Failure("You cannot view feedback for this branch.", 403);
                if (filter.BranchId.HasValue) allowedBranches = [filter.BranchId.Value];
            }

            IQueryable<DataAccessLayer.Entity.WashHistory> query = _context.WashHistories.AsNoTracking()
                .Include(history => history.Booking).ThenInclude(booking => booking.Branch)
                .Include(history => history.Booking).ThenInclude(booking => booking.AssignedStaff)
                .Include(history => history.Booking).ThenInclude(booking => booking.BookingDetails).ThenInclude(detail => detail.Service)
                .Include(history => history.Booking).ThenInclude(booking => booking.StaffWorks).ThenInclude(work => work.StaffUser)
                .Where(history => history.CustomerRating.HasValue);

            if (!isAdmin || filter.BranchId.HasValue) query = query.Where(history => allowedBranches.Contains(history.Booking.BranchID));
            if (filter.From.HasValue) query = query.Where(history => history.WashDate >= filter.From.Value);
            if (filter.To.HasValue)
            {
                var inclusiveTo = filter.To.Value.TimeOfDay == TimeSpan.Zero
                    ? filter.To.Value.Date.AddDays(1).AddTicks(-1)
                    : filter.To.Value;
                query = query.Where(history => history.WashDate <= inclusiveTo);
            }
            if (filter.Rating.HasValue) query = query.Where(history => history.CustomerRating == filter.Rating.Value);

            var page = Math.Max(1, filter.Page);
            var pageSize = Math.Clamp(filter.PageSize, 1, 50);
            var total = await query.CountAsync();
            var histories = await query.OrderByDescending(history => history.WashDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var items = histories.Select(history =>
            {
                var staff = history.Booking.StaffWorks
                    .OrderBy(work => work.StaffUser.FullName)
                    .Select(work => new CustomerFeedbackStaffDto { StaffUserId = work.StaffUserID, StaffName = work.StaffUser.FullName, WorkRole = work.WorkRole })
                    .ToList();
                if (staff.Count == 0 && history.Booking.AssignedStaff is not null)
                    staff.Add(new CustomerFeedbackStaffDto { StaffUserId = history.Booking.AssignedStaffID!.Value, StaffName = history.Booking.AssignedStaff.FullName });
                return new CustomerFeedbackItemDto
                {
                    WashHistoryId = history.WashHistoryID,
                    WashDate = history.WashDate,
                    Rating = history.CustomerRating!.Value,
                    Feedback = history.Feedback,
                    BranchName = history.Booking.Branch.BranchName,
                    Services = history.Booking.BookingDetails.Select(detail => detail.Service.ServiceName).ToList(),
                    StaffMembers = staff
                };
            }).ToList();

            return BusinessLayer.Dtos.Operations.OperationResult<PagedResult<CustomerFeedbackItemDto>>.Success(new PagedResult<CustomerFeedbackItemDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total });
        }

        public async Task<BusinessLayer.Dtos.Operations.OperationResult<PagedResult<OperationalWashHistoryItemDto>>> GetOperationalHistoryAsync(Guid actorId, bool isAdmin, OperationalWashHistoryFilter filter)
        {
            if (filter.From.HasValue && filter.To.HasValue && filter.From > filter.To)
                return BusinessLayer.Dtos.Operations.OperationResult<PagedResult<OperationalWashHistoryItemDto>>.Failure("From date must not be later than to date.", 400);

            IReadOnlyList<Guid> allowedBranches;
            if (isAdmin)
            {
                if (filter.BranchId.HasValue && !await _context.Branches.AnyAsync(branch => branch.BranchID == filter.BranchId.Value))
                    return BusinessLayer.Dtos.Operations.OperationResult<PagedResult<OperationalWashHistoryItemDto>>.Failure("Branch not found.", 404);
                allowedBranches = filter.BranchId.HasValue ? [filter.BranchId.Value] : [];
            }
            else
            {
                allowedBranches = await _branchWorkforce.GetManagedBranchIdsAsync(actorId);
                if (allowedBranches.Count == 0)
                    return BusinessLayer.Dtos.Operations.OperationResult<PagedResult<OperationalWashHistoryItemDto>>.Failure("You are not assigned to an active managed branch.", 403);
                if (filter.BranchId.HasValue && !allowedBranches.Contains(filter.BranchId.Value))
                    return BusinessLayer.Dtos.Operations.OperationResult<PagedResult<OperationalWashHistoryItemDto>>.Failure("You cannot view wash history for this branch.", 403);
                if (filter.BranchId.HasValue) allowedBranches = [filter.BranchId.Value];
            }

            IQueryable<DataAccessLayer.Entity.WashHistory> query = _context.WashHistories.AsNoTracking()
                .Include(history => history.Booking).ThenInclude(booking => booking.Customer).ThenInclude(customer => customer.User)
                .Include(history => history.Booking).ThenInclude(booking => booking.Vehicle)
                .Include(history => history.Booking).ThenInclude(booking => booking.Branch)
                .Include(history => history.Booking).ThenInclude(booking => booking.AssignedStaff)
                .Include(history => history.Booking).ThenInclude(booking => booking.BookingDetails).ThenInclude(detail => detail.Service)
                .Include(history => history.Booking).ThenInclude(booking => booking.StaffWorks).ThenInclude(work => work.StaffUser);

            if (!isAdmin || filter.BranchId.HasValue)
                query = query.Where(history => allowedBranches.Contains(history.Booking.BranchID));
            if (filter.From.HasValue) query = query.Where(history => history.WashDate >= filter.From.Value);
            if (filter.To.HasValue) query = query.Where(history => history.WashDate <= filter.To.Value);
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();
                query = query.Where(history => history.Booking.Customer.User.FullName.Contains(search) || history.Booking.Vehicle.LicensePlate.Contains(search));
            }

            var page = Math.Max(1, filter.Page);
            var pageSize = Math.Clamp(filter.PageSize, 1, 50);
            var total = await query.CountAsync();
            var histories = await query.OrderByDescending(history => history.WashDate)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var items = histories.Select(history =>
            {
                var staff = history.Booking.StaffWorks
                    .OrderBy(work => work.StaffUser.FullName)
                    .Select(work => new OperationalWashHistoryStaffDto
                    {
                        StaffUserId = work.StaffUserID,
                        StaffName = work.StaffUser.FullName,
                        WorkRole = work.WorkRole,
                        ContributionPercent = work.ContributionPercent
                    }).ToList();
                if (staff.Count == 0 && history.Booking.AssignedStaff is not null)
                    staff.Add(new OperationalWashHistoryStaffDto { StaffUserId = history.Booking.AssignedStaffID!.Value, StaffName = history.Booking.AssignedStaff.FullName });

                return new OperationalWashHistoryItemDto
                {
                    WashHistoryId = history.WashHistoryID,
                    WashDate = history.WashDate,
                    CustomerName = history.Booking.Customer.User.FullName,
                    VehiclePlate = history.Booking.Vehicle.LicensePlate,
                    BranchName = history.Booking.Branch.BranchName,
                    ActualTotalAmount = history.ActualTotalAmount,
                    DiscountAmount = history.DiscountAmount,
                    FinalAmount = history.FinalAmount,
                    CustomerRating = history.CustomerRating,
                    Feedback = history.Feedback,
                    Services = history.Booking.BookingDetails.Select(detail => detail.Service.ServiceName).ToList(),
                    StaffMembers = staff
                };
            }).ToList();

            return BusinessLayer.Dtos.Operations.OperationResult<PagedResult<OperationalWashHistoryItemDto>>.Success(new PagedResult<OperationalWashHistoryItemDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            });
        }
    }
}
