# AutoWash Pro — Car Washing System API

Smart car washing management system backend (.NET 8, PostgreSQL).

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL 14+ (local or cloud)
- (Optional) Google Gemini API key for live AI responses

## Quick Start

```bash
# Restore & run
cd API
dotnet restore
dotnet run
```

Swagger UI: `https://localhost:7xxx/swagger` (port shown in console).

## Configuration

Copy secrets out of source control — use **User Secrets** (recommended for dev):

```bash
cd API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:MyDB" "Host=localhost;Port=5432;Database=carwashing;Username=postgres;Password=YOUR_PASSWORD"
dotnet user-secrets set "JwtSettings:SecretKey" "YOUR_JWT_SECRET_AT_LEAST_32_CHARS"
dotnet user-secrets set "EmailSettings:Password" "YOUR_GMAIL_APP_PASSWORD"
dotnet user-secrets set "GeminiSettings:ApiKey" "YOUR_GEMINI_KEY"
```

| Setting | Description |
|---------|-------------|
| `ConnectionStrings:MyDB` | PostgreSQL connection string |
| `JwtSettings:SecretKey` | JWT signing key (min 32 chars) |
| `EmailSettings:*` | SMTP for OTP verification emails |
| `GeminiSettings:ApiKey` | Gemini API key (empty = mock fallback) |
| `GeminiSettings:UseMockFallback` | `true` = no external AI calls |
| `AiSettings:UseMockCustomerContext` | `true` = use static demo AI context |
| `CorsSettings:AllowedOrigins` | Frontend origins (dev defaults in `appsettings.Development.json`) |

For local overrides without secrets, create `API/appsettings.Local.json` (git-ignored).

## Database Migrations

```bash
cd API
dotnet ef database update --project ../DataAccessLayer
```

## Demo Accounts

Seeded automatically on startup (idempotent — safe to restart).

| Username | Password | Role | Tier | Notes |
|----------|----------|------|------|-------|
| `admin` | `Admin@123` | Admin | — | Full admin access + AI admin chat |
| `staff` | `Staff@123` | Staff | — | Staff role testing |
| `demo_customer` | `Customer@123` | Customer | Silver | 450 pts, Toyota Vios `51A12345` |
| `demo_vip` | `Customer@123` | Customer | Gold | 2100 pts, Hyundai Santa Fe |
| `demo_bronze` | `Customer@123` | Customer | Bronze | New member, Kia Morning |
| `demo_platinum` | `Customer@123` | Customer | Platinum | VIP, Mercedes GLC |

All demo customer emails are verified — login works immediately.

## API Overview

| Area | Base route | Auth |
|------|-----------|------|
| Auth | `/api/auth` | Public (register/login) |
| Customer profile | `/api/customers/me` | Customer |
| Vehicles | `/api/vehicles` | Customer |
| Wash history | `/api/wash-histories` | Customer |
| Admin users | `/api/admin/users` | Admin |
| Behavioral logs | `/api/behavioral-logs` | Admin |
| AI assistant | `/api/ai` | Customer / Admin |

JWT smoke endpoints: `GET /api/auth/admin-only`, `GET /api/auth/customer-only`.

## Running Tests

```bash
dotnet test
```

Tests use in-memory database and mock email/AI — no PostgreSQL or Gemini required.

Coverage includes:
- Phone normalization duplicate detection (Auth)
- Auth → verify → login → profile → vehicle flow (integration)
- AI prompts, hallucination guard, E2E AI endpoints with fallback

## AI Demo

See [AI_DEMO.md](AI_DEMO.md) for sample questions, expected answers, and fallback behavior.

## Project Structure

```
API/              — Controllers, middleware, seed, Program.cs
BusinessLayer/    — Services, DTOs, validators, AI layer
DataAccessLayer/  — EF Core entities, migrations
CarWashingSystem.Tests/ — xUnit unit + integration tests
```

## Security Notes

- Do **not** commit real credentials to `appsettings.json`
- CORS is restricted to configured origins in production; dev allows localhost
- AI layer is read-only — does not modify bookings or payments
- Rate limits: AI customer 10/min, admin 20/min (configurable)
