# AutoWash Pro Database Design

## 1. Core customer data

- `Users`: authentication identity and contact information.
- `Customers`: loyalty balance, current tier, spending and visit aggregates.
- `Vehicles`: customer vehicles identified by a unique normalized license plate.
- `CustomerTierHistories`: immutable history of every tier assignment, upgrade and downgrade.

`Users` and `Customers` use a one-to-one relationship. A customer may own multiple vehicles.

## 2. Booking and wash operations

- `Branches`: wash locations and operating hours.
- `WashBays`: physical washing capacity at each branch.
- `Bookings`: scheduled interval, tier snapshot, priority snapshot and operational timestamps.
- `BookingDetails`: services and price snapshots selected for a booking.
- `WashHistories`: final result of a completed booking. One booking has at most one wash history.
- `Payments`: records payment collected at the counter. Online payment and refund processing are outside the project scope.

The booking service must validate that:

1. The vehicle belongs to the booking customer.
2. The booking date is within the customer's tier booking window.
3. The selected wash bay has no overlapping active booking.
4. `ScheduledEnd` includes the total estimated service duration.
5. Tier and queue priority are copied into the booking as snapshots.

## 3. Loyalty tiers and benefits

- `LoyaltyTiers`: configurable qualification thresholds, booking window, queue priority and point multiplier.
- `TierBenefits`: structured auto-applied benefits instead of storing business rules in text.
- `CustomerTierHistories`: evidence used by each monthly tier review.

A monthly background job should calculate qualified spending and completed visits over `QualificationPeriodMonths`, select the highest matching active tier, update the customer and insert a tier history record in one transaction.

## 4. Point ledger and rewards

- `LoyaltyPointTransactions`: immutable point ledger with source references and balance snapshots.
- `Rewards`: reward catalog for fixed discounts, percentage discounts, free washes and add-on services.
- `RewardRedemptions`: reservation and usage lifecycle of a redeemed reward.

Point rules:

1. Earn transactions create a point batch with `OriginalPoints` and `RemainingPoints`.
2. Earned points receive an `ExpiryDate` of 12 months after earning.
3. Redemption consumes batches by earliest expiry first.
4. Expiry and redemption create negative ledger transactions; historical transactions are never deleted or edited.
5. Customer balance and ledger entries must be updated in one database transaction.
6. `IdempotencyKey` prevents duplicate points when completing a booking more than once.

## 5. Promotions

- `Promotions`: promotion type, value, target tier, dates, limits and stacking policy.
- `PromotionServices`: services eligible for a promotion.
- `PromotionCustomers`: customers targeted by a campaign and their usage count.
- `BookingPromotions`: promotion values actually applied to a booking.

Promotion eligibility must be evaluated at checkout and the applied result stored in `BookingPromotions`. Historical invoices must not be recalculated from a promotion that an admin later edits.

## 6. LPR and personalization

- `LicensePlateRecognitionLogs`: detected plate, normalized plate, confidence, image reference and manual review status.
- `BehavioralLogs`: customer interactions with bookings, services and promotions for reporting or optional AI personalization.

Images should be stored in object/file storage. The database stores only the image URL and recognition metadata.

## 7. Concurrency and deletion

- `Customers` and `Bookings` use a numeric concurrency token for optimistic concurrency.
- Transactional and historical records use restricted deletion.
- Deactivation or soft deletion should be preferred for customers, vehicles, services, rewards and promotions.
- Aggregate fields such as `CurrentPoints`, `TotalSpent` and `TotalVisits` are cached values and should be rebuildable from ledger and completed wash data.

## 8. Migration

The project uses PostgreSQL through Npgsql. Supply the connection string through `ConnectionStrings__MyDB` and apply migrations with:

```powershell
dotnet ef database update --project DataAccessLayer --startup-project API
```

The upgrade migration preserves existing promotion percentages, converts `BookingDate` to `ScheduledStart`, initializes `ScheduledEnd`, ranks existing tiers and initializes existing earned point batches.
