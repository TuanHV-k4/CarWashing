using BusinessLayer.Dtos.Operations;
using BusinessLayer.Helpers;
using BusinessLayer.IService;
using BusinessLayer.IService.Operations;
using DataAccessLayer.Context;
using DataAccessLayer.Entity;
using DataAccessLayer.Enums;
using Microsoft.EntityFrameworkCore;
using ServiceEntity = DataAccessLayer.Entity.Service;

namespace BusinessLayer.Service.Operations
{
    public class OperationsService : IOperationsService, IBookingReadService
    {
        private static readonly BookingStatusEnum[] ActiveBookingStatuses =
        {
            BookingStatusEnum.Pending,
            BookingStatusEnum.Confirmed,
            BookingStatusEnum.CheckedIn,
            BookingStatusEnum.InProgress
        };

        private readonly ApplicationDbContext _context;
        private readonly IWashCompletionService _washCompletionService;
        private readonly IBehavioralLogWriter _behavioralLogWriter;
        private readonly IBranchWorkforceService _branchWorkforceService;

        public OperationsService(
            ApplicationDbContext context,
            IWashCompletionService washCompletionService,
            IBehavioralLogWriter behavioralLogWriter,
            IBranchWorkforceService branchWorkforceService)
        {
            _context = context;
            _washCompletionService = washCompletionService;
            _behavioralLogWriter = behavioralLogWriter;
            _branchWorkforceService = branchWorkforceService;
        }

        public async Task<PagedResult<ServiceListItemResponse>> GetServicesAsync(int page, int pageSize, bool includeInactive)
        {
            page = NormalizePage(page);
            pageSize = NormalizePageSize(pageSize);

            var query = _context.Services.AsNoTracking();
            if (!includeInactive)
            {
                query = query.Where(service => service.Status == ServiceStatusEnum.Active);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(service => service.ServiceName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(service => new ServiceListItemResponse
                {
                    Id = service.ServiceID,
                    Name = service.ServiceName,
                    Description = service.Description,
                    Price = service.Price,
                    DurationMinutes = ToMinutes(service.EstimatedDuration),
                    IsActive = service.Status == ServiceStatusEnum.Active
                })
                .ToListAsync();

            return new PagedResult<ServiceListItemResponse> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
        }

        public async Task<OperationResult<ServiceResponse>> GetServiceAsync(Guid id)
        {
            var service = await _context.Services.AsNoTracking().FirstOrDefaultAsync(item => item.ServiceID == id);
            return service is null
                ? OperationResult<ServiceResponse>.Failure("Service not found.", 404)
                : OperationResult<ServiceResponse>.Success(MapService(service));
        }

        public async Task<OperationResult<ServiceResponse>> CreateServiceAsync(CreateServiceRequest request)
        {
            var validation = ValidateServiceRequest(request.Name, request.Description, request.Price, request.DurationMinutes);
            if (validation is not null)
            {
                return OperationResult<ServiceResponse>.Failure(validation, 400);
            }

            var normalizedName = request.Name.Trim();
            var nameExists = await _context.Services.AnyAsync(service => service.ServiceName.ToLower() == normalizedName.ToLower());
            if (nameExists)
            {
                return OperationResult<ServiceResponse>.Failure("Service name already exists.", 409);
            }

            var service = new ServiceEntity
            {
                ServiceName = normalizedName,
                Description = NormalizeOptional(request.Description),
                Price = request.Price,
                EstimatedDuration = TimeSpan.FromMinutes(request.DurationMinutes),
                Status = ServiceStatusEnum.Active
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return OperationResult<ServiceResponse>.Success(MapService(service), 201);
        }

        public async Task<OperationResult<ServiceResponse>> UpdateServiceAsync(Guid id, UpdateServiceRequest request)
        {
            var validation = ValidateServiceRequest(request.Name, request.Description, request.Price, request.DurationMinutes);
            if (validation is not null)
            {
                return OperationResult<ServiceResponse>.Failure(validation, 400);
            }

            var service = await _context.Services.FirstOrDefaultAsync(item => item.ServiceID == id);
            if (service is null)
            {
                return OperationResult<ServiceResponse>.Failure("Service not found.", 404);
            }

            var normalizedName = request.Name.Trim();
            var nameExists = await _context.Services.AnyAsync(item => item.ServiceID != id && item.ServiceName.ToLower() == normalizedName.ToLower());
            if (nameExists)
            {
                return OperationResult<ServiceResponse>.Failure("Service name already exists.", 409);
            }

            service.ServiceName = normalizedName;
            service.Description = NormalizeOptional(request.Description);
            service.Price = request.Price;
            service.EstimatedDuration = TimeSpan.FromMinutes(request.DurationMinutes);
            service.Status = request.IsActive ? ServiceStatusEnum.Active : ServiceStatusEnum.Inactive;

            await _context.SaveChangesAsync();
            return OperationResult<ServiceResponse>.Success(MapService(service));
        }

        public async Task<OperationResult<bool>> DeleteServiceAsync(Guid id)
        {
            var service = await _context.Services.FirstOrDefaultAsync(item => item.ServiceID == id);
            if (service is null)
            {
                return OperationResult<bool>.Failure("Service not found.", 404);
            }

            var hasBookings = await _context.BookingDetails.AnyAsync(detail => detail.ServiceID == id);
            service.Status = hasBookings ? ServiceStatusEnum.Archived : ServiceStatusEnum.Inactive;
            await _context.SaveChangesAsync();

            return OperationResult<bool>.Success(true);
        }

        public async Task<PagedResult<BranchListItemResponse>> GetBranchesAsync(int page, int pageSize, bool includeInactive)
        {
            page = NormalizePage(page);
            pageSize = NormalizePageSize(pageSize);

            var query = _context.Branches.AsNoTracking();
            if (!includeInactive)
            {
                query = query.Where(branch => branch.Status == BranchStatusEnum.Open);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(branch => branch.BranchName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(branch => new BranchListItemResponse
                {
                    Id = branch.BranchID,
                    Name = branch.BranchName,
                    Address = branch.Address,
                    Phone = branch.PhoneNumber,
                    OpenTime = branch.OpenTime,
                    CloseTime = branch.CloseTime,
                    IsActive = branch.Status == BranchStatusEnum.Open
                })
                .ToListAsync();

            return new PagedResult<BranchListItemResponse> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
        }

        public async Task<OperationResult<BranchResponse>> GetBranchAsync(Guid id)
        {
            var branch = await _context.Branches.AsNoTracking().FirstOrDefaultAsync(item => item.BranchID == id);
            return branch is null
                ? OperationResult<BranchResponse>.Failure("Branch not found.", 404)
                : OperationResult<BranchResponse>.Success(MapBranch(branch));
        }

        public async Task<OperationResult<BranchResponse>> CreateBranchAsync(CreateBranchRequest request)
        {
            var validation = ValidateBranchRequest(request.Name, request.Address, request.OpenTime, request.CloseTime);
            if (validation is not null)
            {
                return OperationResult<BranchResponse>.Failure(validation, 400);
            }

            var normalizedName = request.Name.Trim();
            var nameExists = await _context.Branches.AnyAsync(branch => branch.BranchName.ToLower() == normalizedName.ToLower());
            if (nameExists)
            {
                return OperationResult<BranchResponse>.Failure("Branch name already exists.", 409);
            }

            var branch = new Branch
            {
                BranchName = normalizedName,
                Address = request.Address.Trim(),
                PhoneNumber = NormalizeOptional(request.Phone),
                OpenTime = request.OpenTime,
                CloseTime = request.CloseTime,
                Status = BranchStatusEnum.Open
            };

            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();

            return OperationResult<BranchResponse>.Success(MapBranch(branch), 201);
        }

        public async Task<OperationResult<BranchResponse>> UpdateBranchAsync(Guid id, UpdateBranchRequest request)
        {
            var validation = ValidateBranchRequest(request.Name, request.Address, request.OpenTime, request.CloseTime);
            if (validation is not null)
            {
                return OperationResult<BranchResponse>.Failure(validation, 400);
            }

            var branch = await _context.Branches.FirstOrDefaultAsync(item => item.BranchID == id);
            if (branch is null)
            {
                return OperationResult<BranchResponse>.Failure("Branch not found.", 404);
            }

            var normalizedName = request.Name.Trim();
            var nameExists = await _context.Branches.AnyAsync(item => item.BranchID != id && item.BranchName.ToLower() == normalizedName.ToLower());
            if (nameExists)
            {
                return OperationResult<BranchResponse>.Failure("Branch name already exists.", 409);
            }

            branch.BranchName = normalizedName;
            branch.Address = request.Address.Trim();
            branch.PhoneNumber = NormalizeOptional(request.Phone);
            branch.OpenTime = request.OpenTime;
            branch.CloseTime = request.CloseTime;
            branch.Status = request.IsActive ? BranchStatusEnum.Open : BranchStatusEnum.Closed;

            await _context.SaveChangesAsync();
            return OperationResult<BranchResponse>.Success(MapBranch(branch));
        }

        public async Task<OperationResult<bool>> DeleteBranchAsync(Guid id)
        {
            var branch = await _context.Branches.FirstOrDefaultAsync(item => item.BranchID == id);
            if (branch is null)
            {
                return OperationResult<bool>.Failure("Branch not found.", 404);
            }

            branch.Status = BranchStatusEnum.Closed;
            await _context.SaveChangesAsync();

            return OperationResult<bool>.Success(true);
        }

        public async Task<PagedResult<WashBayListItemResponse>> GetWashBaysAsync(int page, int pageSize, Guid? branchId, bool includeInactive, IReadOnlyCollection<Guid>? allowedBranchIds = null)
        {
            page = NormalizePage(page);
            pageSize = NormalizePageSize(pageSize);

            var now = DateTime.UtcNow;
            var localNow = BangkokNow();
            var localDayStart = new DateTime(localNow.Year, localNow.Month, localNow.Day, 0, 0, 0, DateTimeKind.Unspecified);
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Bangkok");
            var offset = timeZone.GetUtcOffset(localDayStart);
            var dayStart = new DateTimeOffset(localDayStart, offset).UtcDateTime;
            var dayEnd = dayStart.AddDays(1);

            var query = _context.WashBays.AsNoTracking();
            if (allowedBranchIds is not null)
            {
                query = query.Where(washBay => allowedBranchIds.Contains(washBay.BranchID));
            }
            if (branchId.HasValue)
            {
                query = query.Where(washBay => washBay.BranchID == branchId.Value);
            }

            if (!includeInactive)
            {
                query = query.Where(washBay => washBay.Status == WashBayStatusEnum.Active);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(washBay => washBay.BayName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(washBay => new WashBayListItemResponse
                {
                    Id = washBay.WashBayID,
                    BranchId = washBay.BranchID,
                    BranchName = washBay.Branch.BranchName,
                    Name = washBay.BayName,
                    IsActive = washBay.Status == WashBayStatusEnum.Active,
                    ActiveBookingCount = washBay.Bookings.Count(booking =>
                        ActiveBookingStatuses.Contains(booking.BookingStatus) &&
                        booking.ScheduledStart >= dayStart && booking.ScheduledStart < dayEnd),
                    NextBookingAt = washBay.Bookings
                        .Where(booking => booking.BookingStatus != BookingStatusEnum.Cancelled && booking.BookingStatus != BookingStatusEnum.NoShow && booking.ScheduledStart >= now)
                        .OrderBy(booking => booking.ScheduledStart)
                        .Select(booking => (DateTime?)booking.ScheduledStart)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return new PagedResult<WashBayListItemResponse> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
        }

        public async Task<OperationResult<WashBayResponse>> GetWashBayAsync(Guid id)
        {
            var washBay = await _context.WashBays.AsNoTracking().FirstOrDefaultAsync(item => item.WashBayID == id);
            return washBay is null
                ? OperationResult<WashBayResponse>.Failure("Wash bay not found.", 404)
                : OperationResult<WashBayResponse>.Success(MapWashBay(washBay));
        }

        public async Task<OperationResult<WashBayResponse>> CreateWashBayAsync(CreateWashBayRequest request)
        {
            var validation = await ValidateWashBayRequestAsync(request.BranchId, request.Name);
            if (validation is not null)
            {
                return OperationResult<WashBayResponse>.Failure(validation, 400);
            }

            var normalizedName = request.Name.Trim();
            var nameExists = await _context.WashBays.AnyAsync(washBay => washBay.BranchID == request.BranchId && washBay.BayName.ToLower() == normalizedName.ToLower());
            if (nameExists)
            {
                return OperationResult<WashBayResponse>.Failure("Wash bay name already exists in this branch.", 409);
            }

            var washBay = new WashBay
            {
                BranchID = request.BranchId,
                BayName = normalizedName,
                Status = WashBayStatusEnum.Active
            };

            _context.WashBays.Add(washBay);
            await _context.SaveChangesAsync();

            return OperationResult<WashBayResponse>.Success(MapWashBay(washBay), 201);
        }

        public async Task<OperationResult<WashBayResponse>> UpdateWashBayAsync(Guid id, UpdateWashBayRequest request)
        {
            var validation = await ValidateWashBayRequestAsync(request.BranchId, request.Name);
            if (validation is not null)
            {
                return OperationResult<WashBayResponse>.Failure(validation, 400);
            }

            var washBay = await _context.WashBays.FirstOrDefaultAsync(item => item.WashBayID == id);
            if (washBay is null)
            {
                return OperationResult<WashBayResponse>.Failure("Wash bay not found.", 404);
            }

            var normalizedName = request.Name.Trim();
            var nameExists = await _context.WashBays.AnyAsync(item =>
                item.WashBayID != id &&
                item.BranchID == request.BranchId &&
                item.BayName.ToLower() == normalizedName.ToLower());
            if (nameExists)
            {
                return OperationResult<WashBayResponse>.Failure("Wash bay name already exists in this branch.", 409);
            }

            if (!request.IsActive && await HasActiveBookingsAsync(id))
            {
                return OperationResult<WashBayResponse>.Failure("Cannot deactivate a wash bay with pending, confirmed, or in-progress bookings.", 409);
            }

            washBay.BranchID = request.BranchId;
            washBay.BayName = normalizedName;
            washBay.Status = request.IsActive ? WashBayStatusEnum.Active : WashBayStatusEnum.Inactive;

            await _context.SaveChangesAsync();
            return OperationResult<WashBayResponse>.Success(MapWashBay(washBay));
        }

        public async Task<OperationResult<bool>> DeleteWashBayAsync(Guid id)
        {
            var washBay = await _context.WashBays.FirstOrDefaultAsync(item => item.WashBayID == id);
            if (washBay is null)
            {
                return OperationResult<bool>.Failure("Wash bay not found.", 404);
            }

            if (await HasActiveBookingsAsync(id))
            {
                return OperationResult<bool>.Failure("Cannot deactivate a wash bay with pending, confirmed, or in-progress bookings.", 409);
            }

            washBay.Status = WashBayStatusEnum.Inactive;
            await _context.SaveChangesAsync();

            return OperationResult<bool>.Success(true);
        }

        public async Task<PagedResult<BookingListItemResponse>> GetBookingsAsync(
            Guid? currentCustomerId,
            bool isAdmin,
            string? status,
            DateTime? fromDate,
            DateTime? toDate,
            Guid? branchId,
            int page,
            int pageSize, IReadOnlyCollection<Guid>? allowedBranchIds = null)
        {
            page = NormalizePage(page);
            pageSize = NormalizePageSize(pageSize);

            var query = _context.Bookings
                .AsNoTracking()
                .Include(booking => booking.Customer)
                .ThenInclude(customer => customer.User)
                .Include(booking => booking.Vehicle)
                .Include(booking => booking.Branch)
                .Include(booking => booking.BookingDetails)
                .ThenInclude(detail => detail.Service)
                .Include(booking => booking.TierSnapshot)
                .AsQueryable();

            if (allowedBranchIds is not null)
                query = query.Where(booking => allowedBranchIds.Contains(booking.BranchID));

            if (!isAdmin)
            {
                if (!currentCustomerId.HasValue)
                {
                    return new PagedResult<BookingListItemResponse> { Items = Array.Empty<BookingListItemResponse>(), Page = page, PageSize = pageSize };
                }

                query = query.Where(booking => booking.CustomerID == currentCustomerId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatusEnum>(status, true, out var parsedStatus))
            {
                query = query.Where(booking => booking.BookingStatus == parsedStatus);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(booking => booking.ScheduledStart >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(booking => booking.ScheduledStart <= toDate.Value);
            }

            if (branchId.HasValue)
            {
                query = query.Where(booking => booking.BranchID == branchId.Value);
            }

            var total = await query.CountAsync();
            var bookings = await query
                .OrderByDescending(booking => booking.ScheduledStart)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<BookingListItemResponse>
            {
                Items = bookings.Select(MapBookingListItem).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public async Task<OperationResult<IReadOnlyList<BookingListItemResponse>>> GetPendingBookingsForManagerAsync(Guid managerUserId, bool isAdmin)
        {
            var query = _context.Bookings
                .AsNoTracking()
                .Include(booking => booking.Customer)
                .ThenInclude(customer => customer.User)
                .Include(booking => booking.Vehicle)
                .Include(booking => booking.Branch)
                .Include(booking => booking.BookingDetails)
                .ThenInclude(detail => detail.Service)
                .Include(booking => booking.TierSnapshot)
                .Where(booking => booking.BookingStatus == BookingStatusEnum.Pending);

            if (!isAdmin)
            {
                var managedBranchIds = await _branchWorkforceService.GetManagedBranchIdsAsync(managerUserId);
                query = query.Where(booking => managedBranchIds.Contains(booking.BranchID));
            }

            var bookings = await query.OrderBy(booking => booking.ScheduledStart).ToListAsync();
            return OperationResult<IReadOnlyList<BookingListItemResponse>>.Success(bookings.Select(MapBookingListItem).ToList());
        }

        public async Task<OperationResult<IReadOnlyList<BookingListItemResponse>>> GetManagerBookingsAsync(Guid managerUserId, bool isAdmin, DateOnly? date = null)
        {
            var query = _context.Bookings
                .AsNoTracking()
                .Include(booking => booking.Customer).ThenInclude(customer => customer.User)
                .Include(booking => booking.Vehicle)
                .Include(booking => booking.Branch)
                .Include(booking => booking.WashBay)
                .Include(booking => booking.BookingDetails).ThenInclude(detail => detail.Service)
                .Include(booking => booking.TierSnapshot)
                .Include(booking => booking.StaffWorks).ThenInclude(work => work.StaffUser)
                .Where(booking => booking.BookingStatus != BookingStatusEnum.NoShow);

            if (!isAdmin)
            {
                var managedBranchIds = await _branchWorkforceService.GetManagedBranchIdsAsync(managerUserId);
                query = query.Where(booking => managedBranchIds.Contains(booking.BranchID));
            }

            if (date.HasValue)
            {
                var start = BranchScheduleTime.ToUtc(date.Value, TimeSpan.Zero);
                var end = BranchScheduleTime.ToUtc(date.Value.AddDays(1), TimeSpan.Zero);
                query = query.Where(booking => booking.BookingStatus == BookingStatusEnum.Pending || (booking.ScheduledStart >= start && booking.ScheduledStart < end));
            }

            var bookings = await query.OrderBy(booking => booking.ScheduledStart).ToListAsync();
            return OperationResult<IReadOnlyList<BookingListItemResponse>>.Success(bookings.Select(MapBookingListItem).ToList());
        }

        public async Task<OperationResult<BookingDetailResponse>> GetBookingAsync(Guid id, Guid? currentCustomerId, bool isAdmin)
        {
            var booking = await _context.Bookings
                .AsNoTracking()
                .Include(item => item.Branch)
                .Include(item => item.WashBay)
                .Include(item => item.AssignedStaff)
                .Include(item => item.StaffWorks)
                .ThenInclude(item => item.StaffUser)
                .Include(item => item.BookingDetails)
                .ThenInclude(detail => detail.Service)
                .Include(item => item.TierSnapshot)
                .FirstOrDefaultAsync(item => item.BookingID == id);

            if (booking is null)
            {
                return OperationResult<BookingDetailResponse>.Failure("Booking not found.", 404);
            }

            if (!isAdmin && (!currentCustomerId.HasValue || booking.CustomerID != currentCustomerId.Value))
            {
                return OperationResult<BookingDetailResponse>.Failure("You are not allowed to access this booking.", 403);
            }

            return OperationResult<BookingDetailResponse>.Success(MapBookingDetail(booking));
        }

        public async Task<OperationResult<BookingResponse>> CreateBookingAsync(CreateBookingRequest request, Guid? currentCustomerId)
        {
            if (!currentCustomerId.HasValue)
            {
                return OperationResult<BookingResponse>.Failure("Current customer is required.", 401);
            }

            if (request.VehicleId == Guid.Empty || request.BranchId == Guid.Empty || (request.ServiceId == Guid.Empty && request.Items.Count == 0))
            {
                return OperationResult<BookingResponse>.Failure("VehicleId, BranchId and ServiceId are required.", 400);
            }

            var start = BranchScheduleTime.ToUtc(request.BookingStartTime);
            if (start <= DateTime.UtcNow)
            {
                return OperationResult<BookingResponse>.Failure("Booking time must be in the future.", 400);
            }

            var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(item => item.VehicleID == request.VehicleId);
            if (vehicle is null || vehicle.CustomerID != currentCustomerId.Value)
            {
                return OperationResult<BookingResponse>.Failure("Vehicle does not belong to the current customer.", 403);
            }

            var requestedItems = request.Items.Count > 0 ? request.Items : new[] { new BookingServiceItemRequest { ServiceId = request.ServiceId, Quantity = 1 } };
            if (requestedItems.Any(item => item.ServiceId == Guid.Empty || item.Quantity <= 0)) return OperationResult<BookingResponse>.Failure("Each service item must have a valid service and quantity.", 400);
            var serviceIds = requestedItems.Select(item => item.ServiceId).Distinct().ToArray();
            if (serviceIds.Length != requestedItems.Count) return OperationResult<BookingResponse>.Failure("A service can only appear once in a booking.", 400);
            var services = await _context.Services.AsNoTracking().Where(item => serviceIds.Contains(item.ServiceID) && item.Status == ServiceStatusEnum.Active).ToListAsync();
            if (services.Count != serviceIds.Length) return OperationResult<BookingResponse>.Failure("One or more services are unavailable.", 400);
            var duration = TimeSpan.FromMinutes(requestedItems.Sum(item => (services.Single(service => service.ServiceID == item.ServiceId).EstimatedDuration ?? TimeSpan.Zero).TotalMinutes * item.Quantity));
            if (duration.TotalMinutes <= 0)
            {
                return OperationResult<BookingResponse>.Failure("Service duration must be greater than 0.", 400);
            }

            var branch = await _context.Branches.AsNoTracking().FirstOrDefaultAsync(item => item.BranchID == request.BranchId);
            if (branch is null || branch.Status != BranchStatusEnum.Open)
            {
                return OperationResult<BookingResponse>.Failure("Branch not found or inactive.", 400);
            }

            var end = start.Add(duration);
            if (!IsInsideOperatingHours(branch, start, end))
            {
                return OperationResult<BookingResponse>.Failure("Booking time must be within branch operating hours.", 400);
            }

            var customer = await _context.Customers
                .AsNoTracking()
                .Include(item => item.Tier)
                .FirstOrDefaultAsync(item => item.CustomerID == currentCustomerId.Value);
            if (customer is null)
            {
                return OperationResult<BookingResponse>.Failure("Current customer not found.", 401);
            }

            if (customer.Tier is not null && customer.Tier.BookingWindowDays > 0 && start > DateTime.UtcNow.AddDays(customer.Tier.BookingWindowDays))
            {
                return OperationResult<BookingResponse>.Failure("Booking time exceeds current tier booking window.", 400);
            }

            var washBay = await FindAvailableWashBayAsync(request.BranchId, start, end);
            if (washBay is null)
            {
                return OperationResult<BookingResponse>.Failure("No available wash bay for selected time window.", 409);
            }

            var booking = new Booking
            {
                CustomerID = currentCustomerId.Value,
                VehicleID = request.VehicleId,
                BranchID = request.BranchId,
                WashBayID = washBay.WashBayID,
                TierIDSnapshot = customer.TierID,
                ScheduledStart = start,
                ScheduledEnd = end,
                BookingStatus = BookingStatusEnum.Pending,
                QueuePriority = customer.Tier?.PriorityLevel ?? 0,
                EstimatedTotalAmount = requestedItems.Sum(item => services.Single(service => service.ServiceID == item.ServiceId).Price * item.Quantity),
                Notes = NormalizeOptional(request.Note)
            };

            foreach (var item in requestedItems)
            {
                var selected = services.Single(service => service.ServiceID == item.ServiceId);
                booking.BookingDetails.Add(new BookingDetail
                {
                    BookingID = booking.BookingID,
                    ServiceID = selected.ServiceID,
                    Quantity = item.Quantity,
                    UnitPrice = selected.Price
                });
            }

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            await _behavioralLogWriter.WriteAsync(new BehavioralLogWriteRequest
            {
                CustomerId = booking.CustomerID,
                BookingId = booking.BookingID,
                ServiceId = requestedItems[0].ServiceId,
                ActionType = BehavioralActionTypeEnum.Book,
                SpendingAmount = booking.EstimatedTotalAmount,
                Notes = "Booking created"
            });

            foreach (var detail in booking.BookingDetails)
            {
                detail.Service = services.First(item => item.ServiceID == detail.ServiceID);
            }
            booking.TierSnapshot = customer.Tier;

            return OperationResult<BookingResponse>.Success(MapBooking(booking), 201);
        }

        public async Task<OperationResult<BookingResponse>> CancelBookingAsync(Guid id, CancelBookingRequest request, Guid? currentCustomerId, bool isAdmin)
        {
            var booking = await LoadBookingAsync(id);
            if (booking is null)
            {
                return OperationResult<BookingResponse>.Failure("Booking not found.", 404);
            }

            if (!isAdmin && (!currentCustomerId.HasValue || booking.CustomerID != currentCustomerId.Value))
            {
                return OperationResult<BookingResponse>.Failure("You are not allowed to cancel this booking.", 403);
            }

            if (booking.BookingStatus == BookingStatusEnum.Completed)
            {
                return OperationResult<BookingResponse>.Failure("Completed booking cannot be cancelled.", 400);
            }

            if (await _context.Payments.AnyAsync(payment => payment.BookingID == id && payment.PaymentStatus == PaymentStatusEnum.Paid))
            {
                return OperationResult<BookingResponse>.Failure("Paid booking cannot be cancelled.", 400);
            }

            booking.BookingStatus = BookingStatusEnum.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;
            booking.CancellationReason = NormalizeOptional(request.Reason);
            booking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _behavioralLogWriter.WriteAsync(new BehavioralLogWriteRequest
            {
                CustomerId = booking.CustomerID,
                BookingId = booking.BookingID,
                ServiceId = booking.BookingDetails.FirstOrDefault()?.ServiceID,
                ActionType = BehavioralActionTypeEnum.CancelBooking,
                Notes = booking.CancellationReason
            });
            return OperationResult<BookingResponse>.Success(MapBooking(booking));
        }

        public async Task<OperationResult<BookingResponse>> ConfirmBookingAsync(Guid id, Guid managerUserId)
        {
            var booking = await LoadBookingAsync(id);
            if (booking is null)
            {
                return OperationResult<BookingResponse>.Failure("Booking not found.", 404);
            }

            if (!await _branchWorkforceService.CanManageBranchAsync(managerUserId, booking.BranchID))
            {
                return OperationResult<BookingResponse>.Failure("You cannot confirm bookings for this branch.", 403);
            }

            if (booking.BookingStatus != BookingStatusEnum.Pending)
            {
                return OperationResult<BookingResponse>.Failure("Only pending bookings can be confirmed.", 400);
            }

            booking.BookingStatus = BookingStatusEnum.Confirmed;
            booking.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return OperationResult<BookingResponse>.Success(MapBooking(booking));
        }

        public async Task<OperationResult<BookingResponse>> StartBookingAsync(Guid id, Guid? managerUserId = null)
        {
            if (managerUserId.HasValue)
            {
                var booking = await _context.Bookings.AsNoTracking().FirstOrDefaultAsync(item => item.BookingID == id);
                if (booking is null) return OperationResult<BookingResponse>.Failure("Booking not found.", 404);
                if (!await _branchWorkforceService.CanManageBranchAsync(managerUserId.Value, booking.BranchID)) return OperationResult<BookingResponse>.Failure("You cannot manage this branch.", 403);
            }
            var result = await TransitionBookingAsync(id, BookingStatusEnum.CheckedIn, BookingStatusEnum.InProgress, "Only checked-in bookings can be started.");
            if (result.Succeeded && result.Data is not null)
            {
                var booking = await _context.Bookings.FirstAsync(item => item.BookingID == id);
                booking.StartedAt ??= DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return result;
        }

        public async Task<OperationResult<BookingResponse>> CompleteBookingAsync(Guid id, Guid? managerUserId = null)
        {
            var booking = await LoadBookingAsync(id);
            if (booking is null)
            {
                return OperationResult<BookingResponse>.Failure("Booking not found.", 404);
            }

            if (booking.BookingStatus == BookingStatusEnum.Completed)
            {
                return OperationResult<BookingResponse>.Success(MapBooking(booking));
            }

            if (booking.BookingStatus != BookingStatusEnum.InProgress)
            {
                return OperationResult<BookingResponse>.Failure("Only in-progress bookings can be completed.", 400);
            }

            if (managerUserId.HasValue && !await _branchWorkforceService.CanManageBranchAsync(managerUserId.Value, booking.BranchID)) return OperationResult<BookingResponse>.Failure("You cannot manage this branch.", 403);
            if (booking.StaffWorks.Count == 0)
            {
                return OperationResult<BookingResponse>.Failure("Assign at least one checked-in staff member before completing this booking.", 400);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var completedAt = DateTime.UtcNow;
                booking.BookingStatus = BookingStatusEnum.Completed;
                booking.CompletedAt = completedAt;
                booking.UpdatedAt = completedAt;

                await _washCompletionService.CompleteWashAsync(new WashCompletionPayload
                {
                    BookingId = booking.BookingID,
                    CustomerId = booking.CustomerID,
                    VehicleId = booking.VehicleID,
                    ServiceId = booking.BookingDetails.First().ServiceID,
                    BranchId = booking.BranchID,
                    Amount = booking.EstimatedTotalAmount,
                    CompletedAt = completedAt
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return OperationResult<BookingResponse>.Success(MapBooking(booking));
        }

        public async Task<OperationResult<BookingAvailabilityResponse>> GetAvailabilityAsync(Guid branchId, Guid serviceId, DateTime date)
        {
            return await GetAvailabilityAsync(new BookingAvailabilityRequest
            {
                BranchId = branchId,
                Date = DateOnly.FromDateTime(date),
                Items = new[] { new BookingServiceItemRequest { ServiceId = serviceId, Quantity = 1 } }
            });
        }

        public async Task<OperationResult<BookingAvailabilityResponse>> GetAvailabilityAsync(BookingAvailabilityRequest request)
        {
            if (request.BranchId == Guid.Empty || request.Items.Count == 0 || request.Items.Any(item => item.ServiceId == Guid.Empty || item.Quantity <= 0))
                return OperationResult<BookingAvailabilityResponse>.Failure("BranchId and at least one valid service item are required.", 400);
            var serviceIds = request.Items.Select(item => item.ServiceId).Distinct().ToArray();
            if (serviceIds.Length != request.Items.Count) return OperationResult<BookingAvailabilityResponse>.Failure("A service can only appear once in availability request.", 400);
            var branch = await _context.Branches.AsNoTracking().FirstOrDefaultAsync(item => item.BranchID == request.BranchId && item.Status == BranchStatusEnum.Open);
            var services = await _context.Services.AsNoTracking().Where(item => serviceIds.Contains(item.ServiceID) && item.Status == ServiceStatusEnum.Active).ToListAsync();
            if (branch is null || services.Count != serviceIds.Length || !branch.OpenTime.HasValue || !branch.CloseTime.HasValue) return OperationResult<BookingAvailabilityResponse>.Failure("Branch or service is unavailable.", 400);
            var duration = TimeSpan.FromMinutes(request.Items.Sum(item => (services.Single(service => service.ServiceID == item.ServiceId).EstimatedDuration ?? TimeSpan.Zero).TotalMinutes * item.Quantity));
            if (duration <= TimeSpan.Zero) return OperationResult<BookingAvailabilityResponse>.Failure("Service duration must be greater than 0.", 400);
            var open = BranchScheduleTime.ToUtc(request.Date, branch.OpenTime.Value);
            var close = BranchScheduleTime.ToUtc(request.Date, branch.CloseTime.Value);
            var slots = new List<BookingAvailabilitySlotResponse>();
            for (var start = open; start.Add(duration) <= close; start = start.AddHours(1))
            {
                if (start <= DateTime.UtcNow) continue;
                var availableBayCount = await CountAvailableWashBaysAsync(request.BranchId, start, start.Add(duration));
                if (availableBayCount > 0) slots.Add(new BookingAvailabilitySlotResponse { StartTime = start, EndTime = start.Add(duration), AvailableBayCount = availableBayCount });
            }
            return OperationResult<BookingAvailabilityResponse>.Success(new BookingAvailabilityResponse
            {
                BranchId = request.BranchId,
                ServiceId = request.Items.Count == 1 ? request.Items[0].ServiceId : Guid.Empty,
                DurationMinutes = (int)duration.TotalMinutes,
                AvailableStartTimes = slots.Select(item => item.StartTime).ToList(),
                Slots = slots
            });
        }

        public async Task<OperationResult<BookingResponse>> RescheduleBookingAsync(Guid id, RescheduleBookingRequest request, Guid? currentCustomerId, bool isAdmin)
        {
            var booking = await LoadBookingAsync(id);
            if (booking is null) return OperationResult<BookingResponse>.Failure("Booking not found.", 404);
            if (!isAdmin && (!currentCustomerId.HasValue || booking.CustomerID != currentCustomerId.Value)) return OperationResult<BookingResponse>.Failure("You are not allowed to reschedule this booking.", 403);
            if (request.ExpectedVersion.HasValue && request.ExpectedVersion.Value != booking.Version) return OperationResult<BookingResponse>.Failure("Booking has changed. Refresh and try again.", 409);
            if (booking.BookingStatus != BookingStatusEnum.Pending) return OperationResult<BookingResponse>.Failure("Only pending bookings can be rescheduled.", 400);
            var start = BranchScheduleTime.ToUtc(request.BookingStartTime);
            if (start <= DateTime.UtcNow) return OperationResult<BookingResponse>.Failure("Booking time must be in the future.", 400);
            var branch = await _context.Branches.AsNoTracking().FirstOrDefaultAsync(item => item.BranchID == booking.BranchID);
            if (branch is null || branch.Status != BranchStatusEnum.Open) return OperationResult<BookingResponse>.Failure("Branch is unavailable.", 400);
            var duration = booking.ScheduledEnd - booking.ScheduledStart;
            var end = start.Add(duration);
            if (!IsInsideOperatingHours(branch, start, end)) return OperationResult<BookingResponse>.Failure("Booking time must be within branch operating hours.", 400);
            var bay = request.WashBayId.HasValue
                ? await _context.WashBays.FirstOrDefaultAsync(item => item.WashBayID == request.WashBayId.Value && item.BranchID == booking.BranchID && item.Status == WashBayStatusEnum.Active)
                : await FindAvailableWashBayAsync(booking.BranchID, start, end, booking.BookingID);
            if (bay is null) return OperationResult<BookingResponse>.Failure("No available wash bay for selected time window.", 409);
            var previousStart = booking.ScheduledStart;
            var previousEnd = booking.ScheduledEnd;
            var previousWashBayId = booking.WashBayID;
            booking.ScheduledStart = start;
            booking.ScheduledEnd = end;
            booking.WashBayID = bay.WashBayID;
            booking.Notes = NormalizeOptional(request.Note) ?? booking.Notes;
            booking.UpdatedAt = DateTime.UtcNow;
            booking.Version++;
            _context.BookingRescheduleHistories.Add(new BookingRescheduleHistory
            {
                BookingID = booking.BookingID,
                PreviousStart = previousStart,
                PreviousEnd = previousEnd,
                PreviousWashBayID = previousWashBayId,
                NewStart = start,
                NewEnd = end,
                NewWashBayID = bay.WashBayID,
                ChangedByCustomerID = currentCustomerId,
                Note = NormalizeOptional(request.Note)
            });
            await _context.SaveChangesAsync();
            return OperationResult<BookingResponse>.Success(MapBooking(booking));
        }

        public async Task<OperationResult<BookingResponse>> CheckInBookingAsync(Guid id, Guid? managerUserId = null)
        {
            var booking = await _context.Bookings.Include(item => item.BookingDetails).ThenInclude(item => item.Service).FirstOrDefaultAsync(item => item.BookingID == id);
            if (booking is null) return OperationResult<BookingResponse>.Failure("Booking not found.", 404);
            if (managerUserId.HasValue && !await _branchWorkforceService.CanManageBranchAsync(managerUserId.Value, booking.BranchID)) return OperationResult<BookingResponse>.Failure("You cannot manage this branch.", 403);
            if (booking.BookingStatus != BookingStatusEnum.Confirmed) return OperationResult<BookingResponse>.Failure("Only confirmed bookings can be checked in.", 400);
            booking.CheckInAt = DateTime.UtcNow;
            booking.BookingStatus = BookingStatusEnum.CheckedIn;
            booking.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return OperationResult<BookingResponse>.Success(MapBooking(booking));
        }

        public async Task<OperationResult<BookingResponse>> MarkNoShowAsync(Guid id)
        {
            var booking = await _context.Bookings.Include(item => item.BookingDetails).ThenInclude(item => item.Service).FirstOrDefaultAsync(item => item.BookingID == id);
            if (booking is null) return OperationResult<BookingResponse>.Failure("Booking not found.", 404);
            if (booking.BookingStatus is not (BookingStatusEnum.Pending or BookingStatusEnum.Confirmed)) return OperationResult<BookingResponse>.Failure("Only pending or confirmed bookings can be marked no-show.", 400);
            booking.BookingStatus = BookingStatusEnum.NoShow;
            booking.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return OperationResult<BookingResponse>.Success(MapBooking(booking));
        }

        public async Task<IReadOnlyList<QueueBookingResponse>> GetQueueAsync(Guid branchId, Guid? washBayId)
        {
            var query = _context.Bookings.AsNoTracking()
                .Include(item => item.BookingDetails).ThenInclude(item => item.Service)
                .Where(item => item.BranchID == branchId && item.CheckInAt.HasValue &&
                    (item.BookingStatus == BookingStatusEnum.CheckedIn || item.BookingStatus == BookingStatusEnum.InProgress));
            if (washBayId.HasValue) query = query.Where(item => item.WashBayID == washBayId.Value);

            var bookings = await query
                .OrderByDescending(item => item.QueuePriority)
                .ThenBy(item => item.CheckInAt)
                .ThenBy(item => item.ScheduledStart)
                .ToListAsync();

            var eta = DateTime.UtcNow;
            return bookings.Select((booking, index) =>
            {
                var response = new QueueBookingResponse
                {
                    BookingId = booking.BookingID,
                    BranchId = booking.BranchID,
                    WashBayId = booking.WashBayID,
                    ServiceName = booking.BookingDetails.FirstOrDefault()?.Service.ServiceName ?? string.Empty,
                    ScheduledStart = booking.ScheduledStart,
                    CheckedInAt = booking.CheckInAt!.Value,
                    Priority = booking.QueuePriority,
                    Position = index + 1,
                    EstimatedStart = eta
                };
                eta = eta.AddMinutes(Math.Max(10, (booking.ScheduledEnd - booking.ScheduledStart).TotalMinutes));
                return response;
            }).ToList();
        }

        public async Task<OperationResult<BookingResponse>> DispatchBookingAsync(Guid id, DispatchBookingRequest request, Guid? managerUserId = null)
        {
            if (request.WashBayId == Guid.Empty) return OperationResult<BookingResponse>.Failure("WashBayId is required.", 400);
            var booking = await LoadBookingAsync(id);
            if (booking is null) return OperationResult<BookingResponse>.Failure("Booking not found.", 404);
            if (managerUserId.HasValue && !await _branchWorkforceService.CanManageBranchAsync(managerUserId.Value, booking.BranchID)) return OperationResult<BookingResponse>.Failure("You cannot manage this branch.", 403);
            if (booking.BookingStatus is not (BookingStatusEnum.Confirmed or BookingStatusEnum.CheckedIn))
                return OperationResult<BookingResponse>.Failure("Only confirmed or checked-in bookings can be dispatched.", 400);

            var washBay = await _context.WashBays.FirstOrDefaultAsync(item => item.WashBayID == request.WashBayId);
            if (washBay is null || washBay.BranchID != booking.BranchID || washBay.Status != WashBayStatusEnum.Active)
                return OperationResult<BookingResponse>.Failure("Wash bay must be active and belong to the booking branch.", 400);

            var overlaps = await _context.Bookings.AnyAsync(item => item.BookingID != booking.BookingID && item.WashBayID == request.WashBayId && ActiveBookingStatuses.Contains(item.BookingStatus) && item.ScheduledStart < booking.ScheduledEnd && booking.ScheduledStart < item.ScheduledEnd);
            if (overlaps) return OperationResult<BookingResponse>.Failure("Wash bay is not available for this booking time.", 409);

            booking.WashBayID = request.WashBayId;
            booking.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return OperationResult<BookingResponse>.Success(MapBooking(booking));
        }

        public async Task<OperationResult<BookingResponse>> AssignBookingStaffAsync(Guid id, AssignBookingStaffRequest request, Guid? managerUserId = null)
        {
            if (request.StaffUserId == Guid.Empty) return OperationResult<BookingResponse>.Failure("StaffUserId is required.", 400);
            var booking = await LoadBookingAsync(id);
            if (booking is null) return OperationResult<BookingResponse>.Failure("Booking not found.", 404);
            if (managerUserId.HasValue && !await _branchWorkforceService.CanManageBranchAsync(managerUserId.Value, booking.BranchID)) return OperationResult<BookingResponse>.Failure("You cannot manage this branch.", 403);
            if (request.ExpectedVersion.HasValue && request.ExpectedVersion.Value != booking.Version)
                return OperationResult<BookingResponse>.Failure("Booking has changed. Refresh and try again.", 409);
            if (booking.BookingStatus is BookingStatusEnum.Completed or BookingStatusEnum.Cancelled or BookingStatusEnum.NoShow)
                return OperationResult<BookingResponse>.Failure("Staff cannot be assigned to a closed booking.", 400);

            var isAssignedToShift = await _context.ShiftAssignments
                .Include(item => item.StaffShift)
                .Include(item => item.AttendanceRecord)
                .AnyAsync(item => item.UserID == request.StaffUserId && item.StaffShift.BranchID == booking.BranchID && item.StaffShift.IsActive && item.StaffShift.StartsAt <= booking.ScheduledStart && item.StaffShift.EndsAt >= booking.ScheduledEnd && item.AttendanceRecord != null && item.AttendanceRecord.CheckedInAt != null && item.AttendanceRecord.CheckedOutAt == null && (!booking.WashBayID.HasValue || !item.WashBayID.HasValue || item.WashBayID == booking.WashBayID));
            if (!isAssignedToShift) return OperationResult<BookingResponse>.Failure("Staff member must be checked in to an active compatible shift.", 400);

            booking.AssignedStaffID = request.StaffUserId;
            booking.Version++;
            booking.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return OperationResult<BookingResponse>.Success(MapBooking(booking));
        }

        public async Task<IReadOnlyList<EligibleBookingStaffResponse>> GetEligibleBookingStaffAsync(Guid id, Guid actorId)
        {
            var booking = await _context.Bookings.AsNoTracking().FirstOrDefaultAsync(item => item.BookingID == id);
            if (booking is null || !await _branchWorkforceService.CanManageBranchAsync(actorId, booking.BranchID))
                return Array.Empty<EligibleBookingStaffResponse>();

            var workDate = UtcDate(BangkokNow());
            return await _context.BranchStaffMemberships.AsNoTracking()
                .Include(item => item.User)
                .Where(item => item.BranchID == booking.BranchID && item.IsActive && item.EffectiveFrom <= workDate && (!item.EffectiveTo.HasValue || item.EffectiveTo >= workDate))
                .Where(item => _context.AttendanceRecords.Any(record => record.BranchStaffMembershipID == item.BranchStaffMembershipID && record.WorkDate == workDate && record.CheckedInAt != null && record.CheckedOutAt == null))
                .OrderBy(item => item.User.FullName)
                .Select(item => new EligibleBookingStaffResponse { UserId = item.UserID, FullName = item.User.FullName, MembershipId = item.BranchStaffMembershipID })
                .ToListAsync();
        }

        public async Task<OperationResult<BookingResponse>> SetBookingStaffWorkAsync(Guid id, SetBookingStaffWorkRequest request, Guid actorId)
        {
            if (request.Staff.Count == 0) return OperationResult<BookingResponse>.Failure("At least one staff member is required.", 400);
            if (request.Staff.Any(item => item.StaffUserId == Guid.Empty || item.ContributionPercent <= 0 || item.ContributionPercent > 100))
                return OperationResult<BookingResponse>.Failure("Each staff member needs a valid id and contribution between 0 and 100.", 400);
            if (request.Staff.Select(item => item.StaffUserId).Distinct().Count() != request.Staff.Count)
                return OperationResult<BookingResponse>.Failure("A staff member can only appear once in a booking.", 400);
            if (request.Staff.Sum(item => item.ContributionPercent) != 100m)
                return OperationResult<BookingResponse>.Failure("Total contribution must equal 100 percent.", 400);

            var booking = await LoadBookingAsync(id);
            if (booking is null) return OperationResult<BookingResponse>.Failure("Booking not found.", 404);
            if (!await _branchWorkforceService.CanManageBranchAsync(actorId, booking.BranchID)) return OperationResult<BookingResponse>.Failure("You cannot manage this branch.", 403);
            if (booking.BookingStatus is BookingStatusEnum.Completed or BookingStatusEnum.Cancelled or BookingStatusEnum.NoShow)
                return OperationResult<BookingResponse>.Failure("Staff work is locked for a closed booking.", 400);
            if (request.ExpectedVersion.HasValue && request.ExpectedVersion.Value != booking.Version)
                return OperationResult<BookingResponse>.Failure("Booking has changed. Refresh and try again.", 409);

            var eligibleIds = (await GetEligibleBookingStaffAsync(id, actorId)).Select(item => item.UserId).ToHashSet();
            if (request.Staff.Any(item => !eligibleIds.Contains(item.StaffUserId)))
                return OperationResult<BookingResponse>.Failure("Every staff member must be checked in and belong to this branch.", 400);

            var isEqualSplit = request.Staff.Max(item => item.ContributionPercent) - request.Staff.Min(item => item.ContributionPercent) <= 0.01m;
            if (!isEqualSplit && string.IsNullOrWhiteSpace(request.AdjustmentReason))
                return OperationResult<BookingResponse>.Failure("A reason is required when contribution is not split equally.", 400);

            var now = DateTime.UtcNow;
            try
            {
                // Replace the assignment set by BookingID instead of asking EF to delete
                // each previously tracked child. This is safe for re-assignment and avoids
                // stale child rows producing a DbUpdateConcurrencyException.
                await using var transaction = await _context.Database.BeginTransactionAsync();
                _context.ChangeTracker.Clear();

                await _context.BookingStaffWorks
                    .Where(item => item.BookingID == id)
                    .ExecuteDeleteAsync();

                await _context.BookingStaffWorks.AddRangeAsync(request.Staff.Select(item => new BookingStaffWork
                {
                    BookingID = id,
                    StaffUserID = item.StaffUserId,
                    ContributionPercent = item.ContributionPercent,
                    WorkRole = item.WorkRole?.Trim(),
                    AdjustmentReason = isEqualSplit ? null : request.AdjustmentReason?.Trim(),
                    AssignedByUserID = actorId,
                    AssignedAt = now
                }));

                var updatedBookings = await _context.Bookings
                    .Where(item => item.BookingID == id && item.BookingStatus != BookingStatusEnum.Completed && item.BookingStatus != BookingStatusEnum.Cancelled && item.BookingStatus != BookingStatusEnum.NoShow)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(item => item.AssignedStaffID, request.Staff[0].StaffUserId)
                        .SetProperty(item => item.Version, item => item.Version + 1)
                        .SetProperty(item => item.UpdatedAt, now));

                if (updatedBookings == 0)
                {
                    await transaction.RollbackAsync();
                    return OperationResult<BookingResponse>.Failure("Staff work is locked for a closed booking.", 400);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return OperationResult<BookingResponse>.Failure("Booking has changed. Refresh and try again.", 409);
            }

            var updatedBooking = await LoadBookingAsync(id);
            return OperationResult<BookingResponse>.Success(MapBooking(updatedBooking!));
        }

        public async Task<OperationResult<IReadOnlyList<BookingRescheduleHistoryResponse>>> GetRescheduleHistoryAsync(Guid id, Guid? currentCustomerId, bool isAdmin)
        {
            var booking = await _context.Bookings.AsNoTracking().FirstOrDefaultAsync(item => item.BookingID == id);
            if (booking is null) return OperationResult<IReadOnlyList<BookingRescheduleHistoryResponse>>.Failure("Booking not found.", 404);
            if (!isAdmin && (!currentCustomerId.HasValue || booking.CustomerID != currentCustomerId.Value))
                return OperationResult<IReadOnlyList<BookingRescheduleHistoryResponse>>.Failure("You are not allowed to access this booking.", 403);
            var items = await _context.BookingRescheduleHistories.AsNoTracking().Where(item => item.BookingID == id).OrderByDescending(item => item.ChangedAt).Select(item => new BookingRescheduleHistoryResponse
            {
                Id = item.BookingRescheduleHistoryID,
                PreviousStart = item.PreviousStart,
                NewStart = item.NewStart,
                PreviousWashBayId = item.PreviousWashBayID,
                NewWashBayId = item.NewWashBayID,
                Note = item.Note,
                ChangedAt = item.ChangedAt
            }).ToListAsync();
            return OperationResult<IReadOnlyList<BookingRescheduleHistoryResponse>>.Success(items);
        }

        public async Task<OperationResult<BookingReadSnapshot>> GetSnapshotAsync(Guid bookingId)
        {
            var booking = await _context.Bookings
                .AsNoTracking()
                .Include(item => item.BookingDetails)
                .ThenInclude(detail => detail.Service)
                .FirstOrDefaultAsync(item => item.BookingID == bookingId);

            if (booking is null)
            {
                return OperationResult<BookingReadSnapshot>.Failure("Booking not found.", 404);
            }

            var detail = booking.BookingDetails.FirstOrDefault();
            return OperationResult<BookingReadSnapshot>.Success(new BookingReadSnapshot
            {
                BookingId = booking.BookingID,
                CustomerId = booking.CustomerID,
                VehicleId = booking.VehicleID,
                BranchId = booking.BranchID,
                ServiceId = detail?.ServiceID ?? Guid.Empty,
                ScheduledStart = booking.ScheduledStart,
                ScheduledEnd = booking.ScheduledEnd,
                Status = booking.BookingStatus.ToString(),
                TotalAmount = booking.EstimatedTotalAmount,
                ServiceName = detail?.Service.ServiceName ?? string.Empty
            });
        }

        public async Task<OperationResult<PaymentResponse>> GetPaymentAsync(Guid id)
        {
            var payment = await _context.Payments.AsNoTracking().Include(item => item.Booking).FirstOrDefaultAsync(item => item.PaymentID == id);
            return payment is null
                ? OperationResult<PaymentResponse>.Failure("Payment not found.", 404)
                : OperationResult<PaymentResponse>.Success(MapPayment(payment));
        }

        public async Task<PagedResult<PaymentListItemResponse>> GetPaymentsAsync(Guid? branchId, string? status, DateTime? from, DateTime? to, int page, int pageSize, IReadOnlyCollection<Guid>? allowedBranchIds = null)
        {
            page = NormalizePage(page); pageSize = NormalizePageSize(pageSize);
            var query = _context.Payments.AsNoTracking().Include(item => item.Booking).AsQueryable();
            if (allowedBranchIds is not null) query = query.Where(item => allowedBranchIds.Contains(item.Booking.BranchID));
            if (branchId.HasValue) query = query.Where(item => item.Booking.BranchID == branchId.Value);
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatusEnum>(status, true, out var paymentStatus)) query = query.Where(item => item.PaymentStatus == paymentStatus);
            if (from.HasValue) query = query.Where(item => item.RecordedAt >= from.Value);
            if (to.HasValue) query = query.Where(item => item.RecordedAt <= to.Value);
            var total = await query.CountAsync();
            var payments = await query.OrderByDescending(item => item.RecordedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var ids = payments.Select(item => item.PaymentID).ToList();
            var refunds = await _context.Refunds.AsNoTracking().Where(item => ids.Contains(item.PaymentID) && item.Status == "Completed").GroupBy(item => item.PaymentID).Select(group => new { PaymentId = group.Key, Amount = group.Sum(item => item.Amount) }).ToDictionaryAsync(item => item.PaymentId, item => item.Amount);
            return new PagedResult<PaymentListItemResponse>
            {
                Items = payments.Select(payment => new PaymentListItemResponse
                {
                    Id = payment.PaymentID, BookingId = payment.BookingID, BranchId = payment.Booking.BranchID, Amount = payment.Amount, Method = payment.PaymentMethod.ToString(), Status = payment.PaymentStatus.ToString(), CreatedAt = payment.RecordedAt, PaidAt = payment.PaidAt, ReferenceNumber = payment.ReferenceNumber, Note = payment.Notes,
                    RefundedAmount = refunds.GetValueOrDefault(payment.PaymentID), RefundableAmount = payment.PaymentStatus == PaymentStatusEnum.Paid ? payment.Amount - refunds.GetValueOrDefault(payment.PaymentID) : 0m
                }).ToList(), Page = page, PageSize = pageSize, TotalCount = total
            };
        }

        public async Task<OperationResult<PaymentResponse>> CreatePaymentAsync(CreatePaymentRequest request)
        {
            if (request.BookingId == Guid.Empty)
            {
                return OperationResult<PaymentResponse>.Failure("BookingId is required.", 400);
            }

            if (request.Amount <= 0)
            {
                return OperationResult<PaymentResponse>.Failure("Amount must be greater than 0.", 400);
            }

            if (!TryParsePaymentMethod(request.Method, out var method))
            {
                return OperationResult<PaymentResponse>.Failure("Payment method must be Cash, Card or BankTransfer.", 400);
            }

            var booking = await _context.Bookings.AsNoTracking().FirstOrDefaultAsync(item => item.BookingID == request.BookingId);
            if (booking is null)
            {
                return OperationResult<PaymentResponse>.Failure("Booking not found.", 404);
            }

            if (booking.BookingStatus == BookingStatusEnum.Cancelled)
            {
                return OperationResult<PaymentResponse>.Failure("Cannot create payment for cancelled booking.", 400);
            }

            if (request.Amount != booking.EstimatedTotalAmount)
            {
                return OperationResult<PaymentResponse>.Failure("Payment amount must equal booking total amount.", 400);
            }

            var payment = new Payment
            {
                BookingID = request.BookingId,
                Amount = request.Amount,
                PaymentMethod = method,
                PaymentStatus = PaymentStatusEnum.Pending,
                Notes = NormalizeOptional(request.Note)
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return OperationResult<PaymentResponse>.Success(MapPayment(payment), 201);
        }

        public async Task<OperationResult<PaymentResponse>> MarkPaymentPaidAsync(Guid id, MarkPaymentPaidRequest request)
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(item => item.PaymentID == id);
            if (payment is null)
            {
                return OperationResult<PaymentResponse>.Failure("Payment not found.", 404);
            }

            if (payment.PaymentStatus == PaymentStatusEnum.Voided)
            {
                return OperationResult<PaymentResponse>.Failure("Voided payment cannot be marked paid.", 400);
            }

            if (payment.PaymentStatus != PaymentStatusEnum.Pending)
            {
                return OperationResult<PaymentResponse>.Failure("Only pending payment can be marked paid.", 400);
            }

            payment.PaymentStatus = PaymentStatusEnum.Paid;
            payment.PaidAt = DateTime.UtcNow;
            payment.ReferenceNumber = NormalizeOptional(request.ReferenceNumber);
            payment.Notes = MergeNotes(payment.Notes, request.Note);

            await _context.SaveChangesAsync();
            return OperationResult<PaymentResponse>.Success(MapPayment(payment));
        }

        public async Task<OperationResult<PaymentResponse>> VoidPaymentAsync(Guid id, VoidPaymentRequest request)
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(item => item.PaymentID == id);
            if (payment is null)
            {
                return OperationResult<PaymentResponse>.Failure("Payment not found.", 404);
            }

            if (payment.PaymentStatus == PaymentStatusEnum.Paid)
            {
                return OperationResult<PaymentResponse>.Failure("Paid payment cannot be voided.", 400);
            }

            if (payment.PaymentStatus != PaymentStatusEnum.Pending)
            {
                return OperationResult<PaymentResponse>.Failure("Only pending payment can be voided.", 400);
            }

            payment.PaymentStatus = PaymentStatusEnum.Voided;
            payment.Notes = MergeNotes(payment.Notes, request.Note);

            await _context.SaveChangesAsync();
            return OperationResult<PaymentResponse>.Success(MapPayment(payment));
        }

        public async Task<OperationResult<RefundResponse>> CreateRefundAsync(Guid paymentId, CreateRefundRequest request)
        {
            if (request.Amount <= 0)
            {
                return OperationResult<RefundResponse>.Failure("Refund amount must be greater than 0.", 400);
            }

            if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length > 500)
            {
                return OperationResult<RefundResponse>.Failure("Reason is required and must be at most 500 characters.", 400);
            }

            var payment = await _context.Payments.FirstOrDefaultAsync(item => item.PaymentID == paymentId);
            if (payment is null)
            {
                return OperationResult<RefundResponse>.Failure("Payment not found.", 404);
            }

            if (payment.PaymentStatus != PaymentStatusEnum.Paid)
            {
                return OperationResult<RefundResponse>.Failure("Only paid payments can be refunded.", 400);
            }

            var refunded = await _context.Refunds
                .Where(item => item.PaymentID == paymentId && item.Status == "Completed")
                .SumAsync(item => (decimal?)item.Amount) ?? 0m;
            if (refunded + request.Amount > payment.Amount)
            {
                return OperationResult<RefundResponse>.Failure("Refund amount exceeds the remaining paid amount.", 400);
            }

            var refund = new Refund
            {
                PaymentID = paymentId,
                Amount = request.Amount,
                Reason = request.Reason.Trim(),
                Status = "Completed",
                ReferenceNumber = NormalizeOptional(request.ReferenceNumber),
                CompletedAt = DateTime.UtcNow
            };
            _context.Refunds.Add(refund);
            await _context.SaveChangesAsync();
            return OperationResult<RefundResponse>.Success(MapRefund(refund), 201);
        }

        public async Task<OperationResult<RefundResponse>> GetRefundAsync(Guid refundId)
        {
            var refund = await _context.Refunds.AsNoTracking().FirstOrDefaultAsync(item => item.RefundID == refundId);
            return refund is null
                ? OperationResult<RefundResponse>.Failure("Refund not found.", 404)
                : OperationResult<RefundResponse>.Success(MapRefund(refund));
        }

        public async Task<OperationResult<PaymentReconciliationResponse>> GetPaymentReconciliationAsync(DateTime from, DateTime to, Guid? branchId)
        {
            if (from > to)
            {
                return OperationResult<PaymentReconciliationResponse>.Failure("From must be earlier than or equal to To.", 400);
            }

            var payments = _context.Payments.AsNoTracking()
                .Include(item => item.Booking)
                .Where(item => item.PaidAt.HasValue && item.PaidAt >= from && item.PaidAt <= to);
            if (branchId.HasValue)
            {
                payments = payments.Where(item => item.Booking.BranchID == branchId.Value);
            }

            var paidPaymentIds = payments.Where(item => item.PaymentStatus == PaymentStatusEnum.Paid).Select(item => item.PaymentID);
            var paidAmount = await payments.Where(item => item.PaymentStatus == PaymentStatusEnum.Paid)
                .SumAsync(item => (decimal?)item.Amount) ?? 0m;
            var count = await payments.CountAsync(item => item.PaymentStatus == PaymentStatusEnum.Paid);
            var refundedAmount = await _context.Refunds.AsNoTracking()
                .Where(item => item.Status == "Completed" && paidPaymentIds.Contains(item.PaymentID))
                .SumAsync(item => (decimal?)item.Amount) ?? 0m;

            return OperationResult<PaymentReconciliationResponse>.Success(new PaymentReconciliationResponse
            {
                From = from,
                To = to,
                BranchId = branchId,
                PaymentCount = count,
                PaidAmount = paidAmount,
                RefundedAmount = refundedAmount,
                NetAmount = paidAmount - refundedAmount
            });
        }

        private async Task<OperationResult<BookingResponse>> TransitionBookingAsync(Guid id, BookingStatusEnum expected, BookingStatusEnum next, string invalidMessage)
        {
            var booking = await LoadBookingAsync(id);
            if (booking is null)
            {
                return OperationResult<BookingResponse>.Failure("Booking not found.", 404);
            }

            if (booking.BookingStatus != expected)
            {
                return OperationResult<BookingResponse>.Failure(invalidMessage, 400);
            }

            booking.BookingStatus = next;
            booking.UpdatedAt = DateTime.UtcNow;
            if (next == BookingStatusEnum.InProgress)
            {
                booking.StartedAt ??= DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return OperationResult<BookingResponse>.Success(MapBooking(booking));
        }

        private async Task<Booking?> LoadBookingAsync(Guid id)
        {
            return await _context.Bookings
                .Include(item => item.BookingDetails)
                .ThenInclude(detail => detail.Service)
                .Include(item => item.TierSnapshot)
                .Include(item => item.StaffWorks)
                .ThenInclude(item => item.StaffUser)
                .FirstOrDefaultAsync(item => item.BookingID == id);
        }

        private async Task<WashBay?> FindAvailableWashBayAsync(Guid branchId, DateTime start, DateTime end, Guid? excludeBookingId = null)
        {
            var washBays = await _context.WashBays
                .Where(item => item.BranchID == branchId && item.Status == WashBayStatusEnum.Active)
                .OrderBy(item => item.BayName)
                .ToListAsync();

            foreach (var washBay in washBays)
            {
                var overlaps = await _context.Bookings.AnyAsync(booking =>
                    booking.WashBayID == washBay.WashBayID &&
                    ActiveBookingStatuses.Contains(booking.BookingStatus) &&
                    (!excludeBookingId.HasValue || booking.BookingID != excludeBookingId.Value) &&
                    booking.ScheduledStart < end &&
                    start < booking.ScheduledEnd);

                if (!overlaps)
                {
                    return washBay;
                }
            }

            return null;
        }

        private async Task<int> CountAvailableWashBaysAsync(Guid branchId, DateTime start, DateTime end)
        {
            var washBayIds = await _context.WashBays.AsNoTracking().Where(item => item.BranchID == branchId && item.Status == WashBayStatusEnum.Active).Select(item => item.WashBayID).ToListAsync();
            if (washBayIds.Count == 0) return 0;
            var occupied = await _context.Bookings.AsNoTracking().Where(booking => washBayIds.Contains(booking.WashBayID ?? Guid.Empty) && ActiveBookingStatuses.Contains(booking.BookingStatus) && booking.ScheduledStart < end && start < booking.ScheduledEnd).Select(booking => booking.WashBayID).Distinct().CountAsync();
            return washBayIds.Count - occupied;
        }

        private static string? ValidateServiceRequest(string name, string? description, decimal price, int durationMinutes)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Name is required.";
            }

            if (name.Trim().Length > 100)
            {
                return "Name must be at most 100 characters.";
            }

            if (price <= 0)
            {
                return "Price must be greater than 0.";
            }

            if (durationMinutes is < 10 or > 240)
            {
                return "DurationMinutes must be between 10 and 240.";
            }

            if (description?.Length > 500)
            {
                return "Description must be at most 500 characters.";
            }

            return null;
        }

        private static string? ValidateBranchRequest(string name, string address, TimeSpan openTime, TimeSpan closeTime)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Name is required.";
            }

            if (string.IsNullOrWhiteSpace(address))
            {
                return "Address is required.";
            }

            if (openTime >= closeTime)
            {
                return "OpenTime must be earlier than CloseTime.";
            }

            return null;
        }

        private async Task<string?> ValidateWashBayRequestAsync(Guid branchId, string name)
        {
            if (branchId == Guid.Empty)
            {
                return "BranchId is required.";
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return "Name is required.";
            }

            var branch = await _context.Branches.AsNoTracking().FirstOrDefaultAsync(item => item.BranchID == branchId);
            if (branch is null)
            {
                return "Branch not found.";
            }

            if (branch.Status != BranchStatusEnum.Open)
            {
                return "Cannot create wash bay for inactive branch.";
            }

            return null;
        }

        private static bool IsInsideOperatingHours(Branch branch, DateTime start, DateTime end)
        {
            if (!branch.OpenTime.HasValue || !branch.CloseTime.HasValue)
            {
                return true;
            }

            var localStart = BranchScheduleTime.ToLocal(start);
            var localEnd = BranchScheduleTime.ToLocal(end);
            return localStart.Date == localEnd.Date && localStart.TimeOfDay >= branch.OpenTime.Value && localEnd.TimeOfDay <= branch.CloseTime.Value;
        }

        private static ServiceResponse MapService(ServiceEntity service)
        {
            return new ServiceResponse
            {
                Id = service.ServiceID,
                Name = service.ServiceName,
                Description = service.Description,
                Price = service.Price,
                DurationMinutes = ToMinutes(service.EstimatedDuration),
                IsActive = service.Status == ServiceStatusEnum.Active
            };
        }

        private static BranchResponse MapBranch(Branch branch)
        {
            return new BranchResponse
            {
                Id = branch.BranchID,
                Name = branch.BranchName,
                Address = branch.Address,
                Phone = branch.PhoneNumber,
                OpenTime = branch.OpenTime,
                CloseTime = branch.CloseTime,
                IsActive = branch.Status == BranchStatusEnum.Open
            };
        }

        private static WashBayResponse MapWashBay(WashBay washBay)
        {
            return new WashBayResponse
            {
                Id = washBay.WashBayID,
                BranchId = washBay.BranchID,
                Name = washBay.BayName,
                IsActive = washBay.Status == WashBayStatusEnum.Active
            };
        }

        private static BookingListItemResponse MapBookingListItem(Booking booking)
        {
            var mapped = MapBooking(booking);
            return new BookingListItemResponse
            {
                Id = mapped.Id,
                CustomerId = mapped.CustomerId,
                VehicleId = mapped.VehicleId,
                BranchId = mapped.BranchId,
                ServiceId = mapped.ServiceId,
                WashBayId = mapped.WashBayId,
                AssignedStaffId = mapped.AssignedStaffId,
                AssignedStaffName = mapped.AssignedStaffName,
                StaffWork = mapped.StaffWork,
                Version = mapped.Version,
                BookingStartTime = mapped.BookingStartTime,
                BookingEndTime = mapped.BookingEndTime,
                Status = mapped.Status,
                TotalAmount = mapped.TotalAmount,
                ServiceNameSnapshot = mapped.ServiceNameSnapshot,
                DurationMinutesSnapshot = mapped.DurationMinutesSnapshot,
                PriceSnapshot = mapped.PriceSnapshot,
                TierSnapshot = mapped.TierSnapshot,
                CreatedAt = mapped.CreatedAt,
                CompletedAt = mapped.CompletedAt,
                CancelledAt = mapped.CancelledAt,
                CancellationReason = mapped.CancellationReason,
                Note = mapped.Note,
                Items = mapped.Items,
                CustomerName = booking.Customer.User.FullName,
                VehiclePlate = booking.Vehicle.LicensePlate,
                BranchName = booking.Branch.BranchName
            };
        }

        private static BookingDetailResponse MapBookingDetail(Booking booking)
        {
            var mapped = MapBooking(booking);
            return new BookingDetailResponse
            {
                Id = mapped.Id,
                CustomerId = mapped.CustomerId,
                VehicleId = mapped.VehicleId,
                BranchId = mapped.BranchId,
                ServiceId = mapped.ServiceId,
                WashBayId = mapped.WashBayId,
                AssignedStaffId = mapped.AssignedStaffId,
                StaffWork = mapped.StaffWork,
                Version = mapped.Version,
                BookingStartTime = mapped.BookingStartTime,
                BookingEndTime = mapped.BookingEndTime,
                Status = mapped.Status,
                TotalAmount = mapped.TotalAmount,
                ServiceNameSnapshot = mapped.ServiceNameSnapshot,
                DurationMinutesSnapshot = mapped.DurationMinutesSnapshot,
                PriceSnapshot = mapped.PriceSnapshot,
                TierSnapshot = mapped.TierSnapshot,
                CreatedAt = mapped.CreatedAt,
                CompletedAt = mapped.CompletedAt,
                CancelledAt = mapped.CancelledAt,
                CancellationReason = mapped.CancellationReason,
                Note = mapped.Note,
                Items = mapped.Items,
                BranchName = booking.Branch.BranchName,
                WashBayName = booking.WashBay?.BayName
            };
        }

        private static BookingResponse MapBooking(Booking booking)
        {
            var detail = booking.BookingDetails.FirstOrDefault();
            var items = booking.BookingDetails.Select(item => new BookingLineItemResponse
            {
                ServiceId = item.ServiceID,
                ServiceName = item.Service.ServiceName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.UnitPrice * item.Quantity,
                DurationMinutesPerUnit = ToMinutes(item.Service.EstimatedDuration)
            }).ToList();
            return new BookingResponse
            {
                Id = booking.BookingID,
                CustomerId = booking.CustomerID,
                VehicleId = booking.VehicleID,
                BranchId = booking.BranchID,
                ServiceId = detail?.ServiceID ?? Guid.Empty,
                WashBayId = booking.WashBayID,
                AssignedStaffId = booking.AssignedStaffID,
                AssignedStaffName = booking.AssignedStaff?.FullName,
                StaffWork = booking.StaffWorks.Select(item => new BookingStaffWorkResponse { StaffUserId = item.StaffUserID, StaffName = item.StaffUser?.FullName ?? string.Empty, ContributionPercent = item.ContributionPercent, WorkRole = item.WorkRole }).ToList(),
                Version = booking.Version,
                BookingStartTime = booking.ScheduledStart,
                BookingEndTime = booking.ScheduledEnd,
                Status = booking.BookingStatus.ToString(),
                TotalAmount = booking.EstimatedTotalAmount,
                ServiceNameSnapshot = detail?.Service.ServiceName ?? string.Empty,
                DurationMinutesSnapshot = ToMinutes(detail?.Service.EstimatedDuration),
                PriceSnapshot = detail?.UnitPrice ?? booking.EstimatedTotalAmount,
                TierSnapshot = booking.TierSnapshot?.TierName,
                CreatedAt = booking.CreatedAt,
                CompletedAt = booking.CompletedAt,
                CancelledAt = booking.CancelledAt,
                CancellationReason = booking.CancellationReason,
                Note = booking.Notes,
                Items = items
            };
        }

        private static PaymentResponse MapPayment(Payment payment)
        {
            return new PaymentResponse
            {
                Id = payment.PaymentID,
                BookingId = payment.BookingID,
                BranchId = payment.Booking?.BranchID ?? Guid.Empty,
                Amount = payment.Amount,
                Method = payment.PaymentMethod.ToString(),
                Status = payment.PaymentStatus.ToString(),
                CreatedAt = payment.RecordedAt,
                PaidAt = payment.PaidAt,
                ReferenceNumber = payment.ReferenceNumber,
                Note = payment.Notes
            };
        }

        private Task<bool> HasActiveBookingsAsync(Guid washBayId) =>
            _context.Bookings.AnyAsync(booking => booking.WashBayID == washBayId && ActiveBookingStatuses.Contains(booking.BookingStatus));

        private static DateTime BangkokNow()
        {
            var timeZoneId = OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Bangkok";
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
        }

        private static DateTime UtcDate(DateTime value) => DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);

        private static RefundResponse MapRefund(Refund refund)
        {
            return new RefundResponse
            {
                Id = refund.RefundID,
                PaymentId = refund.PaymentID,
                Amount = refund.Amount,
                Reason = refund.Reason,
                Status = refund.Status,
                ReferenceNumber = refund.ReferenceNumber,
                CreatedAt = refund.CreatedAt,
                CompletedAt = refund.CompletedAt
            };
        }

        private static int ToMinutes(TimeSpan? duration)
        {
            return duration.HasValue ? (int)Math.Round(duration.Value.TotalMinutes) : 0;
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static int NormalizePage(int page)
        {
            return page < 1 ? 1 : page;
        }

        private static int NormalizePageSize(int pageSize)
        {
            return pageSize switch
            {
                < 1 => 20,
                > 100 => 100,
                _ => pageSize
            };
        }

        private static bool TryParsePaymentMethod(string value, out PaymentMethodEnum method)
        {
            method = PaymentMethodEnum.Cash;
            if (string.Equals(value, "Cash", StringComparison.OrdinalIgnoreCase))
            {
                method = PaymentMethodEnum.Cash;
                return true;
            }

            if (string.Equals(value, "Card", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "CardAtCounter", StringComparison.OrdinalIgnoreCase))
            {
                method = PaymentMethodEnum.CardAtCounter;
                return true;
            }

            if (string.Equals(value, "BankTransfer", StringComparison.OrdinalIgnoreCase))
            {
                method = PaymentMethodEnum.BankTransfer;
                return true;
            }

            return false;
        }

        private static string? MergeNotes(string? existing, string? addition)
        {
            var normalizedAddition = NormalizeOptional(addition);
            if (normalizedAddition is null)
            {
                return existing;
            }

            return string.IsNullOrWhiteSpace(existing) ? normalizedAddition : $"{existing} | {normalizedAddition}";
        }
    }
}
