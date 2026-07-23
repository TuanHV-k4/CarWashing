using DataAccessLayer.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations
{
    public class CustomerTierHistoryConfiguration : IEntityTypeConfiguration<CustomerTierHistory>
    {
        public void Configure(EntityTypeBuilder<CustomerTierHistory> builder)
        {
            builder.HasKey(h => h.CustomerTierHistoryID);
            builder.Property(h => h.QualifiedSpent).HasPrecision(18, 2);
            builder.Property(h => h.ChangeReason).HasConversion<int>();
            builder.Property(h => h.ChangedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(h => h.Notes).HasMaxLength(1000);
            builder.HasIndex(h => new { h.CustomerID, h.ChangedAt });
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_CustomerTierHistories_Period", "\"ReviewPeriodEnd\" >= \"ReviewPeriodStart\"");
                t.HasCheckConstraint("CK_CustomerTierHistories_QualifiedValues", "\"QualifiedSpent\" >= 0 AND \"QualifiedVisits\" >= 0");
            });
            builder.HasOne(h => h.Customer).WithMany(c => c.TierHistories).HasForeignKey(h => h.CustomerID).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(h => h.PreviousTier).WithMany(t => t.TierHistoriesAsPrevious).HasForeignKey(h => h.PreviousTierID).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(h => h.NewTier).WithMany(t => t.TierHistoriesAsNew).HasForeignKey(h => h.NewTierID).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
