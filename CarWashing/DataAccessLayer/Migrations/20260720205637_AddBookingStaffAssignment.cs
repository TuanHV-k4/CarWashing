using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingStaffAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedStaffID",
                table: "Bookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_AssignedStaffID",
                table: "Bookings",
                column: "AssignedStaffID");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_AssignedStaffID",
                table: "Bookings",
                column: "AssignedStaffID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_AssignedStaffID",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_AssignedStaffID",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "AssignedStaffID",
                table: "Bookings");
        }
    }
}
