using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccessLayer.Entity;

namespace DataAccessLayer.Configurations
{
    public class BehavioralLogConfiguration : IEntityTypeConfiguration<BehavioralLog>
    {
        public void Configure(EntityTypeBuilder<BehavioralLog> builder)
        {
            builder.HasKey(b => b.LogID);
            builder.Property(b => b.ActionType).HasConversion<int>();
            builder.Property(b => b.ActionTime).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(b => b.PointsChanged).HasDefaultValue(0);
            builder.Property(b => b.SpendingAmount).HasPrecision(18, 2).HasDefaultValue(0);
            builder.Property(b => b.RewardUsed).HasPrecision(18, 2).HasDefaultValue(0);
            builder.Property(b => b.SessionID).HasMaxLength(100);
            builder.Property(b => b.MetadataJson).HasMaxLength(4000);
            builder.Property(b => b.Notes).HasMaxLength(1000);
            builder.HasOne(b => b.Customer)
                    .WithMany(c => c.BehavioralLogs)
                    .HasForeignKey(b => b.CustomerID)
                    .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(b => b.Booking)
                   .WithMany(bk => bk.BehavioralLogs)
                   .HasForeignKey(b => b.BookingID)
                   .OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(b => b.Service).WithMany(s => s.BehavioralLogs).HasForeignKey(b => b.ServiceID).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(b => b.Promotion).WithMany(p => p.BehavioralLogs).HasForeignKey(b => b.PromotionID).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
