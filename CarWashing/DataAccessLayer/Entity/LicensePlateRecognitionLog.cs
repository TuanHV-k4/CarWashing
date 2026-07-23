using DataAccessLayer.Enums;

namespace DataAccessLayer.Entity
{
    public class LicensePlateRecognitionLog
    {
        public LicensePlateRecognitionLog()
        {
            RecognitionID = Guid.NewGuid();
            DetectedAt = DateTime.UtcNow;
        }

        public Guid RecognitionID { get; set; }
        public Guid BranchID { get; set; }
        public Guid? MatchedVehicleID { get; set; }
        public string DetectedPlate { get; set; } = null!;
        public string NormalizedPlate { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public decimal ConfidenceScore { get; set; }
        public RecognitionReviewStatusEnum ReviewStatus { get; set; }
        public DateTime DetectedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public virtual Branch Branch { get; set; } = null!;
        public virtual Vehicle? MatchedVehicle { get; set; }
    }
}
