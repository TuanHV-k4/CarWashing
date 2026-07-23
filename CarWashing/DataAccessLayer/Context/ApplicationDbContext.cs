using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Entity;

namespace DataAccessLayer.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Branch> Branches { get; set; } = null!;
        public DbSet<LoyaltyTier> LoyaltyTiers { get; set; } = null!;
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<Service> Services { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<BookingDetail> BookingDetails { get; set; } = null!;
        public DbSet<WashHistory> WashHistories { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<LoyaltyPointTransaction> LoyaltyPointTransactions { get; set; } = null!;
        public DbSet<Promotion> Promotions { get; set; } = null!;
        public DbSet<PromotionCustomer> PromotionCustomers { get; set; } = null!;
        public DbSet<BehavioralLog> BehavioralLogs { get; set; } = null!;
        public DbSet<TierBenefit> TierBenefits { get; set; } = null!;
        public DbSet<CustomerTierHistory> CustomerTierHistories { get; set; } = null!;
        public DbSet<WashBay> WashBays { get; set; } = null!;
        public DbSet<Reward> Rewards { get; set; } = null!;
        public DbSet<RewardRedemption> RewardRedemptions { get; set; } = null!;
        public DbSet<PromotionService> PromotionServices { get; set; } = null!;
        public DbSet<BookingPromotion> BookingPromotions { get; set; } = null!;
        public DbSet<LicensePlateRecognitionLog> LicensePlateRecognitionLogs { get; set; } = null!;
        public DbSet<StaffShift> StaffShifts { get; set; } = null!;
        public DbSet<ShiftAssignment> ShiftAssignments { get; set; } = null!;
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; } = null!;
        public DbSet<AttendanceAdjustment> AttendanceAdjustments { get; set; } = null!;
        public DbSet<BranchStaffMembership> BranchStaffMemberships { get; set; } = null!;
        public DbSet<BranchManagerMembership> BranchManagerMemberships { get; set; } = null!;
        public DbSet<BranchWorkSchedule> BranchWorkSchedules { get; set; } = null!;
        public DbSet<Refund> Refunds { get; set; } = null!;
        public DbSet<BookingRescheduleHistory> BookingRescheduleHistories { get; set; } = null!;
        public DbSet<BookingStaffWork> BookingStaffWorks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            NormalizeDateTimesToUtc();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            NormalizeDateTimesToUtc();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void NormalizeDateTimesToUtc()
        {
            foreach (var entry in ChangeTracker.Entries().Where(entry => entry.State is EntityState.Added or EntityState.Modified))
            {
                foreach (var property in entry.Properties.Where(property => property.CurrentValue is DateTime))
                {
                    var value = (DateTime)property.CurrentValue!;
                    property.CurrentValue = value.Kind == DateTimeKind.Utc
                        ? value
                        : value.Kind == DateTimeKind.Local
                            ? value.ToUniversalTime()
                            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
                }
            }
        }
    }
}
