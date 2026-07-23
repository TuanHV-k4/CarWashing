using DataAccessLayer.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations
{
    public class RewardConfiguration : IEntityTypeConfiguration<Reward>
    {
        public void Configure(EntityTypeBuilder<Reward> builder)
        {
            builder.HasKey(r => r.RewardID);
            builder.Property(r => r.RewardName).IsRequired().HasMaxLength(200);
            builder.Property(r => r.Description).HasMaxLength(1000);
            builder.Property(r => r.RewardType).HasConversion<int>();
            builder.Property(r => r.RewardValue).HasPrecision(18, 2);
            builder.Property(r => r.Status).HasConversion<int>();
            builder.Property(r => r.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.HasIndex(r => r.RewardName).IsUnique();
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Rewards_PointsRequired", "\"PointsRequired\" > 0");
                t.HasCheckConstraint("CK_Rewards_Value", "\"RewardValue\" >= 0");
                t.HasCheckConstraint("CK_Rewards_Dates", "\"ValidFrom\" IS NULL OR \"ValidTo\" IS NULL OR \"ValidTo\" > \"ValidFrom\"");
                t.HasCheckConstraint("CK_Rewards_UsageLimit", "\"UsageLimitPerCustomer\" IS NULL OR \"UsageLimitPerCustomer\" > 0");
            });
            builder.HasOne(r => r.Service).WithMany(s => s.Rewards).HasForeignKey(r => r.ServiceID).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
