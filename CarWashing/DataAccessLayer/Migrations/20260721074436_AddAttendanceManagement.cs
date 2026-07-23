using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttendanceRecords",
                columns: table => new
                {
                    AttendanceRecordID = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftAssignmentID = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CheckedInAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckedOutAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckInSource = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CheckOutSource = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    LateMinutes = table.Column<int>(type: "integer", nullable: false),
                    EarlyLeaveMinutes = table.Column<int>(type: "integer", nullable: false),
                    WorkedMinutes = table.Column<int>(type: "integer", nullable: false),
                    AdminNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockedByUserID = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecords", x => x.AttendanceRecordID);
                    table.CheckConstraint("CK_AttendanceRecords_WorkedMinutes", "\"WorkedMinutes\" >= 0 AND \"LateMinutes\" >= 0 AND \"EarlyLeaveMinutes\" >= 0");
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_ShiftAssignments_ShiftAssignmentID",
                        column: x => x.ShiftAssignmentID,
                        principalTable: "ShiftAssignments",
                        principalColumn: "ShiftAssignmentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Users_LockedByUserID",
                        column: x => x.LockedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AttendanceAdjustments",
                columns: table => new
                {
                    AttendanceAdjustmentID = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendanceRecordID = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PreviousValues = table.Column<string>(type: "jsonb", nullable: false),
                    NewValues = table.Column<string>(type: "jsonb", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AdjustedByUserID = table.Column<Guid>(type: "uuid", nullable: false),
                    AdjustedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceAdjustments", x => x.AttendanceAdjustmentID);
                    table.ForeignKey(
                        name: "FK_AttendanceAdjustments_AttendanceRecords_AttendanceRecordID",
                        column: x => x.AttendanceRecordID,
                        principalTable: "AttendanceRecords",
                        principalColumn: "AttendanceRecordID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttendanceAdjustments_Users_AdjustedByUserID",
                        column: x => x.AdjustedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceAdjustments_AdjustedByUserID",
                table: "AttendanceAdjustments",
                column: "AdjustedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceAdjustments_AttendanceRecordID_AdjustedAt",
                table: "AttendanceAdjustments",
                columns: new[] { "AttendanceRecordID", "AdjustedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_LockedByUserID",
                table: "AttendanceRecords",
                column: "LockedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_ShiftAssignmentID",
                table: "AttendanceRecords",
                column: "ShiftAssignmentID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_Status_CheckedInAt",
                table: "AttendanceRecords",
                columns: new[] { "Status", "CheckedInAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceAdjustments");

            migrationBuilder.DropTable(
                name: "AttendanceRecords");
        }
    }
}
