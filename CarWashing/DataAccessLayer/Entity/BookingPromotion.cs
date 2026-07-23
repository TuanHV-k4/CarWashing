namespace DataAccessLayer.Entity
{
    public class BookingPromotion
    {
        public BookingPromotion()
        {
            BookingPromotionID = Guid.NewGuid();
            AppliedAt = DateTime.UtcNow;
        }

        public Guid BookingPromotionID { get; set; }
        public Guid BookingID { get; set; }
        public Guid PromotionID { get; set; }
        public decimal DiscountAmount { get; set; }
        public int BonusPoints { get; set; }
        public DateTime AppliedAt { get; set; }

        public virtual Booking Booking { get; set; } = null!;
        public virtual Promotion Promotion { get; set; } = null!;
    }
}
