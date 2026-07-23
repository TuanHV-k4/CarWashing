# AutoWash Pro Frontend

React + Vite + TypeScript frontend cho .NET API của AutoWash Pro.

## Chạy cục bộ

1. Sao chép `.env.example` thành `.env` và đặt `VITE_API_BASE_URL` theo API đang chạy.
2. Chạy `npm install`.
3. Chạy `npm run dev`.

## Kiểm tra chất lượng

- `npm run build`: kiểm tra TypeScript và tạo production build.
- `npm run lint`: chạy Oxlint.

## Kiến trúc

- `src/app`: bootstrap và provider.
- `src/shared`: API client, UI dùng chung và tiện ích.
- `src/features`: module nghiệp vụ customer, operations và admin.

Không đưa token hoặc secret vào biến bắt đầu bằng `VITE_`; các biến này được đóng gói vào JavaScript phía client.
