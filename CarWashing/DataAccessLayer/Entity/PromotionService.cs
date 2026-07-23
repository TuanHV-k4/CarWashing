namespace DataAccessLayer.Entity
{
    public class PromotionService
    {
        public Guid PromotionID { get; set; }
        public Guid ServiceID { get; set; }

        public virtual Promotion Promotion { get; set; } = null!;
        public virtual Service Service { get; set; } = null!;
    }
}
