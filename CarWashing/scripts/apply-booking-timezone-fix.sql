-- Run this once against the existing AutoWash database after taking a backup.
-- It is idempotent: the maintenance record prevents a second seven-hour shift.

BEGIN;

CREATE TABLE IF NOT EXISTS "__AppMaintenanceHistory" (
    "Id" character varying(150) NOT NULL PRIMARY KEY,
    "AppliedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP
);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM "__AppMaintenanceHistory"
        WHERE "Id" = '20260724_CorrectBookingScheduleUtc'
    ) THEN
        UPDATE "Bookings"
        SET "ScheduledStart" = "ScheduledStart" - INTERVAL '7 hours',
            "ScheduledEnd" = "ScheduledEnd" - INTERVAL '7 hours';

        UPDATE "BookingRescheduleHistories"
        SET "PreviousStart" = "PreviousStart" - INTERVAL '7 hours',
            "PreviousEnd" = "PreviousEnd" - INTERVAL '7 hours',
            "NewStart" = "NewStart" - INTERVAL '7 hours',
            "NewEnd" = "NewEnd" - INTERVAL '7 hours';

        INSERT INTO "__AppMaintenanceHistory" ("Id")
        VALUES ('20260724_CorrectBookingScheduleUtc');
    END IF;
END $$;

COMMIT;
