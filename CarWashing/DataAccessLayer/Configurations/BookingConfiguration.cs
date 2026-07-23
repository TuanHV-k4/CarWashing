using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccessLayer.Entity;

namespace DataAccessLayer.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.BookingID);
            builder.Property(b => b.ScheduledStart).IsRequired();
            builder.Property(b => b.ScheduledEnd).IsRequired();
            builder.Property(b => b.BookingStatus).HasConversion<int>();
            builder.Property(b => b.EstimatedTotalAmount).HasPrecision(18, 2);
            builder.Property(b => b.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(b => b.CancellationReason).HasMaxLength(500);
            builder.Property(b => b.Notes).HasMaxLength(1000);
            builder.Property(b => b.Version);
            builder.HasIndex(b => new { b.BranchID, b.ScheduledStart, b.BookingStatus });
            builder.HasIndex(b => new { b.WashBayID, b.ScheduledStart, b.ScheduledEnd });
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Bookings_Schedule", "\"ScheduledEnd\" > \"ScheduledStart\"");
                t.HasCheckConstraint("CK_Bookings_Amount", "\"EstimatedTotalAmount\" >= 0");
                t.HasCheckConstraint("CK_Bookings_QueuePriority", "\"QueuePriority\" >= 0");
            });
            builder.HasOne(b => b.Customer)
                   .WithMany(c => c.Bookings)
                   .HasForeignKey(b => b.CustomerID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Vehicle)
                   .WithMany(v => v.Bookings)
                   .HasForeignKey(b => b.VehicleID)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(b => b.Branch)
                   .WithMany(br => br.Bookings)
                   .HasForeignKey(b => b.BranchID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.WashBay)
                   .WithMany(w => w.Bookings)
                   .HasForeignKey(b => b.WashBayID)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(b => b.AssignedStaff)
                   .WithMany()
                   .HasForeignKey(b => b.AssignedStaffID)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(b => b.TierSnapshot)
                   .WithMany(t => t.BookingSnapshots)
                   .HasForeignKey(b => b.TierIDSnapshot)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
