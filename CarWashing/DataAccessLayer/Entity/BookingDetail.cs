using System;

namespace DataAccessLayer.Entity
{
    public class BookingDetail
    {
        public BookingDetail()
        {
            BookingDetailID = Guid.NewGuid();
        }

        public Guid BookingDetailID { get; set; }
        public Guid BookingID { get; set; }
        public Guid ServiceID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public virtual Booking Booking { get; set; } = null!;
        public virtual Service Service { get; set; } = null!;
    }
}
