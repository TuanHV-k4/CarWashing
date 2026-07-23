using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccessLayer.Entity;

namespace DataAccessLayer.Configurations
{
    public class PromotionCustomerConfiguration : IEntityTypeConfiguration<PromotionCustomer>
    {
        public void Configure(EntityTypeBuilder<PromotionCustomer> builder)
        {
            builder.HasKey(pc => pc.PromotionCustomerID);
            builder.HasIndex(pc => new { pc.PromotionID, pc.CustomerID }).IsUnique();
            builder.HasIndex(pc => new { pc.PromotionID, pc.SentAt });
            builder.Property(pc => pc.AudienceSnapshot).HasMaxLength(4000);
            builder.Property(pc => pc.EligibilitySnapshot).HasMaxLength(4000);
            builder.ToTable(t => t.HasCheckConstraint("CK_PromotionCustomers_UsageCount", "\"UsageCount\" >= 0"));
            builder.HasOne(pc => pc.Promotion).WithMany(p => p.PromotionCustomers).HasForeignKey(pc => pc.PromotionID).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(pc => pc.Customer).WithMany(c => c.PromotionCustomers).HasForeignKey(pc => pc.CustomerID).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
