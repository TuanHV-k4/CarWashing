using DataAccessLayer.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations;

public class BookingStaffWorkConfiguration : IEntityTypeConfiguration<BookingStaffWork>
{
    public void Configure(EntityTypeBuilder<BookingStaffWork> builder)
    {
        builder.HasKey(item => item.BookingStaffWorkID);
        builder.HasIndex(item => new { item.BookingID, item.StaffUserID }).IsUnique();
        builder.HasIndex(item => new { item.StaffUserID, item.BookingID });
        builder.Property(item => item.ContributionPercent).HasPrecision(5, 2);
        builder.Property(item => item.WorkRole).HasMaxLength(100);
        builder.Property(item => item.AdjustmentReason).HasMaxLength(500);
        builder.HasOne(item => item.Booking).WithMany(item => item.StaffWorks).HasForeignKey(item => item.BookingID).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.StaffUser).WithMany().HasForeignKey(item => item.StaffUserID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.AssignedByUser).WithMany().HasForeignKey(item => item.AssignedByUserID).OnDelete(DeleteBehavior.Restrict);
        builder.ToTable(table => table.HasCheckConstraint("CK_BookingStaffWorks_Contribution", "\"ContributionPercent\" > 0 AND \"ContributionPercent\" <= 100"));
    }
}
