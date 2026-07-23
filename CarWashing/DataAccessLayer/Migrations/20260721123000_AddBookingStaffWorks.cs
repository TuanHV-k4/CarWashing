using System;
using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260721123000_AddBookingStaffWorks")]
    public partial class AddBookingStaffWorks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookingStaffWorks",
                columns: table => new
                {
                    BookingStaffWorkID = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingID = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffUserID = table.Column<Guid>(type: "uuid", nullable: false),
                    ContributionPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    WorkRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AdjustmentReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AssignedByUserID = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingStaffWorks", x => x.BookingStaffWorkID);
                    table.ForeignKey("FK_BookingStaffWorks_Bookings_BookingID", x => x.BookingID, "Bookings", "BookingID", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_BookingStaffWorks_Users_AssignedByUserID", x => x.AssignedByUserID, "Users", "UserID", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_BookingStaffWorks_Users_StaffUserID", x => x.StaffUserID, "Users", "UserID", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_BookingStaffWorks_AssignedByUserID", table: "BookingStaffWorks", column: "AssignedByUserID");
            migrationBuilder.CreateIndex(name: "IX_BookingStaffWorks_BookingID_StaffUserID", table: "BookingStaffWorks", columns: new[] { "BookingID", "StaffUserID" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_BookingStaffWorks_StaffUserID_BookingID", table: "BookingStaffWorks", columns: new[] { "StaffUserID", "BookingID" });
            migrationBuilder.AddCheckConstraint(name: "CK_BookingStaffWorks_Contribution", table: "BookingStaffWorks", sql: "\"ContributionPercent\" > 0 AND \"ContributionPercent\" <= 100");
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "BookingStaffWorks");
    }
}
