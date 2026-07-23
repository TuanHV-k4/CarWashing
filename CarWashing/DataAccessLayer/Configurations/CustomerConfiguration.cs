using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccessLayer.Entity;

namespace DataAccessLayer.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(c => c.CustomerID);
            builder.Property(c => c.CurrentPoints).HasDefaultValue(0);
            builder.Property(c => c.LifetimePoints).HasDefaultValue(0);
            builder.Property(c => c.TotalSpent).HasPrecision(18, 2).HasDefaultValue(0);
            builder.Property(c => c.TotalVisits).HasDefaultValue(0);
            builder.Property(c => c.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(c => c.Version).IsConcurrencyToken();
            builder.HasIndex(c => c.UserID).IsUnique();
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Customers_CurrentPoints", "\"CurrentPoints\" >= 0");
                t.HasCheckConstraint("CK_Customers_LifetimePoints", "\"LifetimePoints\" >= 0");
                t.HasCheckConstraint("CK_Customers_TotalSpent", "\"TotalSpent\" >= 0");
                t.HasCheckConstraint("CK_Customers_TotalVisits", "\"TotalVisits\" >= 0");
            });
            builder.HasOne(c => c.Tier).WithMany(t => t.Customers).HasForeignKey(c => c.TierID).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
