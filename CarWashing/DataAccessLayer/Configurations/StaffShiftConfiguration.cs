using DataAccessLayer.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DataAccessLayer.Configurations;
public class StaffShiftConfiguration : IEntityTypeConfiguration<StaffShift>
{ public void Configure(EntityTypeBuilder<StaffShift> builder) { builder.HasKey(x => x.StaffShiftID); builder.Property(x => x.Name).HasMaxLength(120); builder.HasIndex(x => new { x.BranchID, x.StartsAt, x.EndsAt }); builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchID).OnDelete(DeleteBehavior.Restrict); builder.ToTable(t => t.HasCheckConstraint("CK_StaffShifts_Time", "\"EndsAt\" > \"StartsAt\"")); } }
