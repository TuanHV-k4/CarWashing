using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260723010000_EnforceSingleActiveManagerBranch")]
public partial class EnforceSingleActiveManagerBranch : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_BranchManagerMemberships_UserID_Active",
            table: "BranchManagerMemberships",
            column: "UserID",
            unique: true,
            filter: "\"IsActive\" = TRUE");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_BranchManagerMemberships_UserID_Active",
            table: "BranchManagerMemberships");
    }
}
