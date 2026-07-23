using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffingAndRefunds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Refunds",
                columns: table => new
                {
                    RefundID = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentID = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refunds", x => x.RefundID);
                    table.CheckConstraint("CK_Refunds_Amount", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_Refunds_Payments_PaymentID",
                        column: x => x.PaymentID,
                        principalTable: "Payments",
                        principalColumn: "PaymentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffShifts",
                columns: table => new
                {
                    StaffShiftID = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchID = table.Column<Guid>(type: "uuid", nullable: false),
                    StartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffShifts", x => x.StaffShiftID);
                    table.CheckConstraint("CK_StaffShifts_Time", "\"EndsAt\" > \"StartsAt\"");
                    table.ForeignKey(
                        name: "FK_StaffShifts_Branches_BranchID",
                        column: x => x.BranchID,
                        principalTable: "Branches",
                        principalColumn: "BranchID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShiftAssignments",
                columns: table => new
                {
                    ShiftAssignmentID = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffShiftID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    WashBayID = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftAssignments", x => x.ShiftAssignmentID);
                    table.ForeignKey(
                        name: "FK_ShiftAssignments_StaffShifts_StaffShiftID",
                        column: x => x.StaffShiftID,
                        principalTable: "StaffShifts",
                        principalColumn: "StaffShiftID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShiftAssignments_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShiftAssignments_WashBays_WashBayID",
                        column: x => x.WashBayID,
                        principalTable: "WashBays",
                        principalColumn: "WashBayID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_PaymentID_Status",
                table: "Refunds",
                columns: new[] { "PaymentID", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAssignments_StaffShiftID_UserID",
                table: "ShiftAssignments",
                columns: new[] { "StaffShiftID", "UserID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAssignments_UserID",
                table: "ShiftAssignments",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAssignments_WashBayID",
                table: "ShiftAssignments",
                column: "WashBayID");

            migrationBuilder.CreateIndex(
                name: "IX_StaffShifts_BranchID_StartsAt_EndsAt",
                table: "StaffShifts",
                columns: new[] { "BranchID", "StartsAt", "EndsAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Refunds");

            migrationBuilder.DropTable(
                name: "ShiftAssignments");

            migrationBuilder.DropTable(
                name: "StaffShifts");
        }
    }
}
