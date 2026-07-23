using DataAccessLayer.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations
{
    public class BookingPromotionConfiguration : IEntityTypeConfiguration<BookingPromotion>
    {
        public void Configure(EntityTypeBuilder<BookingPromotion> builder)
        {
            builder.HasKey(bp => bp.BookingPromotionID);
            builder.Property(bp => bp.DiscountAmount).HasPrecision(18, 2);
            builder.Property(bp => bp.AppliedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.HasIndex(bp => new { bp.BookingID, bp.PromotionID }).IsUnique();
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_BookingPromotions_Discount", "\"DiscountAmount\" >= 0");
                t.HasCheckConstraint("CK_BookingPromotions_BonusPoints", "\"BonusPoints\" >= 0");
            });
            builder.HasOne(bp => bp.Booking).WithMany(b => b.BookingPromotions).HasForeignKey(bp => bp.BookingID).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(bp => bp.Promotion).WithMany(p => p.BookingPromotions).HasForeignKey(bp => bp.PromotionID).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
