using DataAccessLayer.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations
{
    public class PromotionServiceConfiguration : IEntityTypeConfiguration<PromotionService>
    {
        public void Configure(EntityTypeBuilder<PromotionService> builder)
        {
            builder.HasKey(ps => new { ps.PromotionID, ps.ServiceID });
            builder.HasOne(ps => ps.Promotion).WithMany(p => p.PromotionServices).HasForeignKey(ps => ps.PromotionID).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(ps => ps.Service).WithMany(s => s.PromotionServices).HasForeignKey(ps => ps.ServiceID).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
