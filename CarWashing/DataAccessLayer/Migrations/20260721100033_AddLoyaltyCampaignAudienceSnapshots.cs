using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    public partial class AddLoyaltyCampaignAudienceSnapshots : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "AudienceSnapshot", table: "PromotionCustomers", type: "character varying(4000)", maxLength: 4000, nullable: true);
            migrationBuilder.AddColumn<string>(name: "EligibilitySnapshot", table: "PromotionCustomers", type: "character varying(4000)", maxLength: 4000, nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "SentByUserID", table: "PromotionCustomers", type: "uuid", nullable: true);
            migrationBuilder.CreateIndex(name: "IX_PromotionCustomers_PromotionID_SentAt", table: "PromotionCustomers", columns: new[] { "PromotionID", "SentAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_PromotionCustomers_PromotionID_SentAt", table: "PromotionCustomers");
            migrationBuilder.DropColumn(name: "AudienceSnapshot", table: "PromotionCustomers");
            migrationBuilder.DropColumn(name: "EligibilitySnapshot", table: "PromotionCustomers");
            migrationBuilder.DropColumn(name: "SentByUserID", table: "PromotionCustomers");
        }
    }
}
