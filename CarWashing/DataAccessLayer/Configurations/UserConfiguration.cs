using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccessLayer.Entity;

namespace DataAccessLayer.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.UserID);
            builder.Property(u => u.Username).IsRequired().HasMaxLength(100);
            builder.Property(u => u.PasswordHash).IsRequired();
            builder.Property(u => u.FullName).HasMaxLength(200);
            builder.Property(u => u.Email).HasMaxLength(200);
            builder.Property(u => u.PhoneNumber).HasMaxLength(50);
            builder.Property(u => u.Status).HasConversion<int>();
            builder.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.HasIndex(u => u.Username).IsUnique();
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.PhoneNumber).IsUnique().HasFilter("\"PhoneNumber\" IS NOT NULL");
            builder.Property(u => u.EmailVerified).HasDefaultValue(false);
            builder.Property(u => u.EmailVerificationToken).HasMaxLength(100);
            builder.Property(u => u.PasswordResetTokenHash).HasMaxLength(64);
            builder.HasIndex(u => u.PasswordResetTokenHash);
            builder.Property(u => u.AuthVersion).HasDefaultValue(0);
            builder.HasOne(u => u.Customer).WithOne(c => c.User).HasForeignKey<Customer>(c => c.UserID).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
