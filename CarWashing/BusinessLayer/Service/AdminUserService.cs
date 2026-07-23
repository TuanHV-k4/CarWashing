using BusinessLayer.Dtos.Admin;
using BusinessLayer.Dtos.Common;
using BusinessLayer.IService;
using DataAccessLayer.Context;
using DataAccessLayer.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service
{
    public class AdminUserService : IAdminUserService
    {
        private readonly ApplicationDbContext _context;

        public AdminUserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<UserSummaryDto>> GetUsersAsync(Guid actorId, string? query, string? role, UserStatusEnum? status, int page, int pageSize)
        {
            page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100);
            var users = _context.Users.Include(u => u.Role).AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(query)) { var text = query.Trim().ToLower(); users = users.Where(u => u.Username.ToLower().Contains(text) || u.Email.ToLower().Contains(text) || u.FullName.ToLower().Contains(text)); }
            if (!string.IsNullOrWhiteSpace(role)) users = users.Where(u => u.Role.RoleName == role);
            if (status.HasValue) users = users.Where(u => u.Status == status.Value);
            var total = await users.CountAsync();
            var selectedUsers = await users.OrderByDescending(u => u.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var items = selectedUsers.Select(user => Map(user, actorId)).ToList();
            return new PagedResult<UserSummaryDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
        }

        public async Task<UserSummaryDto> UpdateUserStatusAsync(Guid actorId, Guid userId, UpdateUserStatusRequestDto request)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserID == userId)
                ?? throw new KeyNotFoundException("User not found.");

            if (actorId == userId) throw new InvalidOperationException("You cannot change your own status.");
            if (!Enum.IsDefined(request.Status)) throw new InvalidOperationException("Invalid user status.");
            if (user.Status == UserStatusEnum.Deleted || request.Status == UserStatusEnum.Deleted)
                throw new InvalidOperationException("Deleted users are read-only.");
            if (user.Status == UserStatusEnum.Active && request.Status != UserStatusEnum.Active)
                await EnsureLastAdminProtected(user);
            user.Status = request.Status; user.AuthVersion++;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Map(user, actorId);
        }

        public async Task<UserSummaryDto> UpdateUserRoleAsync(Guid actorId, Guid userId, UpdateUserRoleRequestDto request)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserID == userId) ?? throw new KeyNotFoundException("User not found.");
            if (actorId == userId) throw new InvalidOperationException("You cannot change your own role.");
            if (user.Status != UserStatusEnum.Active) throw new InvalidOperationException("Only active users can change roles.");
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == request.Role) ?? throw new InvalidOperationException("Invalid role.");
            if (user.Role.RoleName == "Admin" && role.RoleName != "Admin" && user.Status == UserStatusEnum.Active)
                await EnsureLastAdminProtected(user);
            user.RoleID = role.RoleID; user.Role = role; user.AuthVersion++; await _context.SaveChangesAsync(); return Map(user, actorId);
        }

        private async Task EnsureLastAdminProtected(DataAccessLayer.Entity.User user)
        {
            if (user.Role.RoleName == "Admin" && await _context.Users.Include(x => x.Role)
                .CountAsync(x => x.Role.RoleName == "Admin" && x.Status == UserStatusEnum.Active) <= 1)
                throw new InvalidOperationException("The last active Admin cannot be changed.");
        }
        private static UserSummaryDto Map(DataAccessLayer.Entity.User user, Guid actorId) => new() { UserID=user.UserID, Username=user.Username, FullName=user.FullName, Email=user.Email, Role=user.Role.RoleName, Status=user.Status, EmailVerified=user.EmailVerified, CreatedAt=user.CreatedAt, IsCurrentUser=user.UserID==actorId };
    }
}
