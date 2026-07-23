using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccessLayer.Entity;

namespace DataAccessLayer.Configurations
{
    public class WashHistoryConfiguration : IEntityTypeConfiguration<WashHistory>
    {
        public void Configure(EntityTypeBuilder<WashHistory> builder)
        {
            builder.HasKey(w => w.WashHistoryID);
            builder.Property(w => w.WashDate).IsRequired();
            builder.Property(w => w.ActualTotalAmount).HasPrecision(18, 2);
            builder.Property(w => w.DiscountAmount).HasPrecision(18, 2);
            builder.Property(w => w.FinalAmount).HasPrecision(18, 2);
            builder.Property(w => w.RewardUsed).HasPrecision(18,2);
            builder.Property(w => w.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(w => w.Feedback).HasMaxLength(2000);
            builder.HasIndex(w => w.BookingID).IsUnique();
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_WashHistories_Amounts", "\"ActualTotalAmount\" >= 0 AND \"DiscountAmount\" >= 0 AND \"FinalAmount\" >= 0");
                t.HasCheckConstraint("CK_WashHistories_Points", "\"PointsEarned\" >= 0");
                t.HasCheckConstraint("CK_WashHistories_Rating", "\"CustomerRating\" IS NULL OR (\"CustomerRating\" >= 1 AND \"CustomerRating\" <= 5)");
            });
            builder.HasOne(w => w.Booking).WithOne(b => b.WashHistory).HasForeignKey<WashHistory>(w => w.BookingID).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
