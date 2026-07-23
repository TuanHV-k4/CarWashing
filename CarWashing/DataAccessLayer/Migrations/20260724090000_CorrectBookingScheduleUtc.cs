using DataAccessLayer.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations;

/// <summary>
/// Corrects schedule values previously entered as Vietnam local time but persisted as UTC.
/// Apply only once; a 09:00 Vietnam booking stored as 09:00Z becomes 02:00Z.
/// </summary>
[DbContext(typeof(ApplicationDbContext))]
[Migration("20260724090000_CorrectBookingScheduleUtc")]
public partial class CorrectBookingScheduleUtc : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS "__AppMaintenanceHistory" (
                "Id" character varying(150) NOT NULL PRIMARY KEY,
                "AppliedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            DO $$
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM "__AppMaintenanceHistory" WHERE "Id" = '20260724_CorrectBookingScheduleUtc') THEN
                    UPDATE "Bookings" SET "ScheduledStart" = "ScheduledStart" - INTERVAL '7 hours', "ScheduledEnd" = "ScheduledEnd" - INTERVAL '7 hours';
                    UPDATE "BookingRescheduleHistories" SET "PreviousStart" = "PreviousStart" - INTERVAL '7 hours', "PreviousEnd" = "PreviousEnd" - INTERVAL '7 hours', "NewStart" = "NewStart" - INTERVAL '7 hours', "NewEnd" = "NewEnd" - INTERVAL '7 hours';
                    INSERT INTO "__AppMaintenanceHistory" ("Id") VALUES ('20260724_CorrectBookingScheduleUtc');
                END IF;
            END $$;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE \"Bookings\" SET \"ScheduledStart\" = \"ScheduledStart\" + INTERVAL '7 hours', \"ScheduledEnd\" = \"ScheduledEnd\" + INTERVAL '7 hours';");
        migrationBuilder.Sql("UPDATE \"BookingRescheduleHistories\" SET \"PreviousStart\" = \"PreviousStart\" + INTERVAL '7 hours', \"PreviousEnd\" = \"PreviousEnd\" + INTERVAL '7 hours', \"NewStart\" = \"NewStart\" + INTERVAL '7 hours', \"NewEnd\" = \"NewEnd\" + INTERVAL '7 hours';");
    }
}
