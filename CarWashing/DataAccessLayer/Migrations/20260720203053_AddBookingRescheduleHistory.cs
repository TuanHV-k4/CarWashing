using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingRescheduleHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookingRescheduleHistories",
                columns: table => new
                {
                    BookingRescheduleHistoryID = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingID = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PreviousEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PreviousWashBayID = table.Column<Guid>(type: "uuid", nullable: true),
                    NewStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NewEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NewWashBayID = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangedByCustomerID = table.Column<Guid>(type: "uuid", nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingRescheduleHistories", x => x.BookingRescheduleHistoryID);
                    table.CheckConstraint("CK_BookingRescheduleHistory_Schedule", "\"PreviousEnd\" > \"PreviousStart\" AND \"NewEnd\" > \"NewStart\"");
                    table.ForeignKey(
                        name: "FK_BookingRescheduleHistories_Bookings_BookingID",
                        column: x => x.BookingID,
                        principalTable: "Bookings",
                        principalColumn: "BookingID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingRescheduleHistories_BookingID_ChangedAt",
                table: "BookingRescheduleHistories",
                columns: new[] { "BookingID", "ChangedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingRescheduleHistories");
        }
    }
}
