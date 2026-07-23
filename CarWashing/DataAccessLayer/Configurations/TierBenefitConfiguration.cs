using DataAccessLayer.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations
{
    public class TierBenefitConfiguration : IEntityTypeConfiguration<TierBenefit>
    {
        public void Configure(EntityTypeBuilder<TierBenefit> builder)
        {
            builder.HasKey(b => b.TierBenefitID);
            builder.Property(b => b.BenefitName).IsRequired().HasMaxLength(200);
            builder.Property(b => b.BenefitType).HasConversion<int>();
            builder.Property(b => b.BenefitValue).HasPrecision(18, 2);
            builder.HasIndex(b => new { b.TierID, b.BenefitName }).IsUnique();
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_TierBenefits_Value", "\"BenefitValue\" >= 0");
                t.HasCheckConstraint("CK_TierBenefits_MonthlyLimit", "\"MonthlyLimit\" IS NULL OR \"MonthlyLimit\" > 0");
            });
            builder.HasOne(b => b.Tier).WithMany(t => t.Benefits).HasForeignKey(b => b.TierID).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(b => b.Service).WithMany(s => s.TierBenefits).HasForeignKey(b => b.ServiceID).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
