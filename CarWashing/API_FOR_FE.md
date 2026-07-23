# AutoWash Pro API Contract for Frontend

## Manager branch scope

- `GET /api/manager/branch-context` trả về `{ branchId, branchName }` của branch active được gán cho BranchManager hiện tại.
- Attendance và workload dành cho manager tự xác định branch từ tài khoản đăng nhập; client không gửi `branchId`.
- Mỗi BranchManager chỉ có một assignment active. Admin phải deactivate assignment cũ trước khi gán branch mới.

Base URL khi chay local: `https://localhost:<port>` hoac `http://localhost:<port>`.

Mac dinh request/response dung JSON. Cac endpoint co `[Authorize]` can header:

```http
Authorization: Bearer <accessToken>
Content-Type: application/json
```

Luu y hien tai:
- `Operations` va `Loyalty` da bo `[Authorize]` theo yeu cau gan nhat, nen FE co the goi truc tiep. Tuy nhien cac API `Auth`, `Vehicles`, `Customers`, `AI`, `AdminUsers`, `BehavioralLogs`, `WashHistories` van co authorize theo controller hien co.
- Dang co 2 controller cung route thuc te `api/services`: `API/Controllers/ServicesController.cs` va `API/Controllers/Operations/ServicesController.cs`. FE nen uu tien contract `Operations Services` ben duoi. Nen doi/xoa 1 route sau de tranh ambiguous endpoint.
- DateTime gui dang ISO string, vi du: `"2026-07-01T09:00:00Z"`.
- Paged response cua Operations/Loyalty co dang `{ items, page, pageSize, totalCount }`.
- `TimeSpan` trong Branch co the gui dang `"08:00:00"`.

## Auth

### POST `/api/Auth/register`
Dang ky tai khoan customer, sau do he thong gui OTP email.

Request:
```json
{
  "username": "customer01",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "fullName": "Nguyen Van A",
  "email": "a@example.com",
  "phoneNumber": "0900000000"
}
```

Response `201`:
```json
{
  "userID": "guid",
  "username": "customer01",
  "fullName": "Nguyen Van A",
  "email": "a@example.com",
  "phoneNumber": "0900000000",
  "role": "Customer",
  "createdAt": "2026-07-01T00:00:00Z"
}
```

### POST `/api/Auth/verify-email`
Request:
```json
{ "email": "a@example.com", "otpCode": "123456" }
```

### POST `/api/Auth/resend-otp`
Request:
```json
{ "email": "a@example.com" }
```

### POST `/api/Auth/login`
Request:
```json
{ "username": "customer01", "password": "Password123!" }
```

Response:
```json
{
  "userID": "guid",
  "username": "customer01",
  "fullName": "Nguyen Van A",
  "email": "a@example.com",
  "role": "Customer",
  "accessToken": "jwt",
  "accessTokenExpiration": "2026-07-01T00:00:00Z"
}
```

### GET `/api/Auth/me`
Auth required. Tra ve claim user hien tai: `userID`, `username`, `email`, `fullName`, `role`.

### GET `/api/Auth/admin-only`
Auth `AdminOnly`.

### GET `/api/Auth/customer-only`
Auth `CustomerOnly`.

## Customer

### GET `/api/customers/me`
Auth `CustomerOnly`.

Response wrapper:
```json
{
  "success": true,
  "data": {
    "customerID": "guid",
    "userID": "guid",
    "username": "customer01",
    "fullName": "Nguyen Van A",
    "email": "a@example.com",
    "phoneNumber": "0900000000",
    "currentPoints": 100,
    "lifetimePoints": 200,
    "totalSpent": 500000,
    "totalVisits": 5,
    "lastVisitDate": "2026-07-01T00:00:00Z",
    "tierID": "guid",
    "tierName": "Silver",
    "tierRank": 2,
    "currentTierSince": "2026-07-01T00:00:00Z",
    "createdAt": "2026-07-01T00:00:00Z",
    "tierPerks": ["perk"]
  }
}
```

## Vehicles

All endpoints auth `CustomerOnly`.

### GET `/api/vehicles/me`
Response wrapper: `ApiResponse<List<VehicleResponseDto>>`.

Vehicle:
```json
{
  "vehicleID": "guid",
  "customerID": "guid",
  "licensePlate": "51A12345",
  "vehicleType": "Sedan",
  "brand": "Toyota",
  "model": "Vios",
  "color": "White",
  "status": "Active",
  "createdAt": "2026-07-01T00:00:00Z"
}
```

### POST `/api/vehicles`
Request:
```json
{
  "licensePlate": "51A12345",
  "vehicleType": "Sedan",
  "brand": "Toyota",
  "model": "Vios",
  "color": "White"
}
```

### PUT `/api/vehicles/{vehicleId}`
Request:
```json
{
  "vehicleType": "SUV",
  "brand": "Honda",
  "model": "CR-V",
  "color": "Black"
}
```

### PUT `/api/vehicles/{vehicleId}/status`
Request:
```json
{ "status": "Inactive" }
```

## Operations Services

Route: `/api/services`.

### GET `/api/services?page=1&pageSize=20&includeInactive=false`
Response:
```json
{
  "items": [
    {
      "id": "guid",
      "name": "Basic Wash",
      "description": "Exterior wash",
      "price": 80000,
      "durationMinutes": 30,
      "isActive": true
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1
}
```

### GET `/api/services/{id}`
Response: `ServiceResponse`.

### POST `/api/services`
Request:
```json
{
  "name": "Basic Wash",
  "description": "Exterior wash",
  "price": 80000,
  "durationMinutes": 30
}
```

### PUT `/api/services/{id}`
Request:
```json
{
  "name": "Basic Wash",
  "description": "Exterior wash",
  "price": 80000,
  "durationMinutes": 30,
  "isActive": true
}
```

### DELETE `/api/services/{id}`
Soft delete/inactive/archive tuy theo booking lien quan.

## Branches

Route: `/api/branches`.

### GET `/api/branches?page=1&pageSize=20&includeInactive=false`
Response item:
```json
{
  "id": "guid",
  "name": "District 1",
  "address": "123 Street",
  "phone": "0900000000",
  "openTime": "08:00:00",
  "closeTime": "20:00:00",
  "isActive": true
}
```

### GET `/api/branches/{id}`
Response: `BranchResponse`.

### POST `/api/branches`
Request:
```json
{
  "name": "District 1",
  "address": "123 Street",
  "phone": "0900000000",
  "openTime": "08:00:00",
  "closeTime": "20:00:00"
}
```

### PUT `/api/branches/{id}`
Request them `isActive`.

### DELETE `/api/branches/{id}`

## Wash Bays

Route: `/api/wash-bays`.

### GET `/api/wash-bays?page=1&pageSize=20&branchId={guid}&includeInactive=false`
Response item:
```json
{
  "id": "guid",
  "branchId": "guid",
  "name": "Bay 1",
  "isActive": true
}
```

### GET `/api/wash-bays/{id}`

### POST `/api/wash-bays`
Request:
```json
{ "branchId": "guid", "name": "Bay 1" }
```

### PUT `/api/wash-bays/{id}`
Request:
```json
{ "branchId": "guid", "name": "Bay 1", "isActive": true }
```

### DELETE `/api/wash-bays/{id}`

## Bookings

Route: `/api/bookings`.

Important: `CreateBooking` lay customer hien tai tu JWT claim `customerId`. Neu khong co JWT/customer claim thi service tra `401 Current customer is required`.

### GET `/api/bookings?status=Pending&fromDate=2026-07-01&toDate=2026-07-31&branchId={guid}&page=1&pageSize=20`
Response item:
```json
{
  "id": "guid",
  "customerId": "guid",
  "vehicleId": "guid",
  "branchId": "guid",
  "serviceId": "guid",
  "washBayId": "guid",
  "bookingStartTime": "2026-07-01T09:00:00Z",
  "bookingEndTime": "2026-07-01T09:30:00Z",
  "status": "Pending",
  "totalAmount": 80000,
  "serviceNameSnapshot": "Basic Wash",
  "durationMinutesSnapshot": 30,
  "priceSnapshot": 80000,
  "tierSnapshot": "Silver",
  "createdAt": "2026-07-01T00:00:00Z",
  "note": "optional"
}
```

### GET `/api/bookings/{id}`
Response them:
```json
{ "branchName": "District 1", "washBayName": "Bay 1" }
```

### POST `/api/bookings`
Request:
```json
{
  "vehicleId": "guid",
  "branchId": "guid",
  "serviceId": "guid",
  "bookingStartTime": "2026-07-01T09:00:00Z",
  "note": "optional"
}
```

### POST `/api/bookings/{id}/cancel`
Request:
```json
{ "reason": "Change plan" }
```

### POST `/api/bookings/{id}/confirm`
Chuyen `Pending -> Confirmed`.

### POST `/api/bookings/{id}/start`
Chuyen `Confirmed -> InProgress`.

### POST `/api/bookings/{id}/complete`
Chuyen `InProgress -> Completed`, tao wash history va earn points.

## Payments

Route: `/api/payments`.

### GET `/api/payments/{id}`
Response:
```json
{
  "id": "guid",
  "bookingId": "guid",
  "amount": 80000,
  "method": "Cash",
  "status": "Pending",
  "createdAt": "2026-07-01T00:00:00Z",
  "paidAt": null,
  "referenceNumber": null,
  "note": "optional"
}
```

### POST `/api/payments`
Request:
```json
{
  "bookingId": "guid",
  "amount": 80000,
  "method": "Cash",
  "note": "optional"
}
```

Allowed method: `Cash`, `Card`, `CardAtCounter`, `BankTransfer`.

### POST `/api/payments/{id}/paid`
Request:
```json
{ "referenceNumber": "REF001", "note": "paid at counter" }
```

### POST `/api/payments/{id}/void`
Request:
```json
{ "note": "void reason" }
```

## Loyalty

Route: `/api/loyalty`.

### GET `/api/loyalty/settings`
Response:
```json
{
  "pointEarnRateAmount": 10000,
  "pointEarnRatePoints": 1,
  "pointExpiryMonths": 12,
  "earnRule": "..."
}
```

### GET `/api/loyalty/tiers?page=1&pageSize=20&includeInactive=false`
Response item:
```json
{
  "id": "guid",
  "name": "Silver",
  "rank": 2,
  "minSpent": 1000000,
  "minVisits": 5,
  "qualificationPeriodMonths": 12,
  "qualificationMode": "AllConditions",
  "bookingWindowDays": 7,
  "priorityLevel": 1,
  "pointMultiplier": 1.2,
  "benefits": "text",
  "status": "Active"
}
```

### GET `/api/loyalty/tiers/{id}`

### POST `/api/loyalty/tiers`
Request:
```json
{
  "name": "Silver",
  "rank": 2,
  "minSpent": 1000000,
  "minVisits": 5,
  "qualificationPeriodMonths": 12,
  "qualificationMode": "AllConditions",
  "bookingWindowDays": 7,
  "priorityLevel": 1,
  "pointMultiplier": 1.2,
  "benefits": "5% discount",
  "isActive": true
}
```

### PUT `/api/loyalty/tiers/{id}`
Same body as create.

### DELETE `/api/loyalty/tiers/{id}`

### GET `/api/loyalty/customers/{customerId}/points/balance`
Response:
```json
{
  "customerId": "guid",
  "currentPoints": 100,
  "lifetimePoints": 200,
  "currentTier": "Silver",
  "totalSpent": 500000,
  "totalVisits": 5
}
```

### GET `/api/loyalty/customers/{customerId}/points/history?page=1&pageSize=20`
Response item:
```json
{
  "id": "guid",
  "customerId": "guid",
  "bookingId": "guid",
  "washHistoryId": "guid",
  "redemptionId": null,
  "points": 8,
  "originalPoints": 8,
  "remainingPoints": 8,
  "balanceAfter": 108,
  "type": "Earn",
  "expiryDate": "2027-07-01T00:00:00Z",
  "idempotencyKey": "wash:guid:earn",
  "description": "Points earned from completed wash",
  "createdAt": "2026-07-01T00:00:00Z"
}
```

### GET `/api/loyalty/wash-history?customerId={guid}&page=1&pageSize=20`
Neu khong truyen `customerId`, tra tat ca history.

### GET `/api/loyalty/rewards?page=1&pageSize=20&includeInactive=false`
Response item:
```json
{
  "id": "guid",
  "name": "Voucher 50k",
  "description": "Discount",
  "type": "FixedDiscount",
  "pointsRequired": 100,
  "value": 50000,
  "serviceId": null,
  "validFrom": "2026-07-01T00:00:00Z",
  "validTo": "2026-12-31T00:00:00Z",
  "usageLimitPerCustomer": 1,
  "status": "Active",
  "createdAt": "2026-07-01T00:00:00Z"
}
```

### GET `/api/loyalty/rewards/{id}`

### POST `/api/loyalty/rewards`
Request:
```json
{
  "name": "Voucher 50k",
  "description": "Discount",
  "type": "FixedDiscount",
  "pointsRequired": 100,
  "value": 50000,
  "serviceId": null,
  "validFrom": "2026-07-01T00:00:00Z",
  "validTo": "2026-12-31T00:00:00Z",
  "usageLimitPerCustomer": 1,
  "isActive": true
}
```

Allowed type: `FixedDiscount`, `PercentageDiscount`, `FreeService`, `AddOnService`.

### PUT `/api/loyalty/rewards/{id}`
Same body as create.

### DELETE `/api/loyalty/rewards/{id}`

### POST `/api/loyalty/rewards/{id}/redeem`
Request:
```json
{
  "customerId": "guid",
  "bookingId": "guid",
  "idempotencyKey": "client-generated-key"
}
```

### POST `/api/loyalty/customers/{customerId}/tier/evaluate`
Manual evaluate tier.

### GET `/api/loyalty/customers/{customerId}/tier/history?page=1&pageSize=20`

### GET `/api/loyalty/dashboard?fromDate=2026-07-01&toDate=2026-07-31`
Response:
```json
{
  "activeCustomers": 10,
  "activeRewards": 3,
  "pointsIssued": 1000,
  "pointsRedeemed": 200,
  "revenue": 5000000,
  "completedWashes": 50
}
```

## Promotions

Route prefix: `/api/loyalty/promotions`.

### GET `/api/loyalty/promotions?page=1&pageSize=20&includeInactive=false`
Response item:
```json
{
  "id": "guid",
  "name": "Summer Sale",
  "code": "SUMMER10",
  "description": "10% off",
  "type": "PercentageDiscount",
  "value": 10,
  "maxDiscountAmount": 50000,
  "bonusPoints": 0,
  "freeServiceId": null,
  "minimumSpend": 100000,
  "startDate": "2026-07-01T00:00:00Z",
  "endDate": "2026-07-31T23:59:59Z",
  "minTierId": null,
  "totalUsageLimit": 100,
  "usageLimitPerCustomer": 1,
  "priority": 1,
  "isStackable": false,
  "status": "Active",
  "createdAt": "2026-07-01T00:00:00Z",
  "serviceIds": ["guid"]
}
```

### GET `/api/loyalty/promotions/{id}`

### POST `/api/loyalty/promotions`
Request:
```json
{
  "name": "Summer Sale",
  "code": "SUMMER10",
  "description": "10% off",
  "type": "PercentageDiscount",
  "value": 10,
  "maxDiscountAmount": 50000,
  "bonusPoints": 0,
  "freeServiceId": null,
  "minimumSpend": 100000,
  "startDate": "2026-07-01T00:00:00Z",
  "endDate": "2026-07-31T23:59:59Z",
  "minTierId": null,
  "totalUsageLimit": 100,
  "usageLimitPerCustomer": 1,
  "priority": 1,
  "isStackable": false,
  "isActive": true,
  "serviceIds": ["guid"]
}
```

Allowed type: `PercentageDiscount`, `FixedDiscount`, `FreeService`, `BonusPoints`.

### PUT `/api/loyalty/promotions/{id}`
Same body as create.

### DELETE `/api/loyalty/promotions/{id}`

### POST `/api/loyalty/promotions/{id}/send`
Request:
```json
{
  "customerIds": ["guid"],
  "expiresAt": "2026-07-31T23:59:59Z"
}
```

Response:
```json
{ "promotionId": "guid", "sentCount": 1, "skippedCount": 0 }
```

### POST `/api/loyalty/promotions/{id}/apply`
Request:
```json
{
  "bookingId": "guid",
  "customerId": "guid",
  "code": "SUMMER10"
}
```

Response:
```json
{
  "bookingId": "guid",
  "promotionId": "guid",
  "discountAmount": 50000,
  "bonusPoints": 0,
  "totalBeforeDiscount": 150000,
  "totalAfterDiscount": 100000
}
```

## Wash Histories

Auth theo controller hien co.

### GET `/api/wash-histories/me?page=1&pageSize=10`
Auth `CustomerOnly`.

### GET `/api/wash-histories/me/{washHistoryId}`
Auth `CustomerOnly`.

### GET `/api/wash-histories/customer/{customerId}?page=1&pageSize=10`
Auth `StaffOrAdmin`.

List item:
```json
{
  "washHistoryID": "guid",
  "bookingID": "guid",
  "washDate": "2026-07-01T00:00:00Z",
  "finalAmount": 100000,
  "pointsEarned": 10,
  "customerRating": 5,
  "vehiclePlate": "51A12345",
  "branchName": "District 1",
  "services": ["Basic Wash"]
}
```

## AI

### AI customer intelligence (new)

- `POST /api/ai/customer/assistant` (Customer): read-only personalized service cards, eligible offers, loyalty summary, care tip, and optional low-load slots. Body: `{ "preference":"nhanh", "branchId":"guid?", "date":"2026-07-23?", "serviceIds":["guid"] }`.
- `GET /api/ai/feedback-insights?from={iso}&to={iso}&branchId={guid?}` (BranchManager/Admin): rating distribution, rule-based themes, and prioritized low-rating wash records. A Branch Manager's branch is resolved by the server.
- `POST /api/ai/operations-copilot` (BranchManager/Admin): read-only operational summary. Body: `{ "message":"Doanh thu hôm nay?", "from":"iso", "to":"iso", "branchId":"guid?" }`. Responses include evidence and navigation-only actions; they never mutate bookings, payments, promotions, staffing, or refunds.

All three responses include `source` and `isFallback`; the MVP uses deterministic rules as its safe fallback/source. The client must keep explicit confirmation and existing mutation endpoints for all writes.

Auth theo controller hien co.

### POST `/api/ai/chat`
Auth `CustomerOnly`.

Request:
```json
{ "message": "Toi nen rua xe goi nao?", "conversationId": null }
```

Response:
```json
{
  "reply": "text",
  "conversationId": "id",
  "isFallback": false,
  "source": "gemini"
}
```

### POST `/api/ai/suggest-services`
Auth `CustomerOnly`.

Request:
```json
{ "vehicleType": "Sedan", "preference": "cheap and fast" }
```

Response:
```json
{
  "suggestions": [
    {
      "serviceId": "guid",
      "serviceName": "Basic Wash",
      "price": 80000,
      "reason": "..."
    }
  ],
  "summary": "...",
  "isFallback": false,
  "source": "gemini"
}
```

### POST `/api/ai/admin/chat`
Auth `AdminOnly`.

Request:
```json
{ "message": "Tom tat doanh thu", "conversationId": null }
```

## Admin Users

### GET `/api/admin/users`
Auth `AdminOnly`. Query: `query`, `role`, `status`, `page`, `pageSize`.

`status` supports `Active`, `Inactive`, `Suspended`, and `Deleted`. `Deleted` is returned for audit/read-only use.

### PUT `/api/admin/users/{userId}/status`
Auth `AdminOnly`.

Request:
```json
{ "status": "Active" }
```

The UI may set `Active`, `Inactive`, or `Suspended`. A `Deleted` user is read-only: the API rejects status changes to/from `Deleted`. A user cannot change their own status and the last active Admin cannot be deactivated.

## Behavioral Logs

Route prefix: `/api/admin/behavioral-logs`. Auth `AdminOnly`.

### GET `/api/admin/behavioral-logs?customerID={guid?}&actionType=Book&from={iso?}&to={iso?}&page=1&pageSize=20`

Returns `ApiResponse<PagedResult<BehavioralLogItemDto>>`. Supported `actionType`: `ViewPromotion`, `Book`, `CancelBooking`, `LeaveFeedback`, `RedeemReward`.

### GET `/api/admin/behavioral-logs/export?...`

Uses the same filters and returns a UTF-8 CSV file. The frontend sends the Admin bearer token when downloading.

## Legacy Service Catalog Controller Warning

Controller `API/Controllers/ServicesController.cs` cung route `api/Services` gom:
- `GET /api/Services`
- `GET /api/Services/{id}`
- `POST /api/Services`
- `PUT /api/Services/{id}`
- `PATCH /api/Services/{id}`
- `DELETE /api/Services/{id}`

Do ASP.NET route thuong khong phan biet hoa/thuong, route nay co the trung voi Operations route `/api/services`. FE nen khong dung controller legacy nay cho den khi backend doi route hoac xoa duplicate.

## Operations extension (Spec 002)

Các endpoint mutation trả lỗi RFC Problem Details khi không hợp lệ. Tất cả thời gian là ISO-8601 UTC.

### Booking capacity, multi-service and queue

- `GET /api/bookings/availability?branchId={guid}&serviceId={guid}&date=2026-07-21`: tương thích request cũ cho một dịch vụ.
- `POST /api/bookings/availability`: body `{ "branchId":"guid", "date":"2026-07-21", "items":[{ "serviceId":"guid", "quantity":1 }] }`; trả `durationMinutes` và `slots` gồm `startTime`, `endTime`, `availableBayCount`. Dùng endpoint này cho booking nhiều dịch vụ/add-on.
- Response booking có thêm `items`, mỗi item có `serviceId`, `serviceName`, `quantity`, `unitPrice`, `lineTotal`, `durationMinutesPerUnit`; frontend không dùng riêng snapshot của dịch vụ đầu tiên cho booking nhiều dịch vụ.
- `POST /api/bookings/{id}/reschedule`: customer sở hữu booking pending hoặc staff/admin. Body: `{ "bookingStartTime": "...", "washBayId": "guid|null", "note": "..." }`.
- `POST /api/bookings/{id}/check-in`, `POST /api/bookings/{id}/no-show`, `POST /api/bookings/{id}/dispatch`: quyền `StaffOrAdmin`. Dispatch body: `{ "washBayId": "guid" }`.
- `GET /api/bookings/queue?branchId={guid}&washBayId={guid?}`: quyền `StaffOrAdmin`; trả vị trí, ETA và priority của booking đã check-in.
- `POST /api/bookings/{id}/assigned-staff`: quyền `StaffOrAdmin`; body `{ "staffUserId":"guid", "expectedVersion": 0 }`. Staff phải check-in trong ca active tương thích branch/thời gian/bãi và chưa check-out; response trả `assignedStaffId` và `version`.
- `GET /api/bookings/{id}/reschedule-history`: customer sở hữu booking hoặc staff/admin; trả audit đổi lịch.

### Staffing

Mọi endpoint staffing yêu cầu đăng nhập staff/admin; tạo, sửa, deactivate và phân công yêu cầu `AdminOnly`.

- `GET /api/staffing/shifts?branchId={guid?}&from={date?}&to={date?}`
- `GET /api/staffing/shifts/{id}`
- `POST /api/staffing/shifts`: `{ "branchId":"guid", "startsAt":"...", "endsAt":"...", "name":"Ca sáng" }`
- `PUT /api/staffing/shifts/{id}`: cùng thời gian/tên và `isActive`.
- `DELETE /api/staffing/shifts/{id}` chỉ deactivate, không xóa lịch sử.
- `POST /api/staffing/shifts/{id}/assignments`: `{ "userId":"guid", "washBayId":"guid|null" }`
- `DELETE /api/staffing/shifts/{id}/assignments/{assignmentId}`
- `GET /api/staffing/shifts/available?branchId={guid}&startsAt={iso}&endsAt={iso}`: trả các nhân viên đang hoạt động chưa có ca active bị chồng trong khoảng thời gian yêu cầu; dùng cho picker phân công ca. `branchId` là phạm vi của ca sẽ tạo.
- `GET /api/staffing/shifts/{id}/capacity`: trả số staff/bãi được gán và tổng bãi active của branch.

Backend chặn staff inactive, ca chồng giờ, bay không thuộc branch hoặc inactive.

### Attendance

Staff/Admin can view their own attendance; only Admin can list, adjust, lock, reopen, or report attendance.

- `GET /api/attendance/me?date=YYYY-MM-DD`
- `POST /api/attendance/assignments/{assignmentId}/check-in`
- `POST /api/attendance/assignments/{assignmentId}/check-out`
- `GET /api/attendance?branchId={guid?}&from={iso}&to={iso}&staffId={guid?}&status={status?}` (Admin)
- `PATCH /api/attendance/{id}` (Admin): `{ "checkedInAt":"iso?", "checkedOutAt":"iso?", "status":"CheckedOut?", "reason":"required", "adminNote":"optional" }`
- `POST /api/attendance/{id}/lock` and `POST /api/attendance/{id}/reopen` (Admin): `{ "reason":"required" }`
- `GET /api/attendance/summary?branchId={guid?}&from={iso}&to={iso}&groupBy=staff|day` (Admin)

Self-service timestamps are set by the server in UTC. One attendance record may exist per shift assignment; repeated check-in/out requests return the existing record. Attendance records prevent destructive shift/assignment changes.

### Payment refund and reconciliation

Các endpoint payment yêu cầu `StaffOrAdmin`; báo cáo đối soát yêu cầu `AdminOnly`.

- `POST /api/payments/{paymentId}/refunds`: `{ "amount": 50000, "reason": "Khách hủy dịch vụ", "referenceNumber": "optional" }`. Chỉ hoàn trên payment `Paid` và không vượt số tiền chưa hoàn.
- `GET /api/payments/refunds/{refundId}`
- `GET /api/payments?branchId={guid?}&status=Paid&from={iso?}&to={iso?}&page=1&pageSize=20`: trả payment cùng `refundedAmount` và `refundableAmount` cho form refund.
- `GET /api/payments/reconciliation?from=2026-07-01T00:00:00Z&to=2026-07-31T23:59:59Z&branchId={guid?}`: trả `paidAmount`, `refundedAmount`, `netAmount`, `paymentCount`.
- `GET /api/payments/reconciliation/export?...`: tải file CSV báo cáo tổng hợp; quyền `AdminOnly`.

Refund hiện được staff/admin ghi nhận nội bộ. Checkout online, xác thực webhook, idempotency key và adapter VNPay/MoMo chưa có vì chưa chọn nhà cung cấp sandbox.

### Reward voucher on booking

- `POST /api/loyalty/me/reward-redemptions/{redemptionId}/apply`: body `{ "bookingId": "guid" }`. Chỉ áp dụng reward redemption `Reserved`, chưa hết hạn, thuộc customer và booking pending. Response có `discountAmount`, `totalBeforeDiscount`, `totalAfterDiscount`.
- `DELETE /api/loyalty/me/reward-redemptions/{redemptionId}/bookings/{bookingId}`: bỏ reward khỏi booking pending, khôi phục tổng tiền và đưa redemption về `Reserved`.

Reward `FixedDiscount`, `PercentageDiscount`, `FreeService` và `AddOnService` được tính theo giá snapshot trong `BookingDetail`. Mỗi booking hiện chỉ nhận một reward voucher; promotion vẫn là luồng độc lập.

### Customer feedback for operations

- `GET /api/feedback?from=2026-07-01&to=2026-07-31&rating=1&branchId={guid?}&page=1&pageSize=20`
- Roles: `Staff`, `BranchManager`, and `Admin` only. Staff receives records from active staff-membership branches, Branch Manager receives managed-branch records, and Admin may view all branches or filter one branch.
- Each item returns the rating, full feedback text, wash date, service names, branch name, and `staffMembers` (`staffUserId`, `staffName`, `workRole`). `staffMembers` uses the recorded booking staff work and falls back to the legacy assigned staff member.
- A non-Admin `branchId` outside the server-resolved role scope returns `403`; a Customer is denied access.

### Role-scoped wash history

- `GET /api/wash-histories/operations?from=2026-07-01&to=2026-07-31&search={customer-name-or-plate?}&branchId={guid?}&page=1&pageSize=20`
- Roles: `BranchManager` and `Admin` only. A Branch Manager receives only records from active managed branches. An Admin receives all branches when `branchId` is omitted, or one selected branch when supplied.
- A manager request for a branch outside their managed scope returns `403`; unknown branches return `404`; invalid date ranges return `400`.
- A date-only `to` value includes the complete selected day.
- Each item contains only operational fields: `washHistoryId`, `washDate`, `customerName`, `vehiclePlate`, `branchName`, wash totals, service names, `staffMembers`, rating, and feedback. Customer email, phone number, payment references, and unrelated history are never returned.
- `staffMembers` returns recorded work allocation (`staffUserId`, `staffName`, `workRole`, `contributionPercent`) and falls back to legacy assigned staff if no allocation was recorded.

