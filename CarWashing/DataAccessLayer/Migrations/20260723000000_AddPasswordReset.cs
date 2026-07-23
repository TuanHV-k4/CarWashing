using System;
using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260723000000_AddPasswordReset")]
public partial class AddPasswordReset : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(name: "AuthVersion", table: "Users", type: "integer", nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<DateTime>(name: "PasswordResetTokenExpiry", table: "Users", type: "timestamp with time zone", nullable: true);
        migrationBuilder.AddColumn<string>(name: "PasswordResetTokenHash", table: "Users", type: "character varying(64)", maxLength: 64, nullable: true);
        migrationBuilder.CreateIndex(name: "IX_Users_PasswordResetTokenHash", table: "Users", column: "PasswordResetTokenHash");
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Users_PasswordResetTokenHash", table: "Users");
        migrationBuilder.DropColumn(name: "AuthVersion", table: "Users");
        migrationBuilder.DropColumn(name: "PasswordResetTokenExpiry", table: "Users");
        migrationBuilder.DropColumn(name: "PasswordResetTokenHash", table: "Users");
    }
}
