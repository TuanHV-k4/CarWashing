using DataAccessLayer.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Configurations
{
    public class LicensePlateRecognitionLogConfiguration : IEntityTypeConfiguration<LicensePlateRecognitionLog>
    {
        public void Configure(EntityTypeBuilder<LicensePlateRecognitionLog> builder)
        {
            builder.HasKey(l => l.RecognitionID);
            builder.Property(l => l.DetectedPlate).IsRequired().HasMaxLength(50);
            builder.Property(l => l.NormalizedPlate).IsRequired().HasMaxLength(50);
            builder.Property(l => l.ImageUrl).HasMaxLength(1000);
            builder.Property(l => l.ConfidenceScore).HasPrecision(5, 4);
            builder.Property(l => l.ReviewStatus).HasConversion<int>();
            builder.Property(l => l.DetectedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.HasIndex(l => new { l.NormalizedPlate, l.DetectedAt });
            builder.ToTable(t => t.HasCheckConstraint("CK_LprLogs_Confidence", "\"ConfidenceScore\" >= 0 AND \"ConfidenceScore\" <= 1"));
            builder.HasOne(l => l.Branch).WithMany(b => b.RecognitionLogs).HasForeignKey(l => l.BranchID).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(l => l.MatchedVehicle).WithMany(v => v.RecognitionLogs).HasForeignKey(l => l.MatchedVehicleID).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
