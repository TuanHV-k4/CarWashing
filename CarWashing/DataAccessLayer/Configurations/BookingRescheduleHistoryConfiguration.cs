using DataAccessLayer.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations;

public class BookingRescheduleHistoryConfiguration : IEntityTypeConfiguration<BookingRescheduleHistory>
{
    public void Configure(EntityTypeBuilder<BookingRescheduleHistory> builder)
    {
        builder.HasKey(item => item.BookingRescheduleHistoryID);
        builder.Property(item => item.Note).HasMaxLength(1000);
        builder.HasIndex(item => new { item.BookingID, item.ChangedAt });
        builder.HasOne(item => item.Booking).WithMany(booking => booking.RescheduleHistory).HasForeignKey(item => item.BookingID).OnDelete(DeleteBehavior.Cascade);
        builder.ToTable(table => table.HasCheckConstraint("CK_BookingRescheduleHistory_Schedule", "\"PreviousEnd\" > \"PreviousStart\" AND \"NewEnd\" > \"NewStart\""));
    }
}
