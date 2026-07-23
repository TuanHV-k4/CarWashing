using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccessLayer.Entity;

namespace DataAccessLayer.Configurations
{
    public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
    {
        public void Configure(EntityTypeBuilder<Promotion> builder)
        {
            builder.HasKey(p => p.PromotionID);
            builder.Property(p => p.PromotionName).IsRequired().HasMaxLength(200);
            builder.Property(p => p.PromotionCode).HasMaxLength(50);
            builder.Property(p => p.Description).HasMaxLength(1000);
            builder.Property(p => p.PromotionType).HasConversion<int>();
            builder.Property(p => p.PromotionValue).HasPrecision(18, 2);
            builder.Property(p => p.MaxDiscountAmount).HasPrecision(18, 2);
            builder.Property(p => p.MinimumSpend).HasPrecision(18, 2);
            builder.Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.HasIndex(p => p.PromotionCode).IsUnique().HasFilter("\"PromotionCode\" IS NOT NULL");
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Promotions_Dates", "\"EndDate\" > \"StartDate\"");
                t.HasCheckConstraint("CK_Promotions_Value", "\"PromotionValue\" >= 0");
                t.HasCheckConstraint("CK_Promotions_MinimumSpend", "\"MinimumSpend\" >= 0");
                t.HasCheckConstraint("CK_Promotions_UsageLimits", "(\"TotalUsageLimit\" IS NULL OR \"TotalUsageLimit\" > 0) AND (\"UsageLimitPerCustomer\" IS NULL OR \"UsageLimitPerCustomer\" > 0)");
            });
            builder.HasOne(p => p.MinTier).WithMany(t => t.Promotions).HasForeignKey(p => p.MinTierID).OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(p => p.FreeService).WithMany(s => s.FreeServicePromotions).HasForeignKey(p => p.FreeServiceID).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
