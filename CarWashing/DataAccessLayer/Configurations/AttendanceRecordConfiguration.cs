using DataAccessLayer.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations;

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.HasKey(item => item.AttendanceRecordID);
        builder.HasIndex(item => item.ShiftAssignmentID).IsUnique().HasFilter("\"ShiftAssignmentID\" IS NOT NULL");
        builder.HasIndex(item => new { item.BranchStaffMembershipID, item.WorkDate }).IsUnique().HasFilter("\"BranchStaffMembershipID\" IS NOT NULL");
        builder.HasIndex(item => new { item.Status, item.CheckedInAt });
        builder.Property(item => item.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(item => item.CheckInSource).HasConversion<string>().HasMaxLength(32);
        builder.Property(item => item.CheckOutSource).HasConversion<string>().HasMaxLength(32);
        builder.Property(item => item.AdminNote).HasMaxLength(1000);
        builder.HasOne(item => item.ShiftAssignment).WithOne(item => item.AttendanceRecord).HasForeignKey<AttendanceRecord>(item => item.ShiftAssignmentID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.LockedByUser).WithMany().HasForeignKey(item => item.LockedByUserID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.Branch).WithMany().HasForeignKey(item => item.BranchID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.BranchStaffMembership).WithMany().HasForeignKey(item => item.BranchStaffMembershipID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.CheckedInByUser).WithMany().HasForeignKey(item => item.CheckedInByUserID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(item => item.CheckedOutByUser).WithMany().HasForeignKey(item => item.CheckedOutByUserID).OnDelete(DeleteBehavior.Restrict);
        builder.ToTable(table => table.HasCheckConstraint("CK_AttendanceRecords_WorkedMinutes", "\"WorkedMinutes\" >= 0 AND \"LateMinutes\" >= 0 AND \"EarlyLeaveMinutes\" >= 0 AND \"OvertimeMinutes\" >= 0"));
    }
}
