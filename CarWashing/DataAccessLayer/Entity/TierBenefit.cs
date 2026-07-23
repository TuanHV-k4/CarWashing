using DataAccessLayer.Enums;

namespace DataAccessLayer.Entity
{
    public class TierBenefit
    {
        public TierBenefit()
        {
            TierBenefitID = Guid.NewGuid();
        }

        public Guid TierBenefitID { get; set; }
        public Guid TierID { get; set; }
        public Guid? ServiceID { get; set; }
        public string BenefitName { get; set; } = null!;
        public TierBenefitTypeEnum BenefitType { get; set; }
        public decimal BenefitValue { get; set; }
        public int? MonthlyLimit { get; set; }
        public bool IsAutoApplied { get; set; }
        public bool IsActive { get; set; }

        public virtual LoyaltyTier Tier { get; set; } = null!;
        public virtual Service? Service { get; set; }
    }
}
