using DataAccessLayer.Enums;

namespace BusinessLayer.Dtos.Admin
{
    public class UpdateUserStatusRequestDto
    {
        public UserStatusEnum Status { get; set; }
    }

    public class UpdateUserRoleRequestDto { public string Role { get; set; } = string.Empty; }

    public class UserSummaryDto
    {
        public Guid UserID { get; set; }
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public UserStatusEnum Status { get; set; }
        public bool EmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsCurrentUser { get; set; }
    }

    public class BehavioralLogFilterDto
    {
        public Guid? CustomerID { get; set; }
        public BehavioralActionTypeEnum? ActionType { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class BehavioralLogItemDto
    {
        public Guid LogID { get; set; }
        public Guid? CustomerID { get; set; }
        public string? CustomerName { get; set; }
        public BehavioralActionTypeEnum ActionType { get; set; }
        public DateTime ActionTime { get; set; }
        public int PointsChanged { get; set; }
        public decimal SpendingAmount { get; set; }
        public string? Notes { get; set; }
    }
}
