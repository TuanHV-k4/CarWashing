using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    public partial class AddFixedBranchManagerAttendance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BranchManagerMemberships",
                columns: table => new
                {
                    BranchManagerMembershipID = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchManagerMemberships", x => x.BranchManagerMembershipID);
                    table.ForeignKey("FK_BranchManagerMemberships_Branches_BranchID", x => x.BranchID, "Branches", "BranchID", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_BranchManagerMemberships_Users_UserID", x => x.UserID, "Users", "UserID", onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateIndex(name: "IX_BranchManagerMemberships_BranchID_UserID", table: "BranchManagerMemberships", columns: new[] { "BranchID", "UserID" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_BranchManagerMemberships_UserID", table: "BranchManagerMemberships", column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "BranchManagerMemberships");
    }
}
