using System;
using System.Collections.Generic;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Entity
{
    public class Vehicle
    {
        public Vehicle()
        {
            VehicleID = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            Bookings = new HashSet<Booking>();
            RecognitionLogs = new HashSet<LicensePlateRecognitionLog>();
        }

        public Guid VehicleID { get; set; }
        public Guid CustomerID { get; set; }
        public string LicensePlate { get; set; } = null!;
        public VehicleTypeEnum? VehicleType { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Color { get; set; }
        public VehicleStatusEnum Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Customer Customer { get; set; } = null!;
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<LicensePlateRecognitionLog> RecognitionLogs { get; set; }
    }
}
