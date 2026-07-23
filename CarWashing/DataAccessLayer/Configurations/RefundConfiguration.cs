using DataAccessLayer.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace DataAccessLayer.Configurations;
public class RefundConfiguration : IEntityTypeConfiguration<Refund>
{ public void Configure(EntityTypeBuilder<Refund> builder) { builder.HasKey(x => x.RefundID); builder.Property(x => x.Amount).HasPrecision(18, 2); builder.Property(x => x.Reason).HasMaxLength(500); builder.Property(x => x.Status).HasMaxLength(32); builder.Property(x => x.ReferenceNumber).HasMaxLength(100); builder.HasIndex(x => new { x.PaymentID, x.Status }); builder.HasOne(x => x.Payment).WithMany().HasForeignKey(x => x.PaymentID).OnDelete(DeleteBehavior.Restrict); builder.ToTable(t => t.HasCheckConstraint("CK_Refunds_Amount", "\"Amount\" > 0")); } }
