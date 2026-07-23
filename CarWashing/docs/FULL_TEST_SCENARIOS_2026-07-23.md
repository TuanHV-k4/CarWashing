# AutoWash Pro — Bộ kịch bản test đầy đủ

Tài liệu này được lập từ mã nguồn hiện tại (React/Vite + ASP.NET Core .NET 8), không chỉ từ tài liệu API cũ. Dùng cho manual E2E, API integration và làm backlog automation.

## 1. Phạm vi và quy ước

- Vai trò: `Customer`, `Staff`, `BranchManager`, `Admin`.
- Mã **P0** là luồng cần chạy trước release; **P1** là nghiệp vụ quan trọng; **P2** là biên, bảo mật, UX và độ bền.
- Mỗi case API cần kiểm tra: HTTP status, schema/body, dữ liệu DB bị ảnh hưởng, không thay đổi dữ liệu khi lỗi, và behavioral log nếu nghiệp vụ có ghi log.
- Thay `A/B` bằng hai người dùng, hai branch hoặc hai booking khác nhau để kiểm thử cô lập dữ liệu/tenant.
- Tất cả thời gian gửi ISO-8601 UTC. Kiểm tra hiển thị FE theo múi giờ người dùng, không bị lệch ngày/ca.

## 2. Dữ liệu và tiền điều kiện chung

Tạo tối thiểu: 2 branch mở (B1/B2), 1 branch đóng, 2 wash bay active ở B1, 1 bay inactive, 2 service active (30 và 60 phút), 1 service inactive, 2 staff B1, 1 staff B2, 1 BranchManager B1, hai customer A/B, reward, tier và promotion hợp lệ. Chuẩn bị thêm booking ở từng trạng thái.

| Mã | Thực thể | Trạng thái dùng để test |
|---|---|---|
| D1 | Booking | `Pending`, `Confirmed`, `CheckedIn`, `InProgress`, `Completed`, `Cancelled`, `NoShow` |
| D2 | Payment | `Pending`, `Paid`, `Voided`, có/không có refund |
| D3 | Attendance | chưa check-in, đang làm, đã check-out, locked |
| D4 | Account | active, inactive/deactivated, email chưa xác minh, token cũ/mới |

## 3. Luồng P0 end-to-end bắt buộc

| Mã | Luồng | Bước chính | Kết quả cuối cần chứng minh |
|---|---|---|---|
| E2E-01 | Đặt lịch đến hoàn tất | Customer tạo lịch → Manager confirm → check-in → dispatch bay → start → complete | Booking `Completed`; wash history; payment/loyalty/dashboard nhất quán |
| E2E-02 | Đổi lịch | Customer đặt lịch → lấy availability → reschedule với version đúng | Giờ mới, version/history được cập nhật; slot cũ được trả lại |
| E2E-03 | Hủy và hoàn tiền | Payment paid → booking cancel → Admin/Staff tạo refund hợp lệ → reconciliation | Không vượt số tiền có thể hoàn; ledger/báo cáo chính xác |
| E2E-04 | Nhân sự vận hành | Admin gán staff vào B1 → tạo ca → assign staff → staff check-in → Manager gán staff cho booking | Chỉ nhân viên đang có mặt, đúng branch/ca/bay được gán |
| E2E-05 | Loyalty | Admin tạo campaign/reward → Customer đổi điểm/áp dụng voucher cho booking → hoàn tất booking | Điểm, voucher/redemption, giá thanh toán và analytics khớp |
| E2E-06 | Quản trị người dùng | Admin đổi role/trạng thái và gán BranchManager | Token cũ bị vô hiệu khi đổi role/status; manager chỉ thấy B1 |

## 4. Authentication, account và session

| ID | Priority | Kịch bản | Kết quả mong đợi |
|---|---|---|---|
| AUTH-01 | P0 | Đăng ký customer với username/email/phone mới, password & confirm hợp lệ | `201`; tạo User + Customer, role Customer; không lộ password/OTP |
| AUTH-02 | P1 | Đăng ký trùng username, email (không phân biệt hoa thường), phone đã chuẩn hóa | `400`; đúng field lỗi; không tạo bản ghi một phần |
| AUTH-03 | P1 | Đăng ký thiếu field, email/phone/password sai format, password-confirm khác | Validate FE và BE; không gọi API khi FE bắt lỗi cơ bản |
| AUTH-04 | P1 | Verify email bằng OTP hợp lệ | Account được marked verified, đăng nhập theo chính sách cho phép |
| AUTH-05 | P2 | OTP sai, hết hạn, verify hai lần, resend với email không tồn tại | Thông báo an toàn; không xác minh sai tài khoản |
| AUTH-06 | P0 | Login đúng cho từng role | JWT có user/role/auth_version đúng; FE điều hướng đúng dashboard role |
| AUTH-07 | P0 | Login sai mật khẩu/username, account inactive/deleted | `401`; không có token mới; thông báo không tiết lộ dữ liệu nhạy cảm |
| AUTH-08 | P1 | Refresh trang sau login và gọi `/api/Auth/me` | Session còn hợp lệ, UI khôi phục user/route đúng |
| AUTH-09 | P0 | Logout rồi truy cập route/API bảo vệ | localStorage bị xóa; FE về login; API trả 401 |
| AUTH-10 | P1 | Dùng token hết hạn, token bị sửa, thiếu Bearer | 401; không xuất stack trace |
| AUTH-11 | P0 | Đổi role/status hoặc reset password rồi gọi API bằng token cũ | Bị từ chối vì `auth_version` đổi; login mới dùng token mới được |
| AUTH-12 | P0 | Forgot password với email tồn tại/không tồn tại | Response không cho phép dò email; email/token đúng chỉ được tạo khi có account |
| AUTH-13 | P0 | Reset password token hợp lệ | Password mới login được; token reset chỉ dùng một lần; token cũ vô hiệu |
| AUTH-14 | P1 | Reset password token sai/hết hạn, password không đạt rule, confirm khác | `400`; password và auth version không đổi |
| AUTH-15 | P2 | Gửi liên tiếp forgot/reset quá giới hạn | Nhận `429` theo IP; sau cửa sổ giới hạn có thể thử lại |

## 5. Phân quyền, cô lập dữ liệu và route guard

| ID | Priority | Kịch bản | Kết quả mong đợi |
|---|---|---|---|
| ACL-01 | P0 | Anonymous gọi mọi endpoint protected | 401; riêng endpoint public vẫn hoạt động đúng |
| ACL-02 | P0 | Customer gọi admin/staff/manager endpoint | 403, không trả dữ liệu nội bộ |
| ACL-03 | P0 | Staff gọi endpoint AdminOnly; BranchManager gọi AdminOnly | 403 |
| ACL-04 | P0 | Customer A đọc/sửa xe, profile, booking, history của B bằng ID | 403/404; không có dữ liệu hoặc mutation chéo |
| ACL-05 | P0 | Manager B1 xác nhận/dispatch/gán người cho booking B2 | 403; manager B1 chỉ nhìn B1 trên endpoint manager |
| ACL-06 | P1 | Staff thao tác booking/payment ngoài scope branch được phép | Bị chặn theo controller/service scope |
| ACL-07 | P1 | FE mở trực tiếp `/customer/*`, `/operations/*`, `/manager/*`, `/admin/*` sai role | Redirect về workspace hợp lệ hoặc login; không nhấp nháy dữ liệu sai |
| ACL-08 | P2 | Thử query paging/filter/ID bất thường để vượt scope | Scope vẫn được áp dụng server-side |

## 6. Hồ sơ customer, xe và lịch sử rửa

| ID | Priority | Kịch bản | Kết quả mong đợi |
|---|---|---|---|
| CUS-01 | P1 | Xem `/customers/me`, sửa full name/email/phone hợp lệ | UI/API phản ánh dữ liệu mới; email unique |
| CUS-02 | P1 | Sửa profile email đã dùng bởi user khác, chuỗi rỗng/dài/format sai | Lỗi validation; dữ liệu cũ không đổi |
| VEH-01 | P0 | Customer thêm xe hợp lệ với biển số mới | Xe hiện ở `/vehicles/me`; owner là customer hiện tại |
| VEH-02 | P1 | Biển số trùng (khác hoa/thường/khoảng trắng), type/field sai | Lỗi; không duplicate xe |
| VEH-03 | P1 | Sửa xe của mình và đổi `Active`/`Inactive` | Chỉ các field cho phép đổi; trạng thái cập nhật |
| VEH-04 | P0 | A sửa xe B hoặc tạo booking bằng xe inactive/xe B | Bị từ chối; không tạo booking |
| HIST-01 | P0 | Hoàn tất booking rồi customer xem `/wash-histories/me` và detail | Có đúng service, vehicle, branch, thời gian, tổng tiền; chỉ lịch sử của mình |
| HIST-02 | P1 | Customer gửi feedback rating 1..5 một lần | Rating/feedback được lưu và manager/admin insight phản ánh nếu áp dụng |
| HIST-03 | P1 | Rating 0/6, feedback quá biên, submit lần hai, xem history người khác | 400/403/404; feedback không bị ghi đè |
| HIST-04 | P1 | Staff/Admin xem lịch sử theo customer hợp lệ/không tồn tại | Có phân trang/filter đúng, không vượt quyền |

## 7. Danh mục vận hành: branch, service, wash bay

| ID | Priority | Kịch bản | Kết quả mong đợi |
|---|---|---|---|
| CAT-01 | P0 | Admin tạo/sửa/xóa (soft deactivate nếu thiết kế) branch | Danh sách/detail đúng; chỉ Admin mutation |
| CAT-02 | P1 | Branch có giờ mở >= giờ đóng, field thiếu, branch không mở | 400; không tạo/sửa lỗi |
| CAT-03 | P1 | Branch inactive/closed xuất hiện ở luồng public/booking | Không thể chọn/tạo lịch; list includeInactive chỉ khi yêu cầu/được phép |
| CAT-04 | P0 | Admin tạo/sửa/deactivate service hợp lệ (price > 0, duration 10..240) | Catalog và luồng booking cập nhật |
| CAT-05 | P1 | Service price <=0, duration ngoài biên, name/description quá dài | 400; không persist |
| CAT-06 | P1 | Tạo/sửa/deactivate wash bay ở branch mở | Bay thuộc đúng branch; status đổi đúng |
| CAT-07 | P1 | Tạo bay cho branch inactive, dispatch vào bay inactive/khác branch | 400; booking không đổi bay |
| CAT-08 | P2 | Phân trang/filter/cờ includeInactive catalog | `items/page/pageSize/totalCount` nhất quán; không lặp/mất bản ghi |

## 8. Booking, availability, queue và trạng thái

| ID | Priority | Kịch bản | Kết quả mong đợi |
|---|---|---|---|
| BOOK-01 | P0 | Customer tải form/availability với xe, B1, service và thời gian hợp lệ | Chỉ trả slot/bay có thể dùng; giá/duration khớp service |
| BOOK-02 | P0 | Tạo booking hợp lệ một/nhiều service trong giờ mở cửa | `Pending`, tổng tiền/end time chính xác, customer/vehicle/branch đúng |
| BOOK-03 | P0 | Tạo booking ngoài giờ, quá khứ, branch/service/bay inactive, xe không thuộc owner | 400; không tạo booking/payment phụ |
| BOOK-04 | P1 | Hai customer đồng thời chọn cùng slot/bay | Tối đa một booking giữ tài nguyên; request còn lại conflict/slot khác hợp lệ |
| BOOK-05 | P1 | Query availability thiếu/sai interval, start >= end, branch không tồn tại | 400/404 rõ ràng, không trả slot sai |
| BOOK-06 | P0 | Customer hủy booking `Pending`/`Confirmed` theo rule | `Cancelled`; không thể tiếp tục vận hành; voucher/reward được xử lý theo rule |
| BOOK-07 | P1 | Hủy booking completed/cancelled/no-show hoặc customer A hủy B | Bị chặn; không có transition sai |
| BOOK-08 | P0 | Customer reschedule booking mở với `expectedVersion` đúng | Start/end mới hợp lệ, version tăng, reschedule-history có actor/lý do/thời gian |
| BOOK-09 | P0 | Reschedule với version cũ (race), sang slot trùng, ngoài giờ, booking đóng | `409` cho version/trùng; `400` cho rule; bản ghi gốc nguyên vẹn |
| BOOK-10 | P1 | Manager/Admin confirm `Pending` đúng branch | Thành `Confirmed`; khách/board/dashboard cập nhật |
| BOOK-11 | P1 | Confirm lần hai hoặc confirm status khác `Pending` | 400; không tạo side effect |
| BOOK-12 | P0 | Staff/manager check-in booking `Confirmed` | Thành `CheckedIn`, có `checkInAt` |
| BOOK-13 | P1 | Check-in khi Pending/Completed/Cancelled/NoShow | 400 |
| BOOK-14 | P0 | Manager dispatch Confirmed/CheckedIn vào bay active, trống, cùng branch | Bay gán thành công; queue/board thay đổi đúng |
| BOOK-15 | P0 | Dispatch hai booking overlap cùng bay; bay khác branch/inactive; booking sai trạng thái | Conflict `409` hoặc 400; không overwrite bay |
| BOOK-16 | P0 | Start từ trạng thái hợp lệ và complete từ `InProgress` | Transition đúng; timestamp/side effect hoàn tất được tạo một lần |
| BOOK-17 | P1 | Start/complete lặp lại hoặc nhảy qua trạng thái | 400; không duplicate history/points/payment effects |
| BOOK-18 | P1 | Staff/Admin mark no-show đúng điều kiện; thử no-show booking closed | Đúng trạng thái và audit; status không hợp lệ bị từ chối |
| BOOK-19 | P1 | Lấy queue/filter ngày/branch/status và paging | Thứ tự/đếm/branch scope đúng; không có booking đóng trong queue nếu rule loại trừ |
| BOOK-20 | P2 | ID không tồn tại/malformed, page/pageSize cực trị, note/reason dài | 404/400 an toàn; không 500 |

## 9. Nhân sự, branch membership, ca và phân công booking

| ID | Priority | Kịch bản | Kết quả mong đợi |
|---|---|---|---|
| HR-01 | P0 | Admin đổi role Staff/BranchManager và gán membership branch | Role/membership hiện đúng; token cũ bị invalid |
| HR-02 | P0 | Gán BranchManager B1 khi manager đã active ở B2 | Bị chặn: một manager chỉ có một assignment active |
| HR-03 | P1 | End membership staff và deactivate membership manager | Không còn quyền/scope sau ngày hiệu lực; history được giữ |
| HR-04 | P1 | Gán membership cho account inactive/role không phù hợp/trùng overlap | 400/409; không có membership sai |
| HR-05 | P0 | Tạo/sửa/deactivate ca B1 hợp lệ | Ca xuất hiện đúng branch; không nhận phân công mới khi inactive |
| HR-06 | P1 | Ca start >= end, staff không thuộc B1, overlap/duplicate assignment | 400/409 theo rule; không tạo assignment lỗi |
| HR-07 | P0 | `available staff` cho khoảng ca trả đúng người không có xung đột | Chỉ staff active, membership hợp lệ và available |
| HR-08 | P0 | Staff check-in một assignment hợp lệ | Attendance mở, thời điểm/nguồn chính xác; staff nhìn thấy ở `/attendance/me` |
| HR-09 | P1 | Check-in lặp, check-out trước check-in, check-in assignment người khác/ngoài thời điểm | Bị từ chối; attendance không sai |
| HR-10 | P0 | Staff check-out attendance đang mở | Có `checkedOutAt`, không còn đủ điều kiện phân công booking |
| HR-11 | P0 | Admin xem attendance, adjust giờ/lý do, lock và reopen | Audit adjustment/lock state đúng; locked record không bị sửa trái phép |
| HR-12 | P1 | Adjust thiếu lý do/thời gian nghịch lý; staff thử admin attendance endpoint | 400/403 |
| HR-13 | P0 | Manager xem eligible staff cho booking và gán 1 staff | Chỉ staff B1, đã check-in, ca active bao phủ booking và compatible bay được chọn |
| HR-14 | P0 | Gán staff cho booking bằng staff chưa check-in, B2, ca không phủ giờ, bay không tương thích | 400; `AssignedStaffID` không đổi |
| HR-15 | P0 | Lưu staff-work nhiều người: từng contribution 0..100, không lặp, tổng 100 | Lưu đúng list/tỷ lệ; kết quả workload phản ánh |
| HR-16 | P1 | Staff-work tổng 99/101, duplicate, staff không eligible, booking closed/version cũ | 400/409; không cập nhật một phần |
| HR-17 | P1 | Manager B1 gán staff/ca/booking B2 | 403 |

## 10. Payment, refund và reconciliation

| ID | Priority | Kịch bản | Kết quả mong đợi |
|---|---|---|---|
| PAY-01 | P0 | Tạo payment cho booking với amount đúng tổng, method Cash/Card/BankTransfer | `Pending`, amount/method/booking đúng |
| PAY-02 | P0 | Amount 0/âm/khác total, method sai, booking không tồn tại | 400/404; không tạo payment |
| PAY-03 | P0 | Mark payment pending là paid | `Paid`, paid timestamp/actor; reconciliation cập nhật |
| PAY-04 | P1 | Mark paid lần hai hoặc paid payment đã void | 400, trạng thái không sai |
| PAY-05 | P0 | Void payment pending | `Voided`; không thể mark paid lại |
| PAY-06 | P1 | Void payment paid | 400; yêu cầu refund thay vì void |
| PAY-07 | P0 | Tạo refund cho paid payment với amount hợp lệ và reason <=500 | Refund record/amount/reason/status đúng; tổng refund không vượt payment |
| PAY-08 | P0 | Refund 0/âm, reason rỗng/dài, payment pending/void, cumulative refund vượt paid | 400; không tạo refund sai |
| PAY-09 | P1 | Đọc payment/refund của branch khác và filter status/date/paging | Scope/filters/totalCount đúng |
| PAY-10 | P0 | Reconciliation từ <= to, export | Tổng paid/void/refund/net và item list khớp DB; export tải đúng content/type |
| PAY-11 | P1 | Reconciliation `from > to`, time zone boundary, dataset rỗng | 400 hoặc zero result hợp lệ; không lệch ngày |

## 11. Loyalty, tier, reward, voucher và promotion campaign

| ID | Priority | Kịch bản | Kết quả mong đợi |
|---|---|---|---|
| LOY-01 | P0 | Customer xem overview, vouchers và catalog reward | Chỉ dữ liệu hiện hành của customer; points/tier/expiry đúng |
| LOY-02 | P0 | Customer redeem reward đủ điểm | Tạo redemption/voucher; ledger trừ đúng điểm chưa hết hạn; không double-spend |
| LOY-03 | P0 | Redeem thiếu điểm, reward inactive/không tồn tại, gửi đồng thời hai request | 400/404/conflict; balance không âm và chỉ một giao dịch hợp lệ |
| LOY-04 | P0 | Apply promotion/voucher/reward-redemption của chính mình vào booking mở | Discount/booking total/usage đúng; voucher được đánh dấu theo rule |
| LOY-05 | P0 | Apply promotion expired, hết quota, sai điều kiện tier/service/branch, booking closed hoặc thuộc B | Bị từ chối; không giảm giá/đánh dấu usage |
| LOY-06 | P1 | Gỡ promotion/reward khỏi booking trước khi hoàn tất | Total và trạng thái voucher/redemption được rollback đúng; không gỡ chéo customer |
| LOY-07 | P0 | Admin CRUD tier (active/inactive), delete tier đang dùng | Validation/reference integrity đúng; tier inactive không cấp mới nếu rule cấm |
| LOY-08 | P1 | Admin evaluate tier và xem tier history | Tier đổi đúng threshold/mode; history có reason; gọi lại idempotent khi không đổi |
| LOY-09 | P0 | Hoàn tất booking hợp lệ | Chỉ tạo một wash history/ledger earning; balance/lifetime/tier/dashboard cập nhật đồng bộ |
| LOY-10 | P1 | Job expiry/maintenance chạy qua điểm quá hạn | Điểm hết hạn bị loại/ledger ghi đúng; không làm âm balance |
| LOY-11 | P0 | Admin CRUD reward/promotion và preview audience | Field/range/date/quota valid; preview không gửi campaign/không tạo usage |
| LOY-12 | P0 | Send promotion theo segment | Snapshot audience được lưu; chỉ người hợp lệ nhận voucher; resend/duplicate theo rule không nhân đôi sai |
| LOY-13 | P1 | Segments at-risk/expiring/loyal có inactiveDays/branch/tier filters | Số người và từng record đúng tiêu chí; clamp boundary 1..3650 |
| LOY-14 | P1 | Promotion analytics/customer 360/usage | Counts, discount, revenue, voucher, ledger và lịch sử khớp raw data/scope |

## 12. Dashboard, báo cáo, log và AI

| ID | Priority | Kịch bản | Kết quả mong đợi |
|---|---|---|---|
| REP-01 | P0 | Admin dashboard theo date range/branch | KPI booking/revenue/payment/loyalty khớp dữ liệu gốc và filter |
| REP-02 | P0 | Manager dashboard/branch context | Chỉ B1; không cho truyền B2 để lấy dữ liệu ngoài scope |
| REP-03 | P1 | Workload report theo khoảng ngày/branch | Công việc và contribution của staff đúng; range lỗi bị chặn |
| REP-04 | P1 | Attendance summary | Tổng check-in/out/absent/adjusted phản ánh records và filter |
| REP-05 | P0 | Admin behavioral logs filter, paging, export | Log actor/action/entity/date đúng; export khớp result filter |
| REP-06 | P1 | Non-admin truy cập logs/export | 403 |
| AI-01 | P1 | Customer AI chat/suggest/customer assistant với input hợp lệ | Response có schema an toàn, không lộ customer khác; UI loading/error hoạt động |
| AI-02 | P1 | Admin/manager feedback insights và operations copilot | Scope branch/role đúng; insight dựa dữ liệu được phép |
| AI-03 | P0 | Gửi quá rate limit AI customer/admin | `429 application/problem+json`; UI báo thử lại, request sau window hoạt động |
| AI-04 | P2 | Prompt rỗng/dài/hostile, Gemini/provider timeout/error | Validate/graceful error, không 500/không lộ secret; không phá UI |

## 13. FE/UX, resilience và bảo mật ngang hệ thống

| ID | Priority | Kịch bản | Kết quả mong đợi |
|---|---|---|---|
| UX-01 | P0 | Mọi trang đã implement: loading, empty, error + retry | Không crash; retry không gửi mutation lặp |
| UX-02 | P0 | Nút mutation click double/refresh trong lúc pending | Disabled/loading; backend idempotency hoặc conflict an toàn |
| UX-03 | P1 | Form booking, profile, auth, payment với validation FE rồi bypass bằng API | FE giúp người dùng; BE vẫn là lớp quyết định cuối |
| UX-04 | P1 | Responsive 320px/tablet/desktop, keyboard tab/Enter/Esc, focus modal/drawer | Có thể thao tác không chuột; focus/aria/label không mất |
| UX-05 | P1 | Nội dung tiếng Việt, số tiền, timezone, ngày cuối tháng/năm/DST (nếu môi trường) | Không lỗi encoding, format và ngày giờ nhất quán |
| SEC-01 | P0 | XSS ở name/note/feedback/promotion/AI; HTML trong response | UI escape text; không thực thi script |
| SEC-02 | P0 | SQL/NoSQL-like injection ở search/filter/query, UUID bất thường | 400 an toàn hoặc search literal; không 500/dump DB |
| SEC-03 | P1 | CORS: origin được phép/không được phép, credentials/header Authorization | Chỉ origin cấu hình truy cập được; preflight hợp lệ |
| SEC-04 | P1 | Lỗi validation/not found/conflict/server | ProblemDetails nhất quán; không lộ connection string, stack trace, token |
| PERF-01 | P2 | 100+ booking, paging lớn, concurrent availability/dispatch/redeem | Response trong SLA thống nhất của nhóm; không duplicate/oversell |

## 14. Ma trận trạng thái booking để kiểm thử transition

| Từ trạng thái | Action hợp lệ cần test | Action phải bị chặn |
|---|---|---|
| Pending | cancel, reschedule, confirm | check-in/start/complete/dispatch/no-show nếu rule không cho |
| Confirmed | cancel, reschedule (nếu rule), check-in, dispatch | confirm lần nữa, complete trực tiếp |
| CheckedIn | dispatch, start, cancel theo rule | confirm lại, complete trực tiếp |
| InProgress | complete | reschedule, confirm, check-in, dispatch lại |
| Completed | chỉ đọc/history/feedback/refund | mọi transition booking/staff-work |
| Cancelled | chỉ đọc/refund theo rule | mọi transition/assign staff |
| NoShow | chỉ đọc/financial handling theo rule | confirm/start/complete/reschedule |

## 15. Lưu ý về coverage FE hiện tại

- Route đã hiện thực có customer, operations, manager, admin, đăng ký/đặt lại mật khẩu. Một số bề mặt cũ trong `FE_BE_DETAILED_TEST_FLOWS.md` không còn khớp route hiện tại.
- Một số endpoint backend chưa có UI đầy đủ (ví dụ CRUD mutation branch trong drawer hiện chỉ đóng form, payment/refund chi tiết, một số admin loyalty). Các case này vẫn phải chạy bằng Swagger/Postman/API integration cho đến khi FE được nối.
- Có hai controller cùng route `api/services` (catalog legacy và Operations Services), là rủi ro ambiguous endpoint. Chạy `CAT-04/05` qua Swagger ngay trong môi trường deploy để xác nhận route thật; nên có regression test bắt ambiguity/HTTP 500.

## 16. Tiêu chí pass release tối thiểu

1. Toàn bộ P0 pass trên môi trường gần production, gồm sáu E2E ở mục 3.
2. Không có lỗi Critical/High còn mở ở auth, ACL, booking concurrency, payment/refund, loyalty balance hay cross-branch access.
3. API trả 401/403/400/404/409/429 đúng loại, không tạo side effect ở test thất bại.
4. P1 có kết quả pass hoặc exception đã được product owner chấp nhận; P2 tạo backlog có owner.
