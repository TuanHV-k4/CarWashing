using BusinessLayer.Dtos.Admin;
using BusinessLayer.Dtos.Common;
using DataAccessLayer.Enums;

namespace BusinessLayer.IService
{
    public interface IAdminUserService
    {
        Task<PagedResult<UserSummaryDto>> GetUsersAsync(Guid actorId, string? query, string? role, UserStatusEnum? status, int page, int pageSize);
        Task<UserSummaryDto> UpdateUserStatusAsync(Guid actorId, Guid userId, UpdateUserStatusRequestDto request);
        Task<UserSummaryDto> UpdateUserRoleAsync(Guid actorId, Guid userId, UpdateUserRoleRequestDto request);
    }
}
