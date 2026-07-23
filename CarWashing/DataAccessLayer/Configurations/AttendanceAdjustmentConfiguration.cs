using DataAccessLayer.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations;

public class AttendanceAdjustmentConfiguration : IEntityTypeConfiguration<AttendanceAdjustment>
{
    public void Configure(EntityTypeBuilder<AttendanceAdjustment> builder)
    {
        builder.HasKey(item => item.AttendanceAdjustmentID);
        builder.HasIndex(item => new { item.AttendanceRecordID, item.AdjustedAt });
        builder.Property(item => item.Action).HasConversion<string>().HasMaxLength(32);
        builder.Property(item => item.PreviousValues).HasColumnType("jsonb");
        builder.Property(item => item.NewValues).HasColumnType("jsonb");
        builder.Property(item => item.Reason).HasMaxLength(500).IsRequired();
        builder.HasOne(item => item.AttendanceRecord).WithMany(item => item.Adjustments).HasForeignKey(item => item.AttendanceRecordID).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.AdjustedByUser).WithMany().HasForeignKey(item => item.AdjustedByUserID).OnDelete(DeleteBehavior.Restrict);
    }
}
