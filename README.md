# CarWashing Fullstack

AutoWash Pro gồm API ASP.NET Core trong `CarWashing` và frontend React/Vite trong `CarWashing_FE`.

## Chạy cho thành viên nhóm

Yêu cầu: .NET SDK 8, Node.js 20+, PostgreSQL dùng chung mà nhóm đã được cấp quyền truy cập.

1. Tạo `CarWashing/API/appsettings.Development.json` bằng cách sao chép `CarWashing/API/appsettings.example.json`.
2. Lấy chuỗi kết nối PostgreSQL dùng chung và JWT secret từ kênh quản lý secret của nhóm, rồi điền vào file local hoặc đặt biến môi trường:

   ```powershell
   $env:ConnectionStrings__MyDB = 'Host=...;Port=5432;Database=...;Username=...;Password=...'
   $env:JwtSettings__SecretKey = 'team-development-secret'
   ```

   Không commit file `appsettings*.json`, mật khẩu PostgreSQL, JWT secret, SMTP password hoặc Gemini API key. AI sẽ dùng mock fallback khi `GeminiSettings:ApiKey` để trống.

3. Chạy API:

   ```powershell
   cd CarWashing
   dotnet restore
   dotnet run --project API/API.csproj
   ```

4. Chạy frontend trong terminal khác:

   ```powershell
   cd CarWashing_FE
   npm install
   npm run dev
   ```

Frontend mặc định gọi `http://localhost:5152`. Có thể thay đổi bằng `VITE_API_BASE_URL` trong `CarWashing_FE/.env.local`.
