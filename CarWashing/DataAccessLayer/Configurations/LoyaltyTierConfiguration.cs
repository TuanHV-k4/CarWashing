using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccessLayer.Entity;

namespace DataAccessLayer.Configurations
{
    public class LoyaltyTierConfiguration : IEntityTypeConfiguration<LoyaltyTier>
    {
        public void Configure(EntityTypeBuilder<LoyaltyTier> builder)
        {
            builder.HasKey(t => t.TierID);
            builder.Property(t => t.TierName).IsRequired().HasMaxLength(200);
            builder.Property(t => t.MinSpent).HasPrecision(18, 2);
            builder.Property(t => t.PointMultiplier).HasPrecision(5, 2);
            builder.Property(t => t.TierBenefits).HasMaxLength(1000);
            builder.Property(t => t.Status).HasConversion<int>();
            builder.Property(t => t.QualificationMode).HasConversion<int>();
            builder.Property(t => t.QualificationPeriodMonths).HasDefaultValue(12);
            builder.HasIndex(t => t.TierName).IsUnique();
            builder.HasIndex(t => t.TierRank).IsUnique();
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_LoyaltyTiers_MinSpent", "\"MinSpent\" >= 0");
                t.HasCheckConstraint("CK_LoyaltyTiers_MinVisits", "\"MinVisits\" >= 0");
                t.HasCheckConstraint("CK_LoyaltyTiers_BookingWindow", "\"BookingWindowDays\" > 0");
                t.HasCheckConstraint("CK_LoyaltyTiers_PointMultiplier", "\"PointMultiplier\" > 0");
                t.HasCheckConstraint("CK_LoyaltyTiers_QualificationPeriod", "\"QualificationPeriodMonths\" > 0");
            });
        }
    }
}
