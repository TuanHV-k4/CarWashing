using DataAccessLayer.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DataAccessLayer.Configurations;
public class ShiftAssignmentConfiguration : IEntityTypeConfiguration<ShiftAssignment>
{ public void Configure(EntityTypeBuilder<ShiftAssignment> builder) { builder.HasKey(x => x.ShiftAssignmentID); builder.HasIndex(x => new { x.StaffShiftID, x.UserID }).IsUnique(); builder.HasOne(x => x.StaffShift).WithMany(x => x.Assignments).HasForeignKey(x => x.StaffShiftID).OnDelete(DeleteBehavior.Cascade); builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserID).OnDelete(DeleteBehavior.Restrict); builder.HasOne(x => x.WashBay).WithMany().HasForeignKey(x => x.WashBayID).OnDelete(DeleteBehavior.SetNull); } }
