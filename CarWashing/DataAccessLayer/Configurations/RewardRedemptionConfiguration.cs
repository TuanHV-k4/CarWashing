using DataAccessLayer.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations
{
    public class RewardRedemptionConfiguration : IEntityTypeConfiguration<RewardRedemption>
    {
        public void Configure(EntityTypeBuilder<RewardRedemption> builder)
        {
            builder.HasKey(r => r.RedemptionID);
            builder.Property(r => r.Status).HasConversion<int>();
            builder.Property(r => r.RedeemedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(r => r.AppliedDiscountAmount).HasPrecision(18, 2);
            builder.HasIndex(r => new { r.CustomerID, r.RedeemedAt });
            builder.ToTable(t => t.HasCheckConstraint("CK_RewardRedemptions_Points", "\"PointsSpent\" > 0"));
            builder.HasOne(r => r.Customer).WithMany(c => c.RewardRedemptions).HasForeignKey(r => r.CustomerID).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(r => r.Reward).WithMany(r => r.Redemptions).HasForeignKey(r => r.RewardID).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(r => r.Booking).WithMany(b => b.RewardRedemptions).HasForeignKey(r => r.BookingID).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
