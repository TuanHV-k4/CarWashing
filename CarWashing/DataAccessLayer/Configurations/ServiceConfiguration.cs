using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccessLayer.Entity;

namespace DataAccessLayer.Configurations
{
    public class ServiceConfiguration : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            builder.HasKey(s => s.ServiceID);
            builder.Property(s => s.ServiceName).IsRequired().HasMaxLength(200);
            builder.Property(s => s.Description).HasMaxLength(1000);
            builder.Property(s => s.Price).HasPrecision(18, 2);
            builder.Property(s => s.Status).HasConversion<int>();
            builder.HasIndex(s => s.ServiceName).IsUnique();
            builder.ToTable(t => t.HasCheckConstraint("CK_Services_Price", "\"Price\" >= 0"));
        }
    }
}
