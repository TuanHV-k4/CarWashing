using System;
using System.Collections.Generic;
using DataAccessLayer.Enums;

namespace DataAccessLayer.Entity
{
    public class Booking
    {
        public Booking()
        {
            BookingID = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            BookingDetails = new HashSet<BookingDetail>();
            Payments = new HashSet<Payment>();
            BehavioralLogs = new HashSet<BehavioralLog>();
            BookingPromotions = new HashSet<BookingPromotion>();
            RewardRedemptions = new HashSet<RewardRedemption>();
            PointTransactions = new HashSet<LoyaltyPointTransaction>();
            RescheduleHistory = new HashSet<BookingRescheduleHistory>();
            StaffWorks = new HashSet<BookingStaffWork>();
        }

        public Guid BookingID { get; set; }
        public Guid CustomerID { get; set; }
        public Guid VehicleID { get; set; }
        public Guid BranchID { get; set; }
        public Guid? WashBayID { get; set; }
        public Guid? AssignedStaffID { get; set; }
        public Guid? TierIDSnapshot { get; set; }
        public DateTime ScheduledStart { get; set; }
        public DateTime ScheduledEnd { get; set; }
        public BookingStatusEnum BookingStatus { get; set; }
        public int QueuePriority { get; set; }
        public decimal EstimatedTotalAmount { get; set; }
        public string? Notes { get; set; }
        public DateTime? CheckInAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public uint Version { get; set; }

        public virtual Customer Customer { get; set; } = null!;
        public virtual Vehicle Vehicle { get; set; } = null!;
        public virtual Branch Branch { get; set; } = null!;
        public virtual WashBay? WashBay { get; set; }
        public virtual User? AssignedStaff { get; set; }
        public virtual LoyaltyTier? TierSnapshot { get; set; }
        public virtual ICollection<BookingDetail> BookingDetails { get; set; }
        public virtual WashHistory? WashHistory { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<BehavioralLog> BehavioralLogs { get; set; }
        public virtual ICollection<BookingPromotion> BookingPromotions { get; set; }
        public virtual ICollection<RewardRedemption> RewardRedemptions { get; set; }
        public virtual ICollection<LoyaltyPointTransaction> PointTransactions { get; set; }
        public virtual ICollection<BookingRescheduleHistory> RescheduleHistory { get; set; }
        public virtual ICollection<BookingStaffWork> StaffWorks { get; set; }
    }
}
