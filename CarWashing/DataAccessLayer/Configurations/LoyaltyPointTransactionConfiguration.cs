using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccessLayer.Entity;

namespace DataAccessLayer.Configurations
{
    public class LoyaltyPointTransactionConfiguration : IEntityTypeConfiguration<LoyaltyPointTransaction>
    {
        public void Configure(EntityTypeBuilder<LoyaltyPointTransaction> builder)
        {
            builder.HasKey(t => t.TransactionID);
            builder.Property(t => t.Points).IsRequired();
            builder.Property(t => t.TransactionType).HasConversion<int>();
            builder.Property(t => t.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(t => t.IdempotencyKey).HasMaxLength(100);
            builder.Property(t => t.Description).HasMaxLength(1000);
            builder.HasIndex(t => t.IdempotencyKey).IsUnique().HasFilter("\"IdempotencyKey\" IS NOT NULL");
            builder.HasIndex(t => new { t.CustomerID, t.ExpiryDate, t.RemainingPoints });
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_PointTransactions_OriginalPoints", "\"OriginalPoints\" >= 0");
                t.HasCheckConstraint("CK_PointTransactions_RemainingPoints", "\"RemainingPoints\" >= 0 AND \"RemainingPoints\" <= \"OriginalPoints\"");
                t.HasCheckConstraint("CK_PointTransactions_BalanceAfter", "\"BalanceAfter\" >= 0");
            });
            builder.HasOne(t => t.Customer).WithMany(c => c.Transactions).HasForeignKey(t => t.CustomerID).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(t => t.Booking).WithMany(b => b.PointTransactions).HasForeignKey(t => t.BookingID).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(t => t.WashHistory).WithMany(w => w.PointTransactions).HasForeignKey(t => t.WashHistoryID).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(t => t.Redemption).WithMany(r => r.PointTransactions).HasForeignKey(t => t.RedemptionID).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(t => t.ReferenceTransaction).WithMany().HasForeignKey(t => t.ReferenceTransactionID).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
