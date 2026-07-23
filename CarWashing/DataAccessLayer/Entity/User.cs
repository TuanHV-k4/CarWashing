using System;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Entity
{
    public class User
    {
        public User()
        {
            UserID = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        public Guid UserID { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public Guid RoleID { get; set; }
        public UserStatusEnum Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool EmailVerified { get; set; }
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiry { get; set; }
        public string? PasswordResetTokenHash { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public int AuthVersion { get; set; }

        public virtual Role Role { get; set; } = null!;
        public virtual Customer? Customer { get; set; }
    }
}
