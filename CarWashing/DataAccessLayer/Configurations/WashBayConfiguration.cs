using DataAccessLayer.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations
{
    public class WashBayConfiguration : IEntityTypeConfiguration<WashBay>
    {
        public void Configure(EntityTypeBuilder<WashBay> builder)
        {
            builder.HasKey(w => w.WashBayID);
            builder.Property(w => w.BayName).IsRequired().HasMaxLength(100);
            builder.Property(w => w.Status).HasConversion<int>();
            builder.HasIndex(w => new { w.BranchID, w.BayName }).IsUnique();
            builder.HasOne(w => w.Branch).WithMany(b => b.WashBays).HasForeignKey(w => w.BranchID).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
