using DataAccessLayer.Enums;

namespace DataAccessLayer.Entity
{
    public class WashBay
    {
        public WashBay()
        {
            WashBayID = Guid.NewGuid();
            Bookings = new HashSet<Booking>();
        }

        public Guid WashBayID { get; set; }
        public Guid BranchID { get; set; }
        public string BayName { get; set; } = null!;
        public WashBayStatusEnum Status { get; set; }

        public virtual Branch Branch { get; set; } = null!;
        public virtual ICollection<Booking> Bookings { get; set; }
    }
}
