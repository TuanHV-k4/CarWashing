using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    BranchID = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OpenTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    CloseTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.BranchID);
                    table.CheckConstraint("CK_Branches_OperatingHours", "\"OpenTime\" IS NULL OR \"CloseTime\" IS NULL OR \"OpenTime\" < \"CloseTime\"");
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyTiers",
                columns: table => new
                {
                    TierID = table.Column<Guid>(type: "uuid", nullable: false),
                    TierName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TierRank = table.Column<int>(type: "integer", nullable: false),
                    MinSpent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MinVisits = table.Column<int>(type: "integer", nullable: false),
                    QualificationPeriodMonths = table.Column<int>(type: "integer", nullable: false, defaultValue: 12),
                    QualificationMode = table.Column<int>(type: "integer", nullable: false),
                    BookingWindowDays = table.Column<int>(type: "integer", nullable: false),
                    PriorityLevel = table.Column<int>(type: "integer", nullable: false),
                    PointMultiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    TierBenefits = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyTiers", x => x.TierID);
                    table.CheckConstraint("CK_LoyaltyTiers_BookingWindow", "\"BookingWindowDays\" > 0");
                    table.CheckConstraint("CK_LoyaltyTiers_MinSpent", "\"MinSpent\" >= 0");
                    table.CheckConstraint("CK_LoyaltyTiers_MinVisits", "\"MinVisits\" >= 0");
                    table.CheckConstraint("CK_LoyaltyTiers_PointMultiplier", "\"PointMultiplier\" > 0");
                    table.CheckConstraint("CK_LoyaltyTiers_QualificationPeriod", "\"QualificationPeriodMonths\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleID = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    ServiceID = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EstimatedDuration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.ServiceID);
                    table.CheckConstraint("CK_Services_Price", "\"Price\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "WashBays",
                columns: table => new
                {
                    WashBayID = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchID = table.Column<Guid>(type: "uuid", nullable: false),
                    BayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WashBays", x => x.WashBayID);
                    table.ForeignKey(
                        name: "FK_WashBays_Branches_BranchID",
                        column: x => x.BranchID,
                        principalTable: "Branches",
                        principalColumn: "BranchID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RoleID = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    EmailVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EmailVerificationToken = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EmailVerificationTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    PromotionID = table.Column<Guid>(type: "uuid", nullable: false),
                    PromotionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PromotionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PromotionType = table.Column<int>(type: "integer", nullable: false),
                    PromotionValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxDiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    BonusPoints = table.Column<int>(type: "integer", nullable: false),
                    FreeServiceID = table.Column<Guid>(type: "uuid", nullable: true),
                    MinimumSpend = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MinTierID = table.Column<Guid>(type: "uuid", nullable: true),
                    TotalUsageLimit = table.Column<int>(type: "integer", nullable: true),
                    UsageLimitPerCustomer = table.Column<int>(type: "integer", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsStackable = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.PromotionID);
                    table.CheckConstraint("CK_Promotions_Dates", "\"EndDate\" > \"StartDate\"");
                    table.CheckConstraint("CK_Promotions_MinimumSpend", "\"MinimumSpend\" >= 0");
                    table.CheckConstraint("CK_Promotions_UsageLimits", "(\"TotalUsageLimit\" IS NULL OR \"TotalUsageLimit\" > 0) AND (\"UsageLimitPerCustomer\" IS NULL OR \"UsageLimitPerCustomer\" > 0)");
                    table.CheckConstraint("CK_Promotions_Value", "\"PromotionValue\" >= 0");
                    table.ForeignKey(
                        name: "FK_Promotions_LoyaltyTiers_MinTierID",
                        column: x => x.MinTierID,
                        principalTable: "LoyaltyTiers",
                        principalColumn: "TierID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Promotions_Services_FreeServiceID",
                        column: x => x.FreeServiceID,
                        principalTable: "Services",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Rewards",
                columns: table => new
                {
                    RewardID = table.Column<Guid>(type: "uuid", nullable: false),
                    RewardName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RewardType = table.Column<int>(type: "integer", nullable: false),
                    PointsRequired = table.Column<int>(type: "integer", nullable: false),
                    RewardValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ServiceID = table.Column<Guid>(type: "uuid", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsageLimitPerCustomer = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rewards", x => x.RewardID);
                    table.CheckConstraint("CK_Rewards_Dates", "\"ValidFrom\" IS NULL OR \"ValidTo\" IS NULL OR \"ValidTo\" > \"ValidFrom\"");
                    table.CheckConstraint("CK_Rewards_PointsRequired", "\"PointsRequired\" > 0");
                    table.CheckConstraint("CK_Rewards_UsageLimit", "\"UsageLimitPerCustomer\" IS NULL OR \"UsageLimitPerCustomer\" > 0");
                    table.CheckConstraint("CK_Rewards_Value", "\"RewardValue\" >= 0");
                    table.ForeignKey(
                        name: "FK_Rewards_Services_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TierBenefits",
                columns: table => new
                {
                    TierBenefitID = table.Column<Guid>(type: "uuid", nullable: false),
                    TierID = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceID = table.Column<Guid>(type: "uuid", nullable: true),
                    BenefitName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BenefitType = table.Column<int>(type: "integer", nullable: false),
                    BenefitValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MonthlyLimit = table.Column<int>(type: "integer", nullable: true),
                    IsAutoApplied = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TierBenefits", x => x.TierBenefitID);
                    table.CheckConstraint("CK_TierBenefits_MonthlyLimit", "\"MonthlyLimit\" IS NULL OR \"MonthlyLimit\" > 0");
                    table.CheckConstraint("CK_TierBenefits_Value", "\"BenefitValue\" >= 0");
                    table.ForeignKey(
                        name: "FK_TierBenefits_LoyaltyTiers_TierID",
                        column: x => x.TierID,
                        principalTable: "LoyaltyTiers",
                        principalColumn: "TierID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TierBenefits_Services_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    TierID = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentPoints = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LifetimePoints = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalSpent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TotalVisits = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastVisitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentTierSince = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextTierReviewAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerID);
                    table.CheckConstraint("CK_Customers_CurrentPoints", "\"CurrentPoints\" >= 0");
                    table.CheckConstraint("CK_Customers_LifetimePoints", "\"LifetimePoints\" >= 0");
                    table.CheckConstraint("CK_Customers_TotalSpent", "\"TotalSpent\" >= 0");
                    table.CheckConstraint("CK_Customers_TotalVisits", "\"TotalVisits\" >= 0");
                    table.ForeignKey(
                        name: "FK_Customers_LoyaltyTiers_TierID",
                        column: x => x.TierID,
                        principalTable: "LoyaltyTiers",
                        principalColumn: "TierID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Customers_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PromotionServices",
                columns: table => new
                {
                    PromotionID = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionServices", x => new { x.PromotionID, x.ServiceID });
                    table.ForeignKey(
                        name: "FK_PromotionServices_Promotions_PromotionID",
                        column: x => x.PromotionID,
                        principalTable: "Promotions",
                        principalColumn: "PromotionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromotionServices_Services_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerTierHistories",
                columns: table => new
                {
                    CustomerTierHistoryID = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerID = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousTierID = table.Column<Guid>(type: "uuid", nullable: true),
                    NewTierID = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewPeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewPeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    QualifiedSpent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    QualifiedVisits = table.Column<int>(type: "integer", nullable: false),
                    ChangeReason = table.Column<int>(type: "integer", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerTierHistories", x => x.CustomerTierHistoryID);
                    table.CheckConstraint("CK_CustomerTierHistories_Period", "\"ReviewPeriodEnd\" >= \"ReviewPeriodStart\"");
                    table.CheckConstraint("CK_CustomerTierHistories_QualifiedValues", "\"QualifiedSpent\" >= 0 AND \"QualifiedVisits\" >= 0");
                    table.ForeignKey(
                        name: "FK_CustomerTierHistories_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerTierHistories_LoyaltyTiers_NewTierID",
                        column: x => x.NewTierID,
                        principalTable: "LoyaltyTiers",
                        principalColumn: "TierID");
                    table.ForeignKey(
                        name: "FK_CustomerTierHistories_LoyaltyTiers_PreviousTierID",
                        column: x => x.PreviousTierID,
                        principalTable: "LoyaltyTiers",
                        principalColumn: "TierID");
                });

            migrationBuilder.CreateTable(
                name: "PromotionCustomers",
                columns: table => new
                {
                    PromotionCustomerID = table.Column<Guid>(type: "uuid", nullable: false),
                    PromotionID = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerID = table.Column<Guid>(type: "uuid", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    UsageCount = table.Column<int>(type: "integer", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionCustomers", x => x.PromotionCustomerID);
                    table.CheckConstraint("CK_PromotionCustomers_UsageCount", "\"UsageCount\" >= 0");
                    table.ForeignKey(
                        name: "FK_PromotionCustomers_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionCustomers_Promotions_PromotionID",
                        column: x => x.PromotionID,
                        principalTable: "Promotions",
                        principalColumn: "PromotionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    VehicleID = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerID = table.Column<Guid>(type: "uuid", nullable: false),
                    LicensePlate = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VehicleType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.VehicleID);
                    table.ForeignKey(
                        name: "FK_Vehicles_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    BookingID = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerID = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleID = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchID = table.Column<Guid>(type: "uuid", nullable: false),
                    WashBayID = table.Column<Guid>(type: "uuid", nullable: true),
                    TierIDSnapshot = table.Column<Guid>(type: "uuid", nullable: true),
                    ScheduledStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScheduledEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BookingStatus = table.Column<int>(type: "integer", nullable: false),
                    QueuePriority = table.Column<int>(type: "integer", nullable: false),
                    EstimatedTotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CheckInAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.BookingID);
                    table.CheckConstraint("CK_Bookings_Amount", "\"EstimatedTotalAmount\" >= 0");
                    table.CheckConstraint("CK_Bookings_QueuePriority", "\"QueuePriority\" >= 0");
                    table.CheckConstraint("CK_Bookings_Schedule", "\"ScheduledEnd\" > \"ScheduledStart\"");
                    table.ForeignKey(
                        name: "FK_Bookings_Branches_BranchID",
                        column: x => x.BranchID,
                        principalTable: "Branches",
                        principalColumn: "BranchID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_LoyaltyTiers_TierIDSnapshot",
                        column: x => x.TierIDSnapshot,
                        principalTable: "LoyaltyTiers",
                        principalColumn: "TierID");
                    table.ForeignKey(
                        name: "FK_Bookings_Vehicles_VehicleID",
                        column: x => x.VehicleID,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleID");
                    table.ForeignKey(
                        name: "FK_Bookings_WashBays_WashBayID",
                        column: x => x.WashBayID,
                        principalTable: "WashBays",
                        principalColumn: "WashBayID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LicensePlateRecognitionLogs",
                columns: table => new
                {
                    RecognitionID = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchID = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchedVehicleID = table.Column<Guid>(type: "uuid", nullable: true),
                    DetectedPlate = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NormalizedPlate = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    ReviewStatus = table.Column<int>(type: "integer", nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicensePlateRecognitionLogs", x => x.RecognitionID);
                    table.CheckConstraint("CK_LprLogs_Confidence", "\"ConfidenceScore\" >= 0 AND \"ConfidenceScore\" <= 1");
                    table.ForeignKey(
                        name: "FK_LicensePlateRecognitionLogs_Branches_BranchID",
                        column: x => x.BranchID,
                        principalTable: "Branches",
                        principalColumn: "BranchID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LicensePlateRecognitionLogs_Vehicles_MatchedVehicleID",
                        column: x => x.MatchedVehicleID,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BehavioralLogs",
                columns: table => new
                {
                    LogID = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerID = table.Column<Guid>(type: "uuid", nullable: true),
                    BookingID = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceID = table.Column<Guid>(type: "uuid", nullable: true),
                    PromotionID = table.Column<Guid>(type: "uuid", nullable: true),
                    SessionID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    ActionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    PointsChanged = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    SpendingAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    RewardUsed = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    PromotionUsed = table.Column<bool>(type: "boolean", nullable: false),
                    MetadataJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BehavioralLogs", x => x.LogID);
                    table.ForeignKey(
                        name: "FK_BehavioralLogs_Bookings_BookingID",
                        column: x => x.BookingID,
                        principalTable: "Bookings",
                        principalColumn: "BookingID");
                    table.ForeignKey(
                        name: "FK_BehavioralLogs_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID");
                    table.ForeignKey(
                        name: "FK_BehavioralLogs_Promotions_PromotionID",
                        column: x => x.PromotionID,
                        principalTable: "Promotions",
                        principalColumn: "PromotionID");
                    table.ForeignKey(
                        name: "FK_BehavioralLogs_Services_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ServiceID");
                });

            migrationBuilder.CreateTable(
                name: "BookingDetails",
                columns: table => new
                {
                    BookingDetailID = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingID = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceID = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingDetails", x => x.BookingDetailID);
                    table.CheckConstraint("CK_BookingDetails_Quantity", "\"Quantity\" > 0");
                    table.CheckConstraint("CK_BookingDetails_UnitPrice", "\"UnitPrice\" >= 0");
                    table.ForeignKey(
                        name: "FK_BookingDetails_Bookings_BookingID",
                        column: x => x.BookingID,
                        principalTable: "Bookings",
                        principalColumn: "BookingID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingDetails_Services_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookingPromotions",
                columns: table => new
                {
                    BookingPromotionID = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingID = table.Column<Guid>(type: "uuid", nullable: false),
                    PromotionID = table.Column<Guid>(type: "uuid", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BonusPoints = table.Column<int>(type: "integer", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingPromotions", x => x.BookingPromotionID);
                    table.CheckConstraint("CK_BookingPromotions_BonusPoints", "\"BonusPoints\" >= 0");
                    table.CheckConstraint("CK_BookingPromotions_Discount", "\"DiscountAmount\" >= 0");
                    table.ForeignKey(
                        name: "FK_BookingPromotions_Bookings_BookingID",
                        column: x => x.BookingID,
                        principalTable: "Bookings",
                        principalColumn: "BookingID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingPromotions_Promotions_PromotionID",
                        column: x => x.PromotionID,
                        principalTable: "Promotions",
                        principalColumn: "PromotionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentID = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingID = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentID);
                    table.CheckConstraint("CK_Payments_Amount", "\"Amount\" >= 0");
                    table.ForeignKey(
                        name: "FK_Payments_Bookings_BookingID",
                        column: x => x.BookingID,
                        principalTable: "Bookings",
                        principalColumn: "BookingID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RewardRedemptions",
                columns: table => new
                {
                    RedemptionID = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerID = table.Column<Guid>(type: "uuid", nullable: false),
                    RewardID = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingID = table.Column<Guid>(type: "uuid", nullable: true),
                    PointsSpent = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RedeemedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardRedemptions", x => x.RedemptionID);
                    table.CheckConstraint("CK_RewardRedemptions_Points", "\"PointsSpent\" > 0");
                    table.ForeignKey(
                        name: "FK_RewardRedemptions_Bookings_BookingID",
                        column: x => x.BookingID,
                        principalTable: "Bookings",
                        principalColumn: "BookingID");
                    table.ForeignKey(
                        name: "FK_RewardRedemptions_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RewardRedemptions_Rewards_RewardID",
                        column: x => x.RewardID,
                        principalTable: "Rewards",
                        principalColumn: "RewardID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WashHistories",
                columns: table => new
                {
                    WashHistoryID = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingID = table.Column<Guid>(type: "uuid", nullable: false),
                    WashDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualTotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PointsEarned = table.Column<int>(type: "integer", nullable: false),
                    RewardUsed = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CustomerRating = table.Column<int>(type: "integer", nullable: true),
                    Feedback = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WashHistories", x => x.WashHistoryID);
                    table.CheckConstraint("CK_WashHistories_Amounts", "\"ActualTotalAmount\" >= 0 AND \"DiscountAmount\" >= 0 AND \"FinalAmount\" >= 0");
                    table.CheckConstraint("CK_WashHistories_Points", "\"PointsEarned\" >= 0");
                    table.CheckConstraint("CK_WashHistories_Rating", "\"CustomerRating\" IS NULL OR (\"CustomerRating\" >= 1 AND \"CustomerRating\" <= 5)");
                    table.ForeignKey(
                        name: "FK_WashHistories_Bookings_BookingID",
                        column: x => x.BookingID,
                        principalTable: "Bookings",
                        principalColumn: "BookingID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyPointTransactions",
                columns: table => new
                {
                    TransactionID = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerID = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingID = table.Column<Guid>(type: "uuid", nullable: true),
                    WashHistoryID = table.Column<Guid>(type: "uuid", nullable: true),
                    RedemptionID = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceTransactionID = table.Column<Guid>(type: "uuid", nullable: true),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    OriginalPoints = table.Column<int>(type: "integer", nullable: false),
                    RemainingPoints = table.Column<int>(type: "integer", nullable: false),
                    BalanceAfter = table.Column<int>(type: "integer", nullable: false),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyPointTransactions", x => x.TransactionID);
                    table.CheckConstraint("CK_PointTransactions_BalanceAfter", "\"BalanceAfter\" >= 0");
                    table.CheckConstraint("CK_PointTransactions_OriginalPoints", "\"OriginalPoints\" >= 0");
                    table.CheckConstraint("CK_PointTransactions_RemainingPoints", "\"RemainingPoints\" >= 0 AND \"RemainingPoints\" <= \"OriginalPoints\"");
                    table.ForeignKey(
                        name: "FK_LoyaltyPointTransactions_Bookings_BookingID",
                        column: x => x.BookingID,
                        principalTable: "Bookings",
                        principalColumn: "BookingID");
                    table.ForeignKey(
                        name: "FK_LoyaltyPointTransactions_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoyaltyPointTransactions_LoyaltyPointTransactions_Reference~",
                        column: x => x.ReferenceTransactionID,
                        principalTable: "LoyaltyPointTransactions",
                        principalColumn: "TransactionID");
                    table.ForeignKey(
                        name: "FK_LoyaltyPointTransactions_RewardRedemptions_RedemptionID",
                        column: x => x.RedemptionID,
                        principalTable: "RewardRedemptions",
                        principalColumn: "RedemptionID");
                    table.ForeignKey(
                        name: "FK_LoyaltyPointTransactions_WashHistories_WashHistoryID",
                        column: x => x.WashHistoryID,
                        principalTable: "WashHistories",
                        principalColumn: "WashHistoryID");
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleID", "RoleName" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Admin" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Staff" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Customer" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BehavioralLogs_BookingID",
                table: "BehavioralLogs",
                column: "BookingID");

            migrationBuilder.CreateIndex(
                name: "IX_BehavioralLogs_CustomerID",
                table: "BehavioralLogs",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_BehavioralLogs_PromotionID",
                table: "BehavioralLogs",
                column: "PromotionID");

            migrationBuilder.CreateIndex(
                name: "IX_BehavioralLogs_ServiceID",
                table: "BehavioralLogs",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_BookingDetails_BookingID_ServiceID",
                table: "BookingDetails",
                columns: new[] { "BookingID", "ServiceID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingDetails_ServiceID",
                table: "BookingDetails",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_BookingPromotions_BookingID_PromotionID",
                table: "BookingPromotions",
                columns: new[] { "BookingID", "PromotionID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingPromotions_PromotionID",
                table: "BookingPromotions",
                column: "PromotionID");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BranchID_ScheduledStart_BookingStatus",
                table: "Bookings",
                columns: new[] { "BranchID", "ScheduledStart", "BookingStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CustomerID",
                table: "Bookings",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TierIDSnapshot",
                table: "Bookings",
                column: "TierIDSnapshot");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_VehicleID",
                table: "Bookings",
                column: "VehicleID");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_WashBayID_ScheduledStart_ScheduledEnd",
                table: "Bookings",
                columns: new[] { "WashBayID", "ScheduledStart", "ScheduledEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_BranchName",
                table: "Branches",
                column: "BranchName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TierID",
                table: "Customers",
                column: "TierID");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserID",
                table: "Customers",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTierHistories_CustomerID_ChangedAt",
                table: "CustomerTierHistories",
                columns: new[] { "CustomerID", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTierHistories_NewTierID",
                table: "CustomerTierHistories",
                column: "NewTierID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTierHistories_PreviousTierID",
                table: "CustomerTierHistories",
                column: "PreviousTierID");

            migrationBuilder.CreateIndex(
                name: "IX_LicensePlateRecognitionLogs_BranchID",
                table: "LicensePlateRecognitionLogs",
                column: "BranchID");

            migrationBuilder.CreateIndex(
                name: "IX_LicensePlateRecognitionLogs_MatchedVehicleID",
                table: "LicensePlateRecognitionLogs",
                column: "MatchedVehicleID");

            migrationBuilder.CreateIndex(
                name: "IX_LicensePlateRecognitionLogs_NormalizedPlate_DetectedAt",
                table: "LicensePlateRecognitionLogs",
                columns: new[] { "NormalizedPlate", "DetectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyPointTransactions_BookingID",
                table: "LoyaltyPointTransactions",
                column: "BookingID");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyPointTransactions_CustomerID_ExpiryDate_RemainingPoi~",
                table: "LoyaltyPointTransactions",
                columns: new[] { "CustomerID", "ExpiryDate", "RemainingPoints" });

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyPointTransactions_IdempotencyKey",
                table: "LoyaltyPointTransactions",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyPointTransactions_RedemptionID",
                table: "LoyaltyPointTransactions",
                column: "RedemptionID");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyPointTransactions_ReferenceTransactionID",
                table: "LoyaltyPointTransactions",
                column: "ReferenceTransactionID");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyPointTransactions_WashHistoryID",
                table: "LoyaltyPointTransactions",
                column: "WashHistoryID");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTiers_TierName",
                table: "LoyaltyTiers",
                column: "TierName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTiers_TierRank",
                table: "LoyaltyTiers",
                column: "TierRank",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingID",
                table: "Payments",
                column: "BookingID");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionCustomers_CustomerID",
                table: "PromotionCustomers",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionCustomers_PromotionID_CustomerID",
                table: "PromotionCustomers",
                columns: new[] { "PromotionID", "CustomerID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_FreeServiceID",
                table: "Promotions",
                column: "FreeServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_MinTierID",
                table: "Promotions",
                column: "MinTierID");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_PromotionCode",
                table: "Promotions",
                column: "PromotionCode",
                unique: true,
                filter: "\"PromotionCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionServices_ServiceID",
                table: "PromotionServices",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_RewardRedemptions_BookingID",
                table: "RewardRedemptions",
                column: "BookingID");

            migrationBuilder.CreateIndex(
                name: "IX_RewardRedemptions_CustomerID_RedeemedAt",
                table: "RewardRedemptions",
                columns: new[] { "CustomerID", "RedeemedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RewardRedemptions_RewardID",
                table: "RewardRedemptions",
                column: "RewardID");

            migrationBuilder.CreateIndex(
                name: "IX_Rewards_RewardName",
                table: "Rewards",
                column: "RewardName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rewards_ServiceID",
                table: "Rewards",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_Services_ServiceName",
                table: "Services",
                column: "ServiceName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TierBenefits_ServiceID",
                table: "TierBenefits",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_TierBenefits_TierID_BenefitName",
                table: "TierBenefits",
                columns: new[] { "TierID", "BenefitName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users",
                column: "PhoneNumber",
                unique: true,
                filter: "\"PhoneNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleID",
                table: "Users",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_CustomerID",
                table: "Vehicles",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles",
                column: "LicensePlate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WashBays_BranchID_BayName",
                table: "WashBays",
                columns: new[] { "BranchID", "BayName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WashHistories_BookingID",
                table: "WashHistories",
                column: "BookingID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BehavioralLogs");

            migrationBuilder.DropTable(
                name: "BookingDetails");

            migrationBuilder.DropTable(
                name: "BookingPromotions");

            migrationBuilder.DropTable(
                name: "CustomerTierHistories");

            migrationBuilder.DropTable(
                name: "LicensePlateRecognitionLogs");

            migrationBuilder.DropTable(
                name: "LoyaltyPointTransactions");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PromotionCustomers");

            migrationBuilder.DropTable(
                name: "PromotionServices");

            migrationBuilder.DropTable(
                name: "TierBenefits");

            migrationBuilder.DropTable(
                name: "RewardRedemptions");

            migrationBuilder.DropTable(
                name: "WashHistories");

            migrationBuilder.DropTable(
                name: "Promotions");

            migrationBuilder.DropTable(
                name: "Rewards");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "WashBays");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "LoyaltyTiers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
