using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccessLayer.Entity;

namespace DataAccessLayer.Configurations
{
    public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.HasKey(v => v.VehicleID);
            builder.Property(v => v.LicensePlate).IsRequired().HasMaxLength(50);
            builder.Property(v => v.VehicleType).HasConversion<string>().HasMaxLength(100);
            builder.Property(v => v.Brand).HasMaxLength(100);
            builder.Property(v => v.Model).HasMaxLength(100);
            builder.Property(v => v.Color).HasMaxLength(50);
            builder.Property(v => v.Status).HasConversion<int>();
            builder.Property(v => v.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.HasIndex(v => v.LicensePlate).IsUnique();
            builder.HasOne(v => v.Customer).WithMany(c => c.Vehicles).HasForeignKey(v => v.CustomerID).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
