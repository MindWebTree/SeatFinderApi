# Counselling App API

A .NET 10 Web API for a NEET-PG style medical counselling / seat-finder app, built with a
layered ("clean") architecture, MySQL + stored procedures only (no EF Core / raw ad-hoc SQL),
and JWT authentication & authorization.

## Architecture

```
CounsellingApp.sln
src/
├── CounsellingApp.API             Presentation layer: controllers, Program.cs, config
├── CounsellingApp.Application     Business logic: services, DTOs, interfaces, JWT/email settings
├── CounsellingApp.Domain          Entities (POCOs) - no dependencies on anything else
└── CounsellingApp.Infrastructure  Data access: MySqlConnector + Dapper, calls stored procedures only
database/
├── 01_schema.sql                  Tables
├── 02_sp_auth.sql                 Stored procedures: register / login / forgot / reset / refresh token
├── 03_sp_counselling.sql          Stored procedures: seat search, states, courses
└── 04_seed_data.sql               Optional sample data for local testing
```

Dependency direction: `API → Application → Domain`, and `Infrastructure → Application → Domain`.
The API layer wires everything together in `Program.cs` (dependency injection); Application never
references Infrastructure directly, only its interfaces (`IUserRepository`, `ICounsellingRepository`),
so the data layer could be swapped later without touching business logic.

## Tech stack

| Concern              | Choice                                             |
|-----------------------|-----------------------------------------------------|
| Framework             | .NET 10 / ASP.NET Core Web API                     |
| Database               | MySQL 8+, accessed **only** via stored procedures  |
| Data access            | Dapper + MySqlConnector                            |
| Naming                  | PascalCase tables/columns; dual `Id`(int)+`Guid` key on every table except `Users`/`Roles` |
| Auth                    | JWT access tokens + rotating refresh tokens, **many-to-many roles** |
| Password hashing        | BCrypt (`BCrypt.Net-Next`)                         |
| Docs                    | Swagger / OpenAPI (dev environment only)           |

### Naming & key strategy

- **Tables/columns are PascalCase** (`Id`, `Name`, `FullName`, ...). Constraint/index names stay
  lowercase snake_case (`uq_`, `fk_`, `ix_` prefixes).
- **`Roles`, `Users`: GUID-only** primary key (`Id CHAR(36)`), no int id at all.
- **Every other table is dual-key**: `Id INT AUTO_INCREMENT PRIMARY KEY` (fast internal joins) +
  `Guid CHAR(36) UNIQUE DEFAULT (UUID())` (safe public-facing id).
- **Relation rule**: any column relating to `Users`/`Roles` is `CHAR(36)`; every other relation
  (`Colleges.StateId`, `SeatAllotments.CollegeId`/`CourseId`, ...) is a plain `INT`.
- **Every table** carries `IsDeleted`, `CreatedOn`, `UpdatedOn` audit columns.
- **`Users` ↔ `Roles` is many-to-many** via the `UserRole` junction table (composite PK
  `UserId, RoleId`) — a user can hold more than one role. `Users` no longer has a `RoleId` column.

| Table | Key(s) |
|---|---|
| `Roles` | `Id` CHAR(36) only |
| `Users` | `Id` CHAR(36) only |
| `UserRole` | composite PK (`UserId`, `RoleId`), both CHAR(36) |
| `PasswordResetTokens` / `RefreshTokens` | `Id` INT + `Guid` CHAR(36); `UserId` FK is CHAR(36) |
| `States` / `Courses` | `Id` INT + `Guid` CHAR(36) |
| `Colleges` | `Id` INT + `Guid` CHAR(36); `StateId` FK is INT |
| `SeatAllotments` | `Id` INT + `Guid` CHAR(36); `CollegeId`/`CourseId` FKs are INT |

`sp_RegisterUser` inserts the user **and** their default role assignment (`RoleIds.Student`) into
`UserRole` in one call. `sp_GetUserByEmail` / `sp_GetUserById` return a comma-separated `Roles`
column via `GROUP_CONCAT`, which the API turns into a `List<string>` and one JWT role claim per
role — `[Authorize(Roles = "Admin")]` matches if the user has *any* matching role claim.

The connection string includes `Guid Format=Char36` so **MySqlConnector** converts `CHAR(36)`
columns to/from `System.Guid` automatically.

## Endpoints

| Method | Route                          | Auth        | Purpose                                             |
|--------|--------------------------------|-------------|------------------------------------------------------|
| POST   | `/api/auth/register`           | Public      | Create a student account                              |
| POST   | `/api/auth/login`               | Public      | Get an access token + refresh token                    |
| POST   | `/api/auth/forgot-password`    | Public      | Request a password reset link (generic response)      |
| POST   | `/api/auth/reset-password`     | Public      | Set a new password using the emailed token             |
| POST   | `/api/auth/refresh-token`      | Public      | Exchange a refresh token for a new access token         |
| GET    | `/api/counselling/seat-finder` | **JWT**     | Core counselling logic - filtered seat search           |
| GET    | `/api/counselling/states`      | Public      | Lookup list for filters                                |
| GET    | `/api/counselling/courses`     | Public      | Lookup list for filters                                |

`seat-finder` is protected with `[Authorize]` — callers must send `Authorization: Bearer <accessToken>`.

## How authentication works

1. **Register** (`/api/auth/register`) — password is hashed with BCrypt before it ever touches the
   database. Both **email and phone number are checked for uniqueness** (phone is optional, but
   when provided it must be unique too, since it doubles as a login identifier).
2. **Login** (`/api/auth/login`) — accepts a single `emailOrPhoneNumber` field. If it contains `@`
   it's looked up via `sp_GetUserByEmail`; otherwise via `sp_GetUserByPhoneNumber`. Both procedures
   return the same shape, so the rest of the flow (password check, token issuance) is identical
   either way. `IsEmailVerified` / `IsPhoneNumberVerified` are tracked on `Users` but no OTP/verify
   flow is wired up yet — that's a natural next endpoint to add (e.g. `POST /api/auth/verify-otp`).
   Verifies the BCrypt hash, then issues:
   - a short-lived **JWT access token** (default 60 min, `HS256`, signed with `JwtSettings:Secret`)
   - a long-lived opaque **refresh token** (default 7 days), stored server-side in `refresh_tokens`
     so it can be revoked.
3. **Calling protected endpoints** — send `Authorization: Bearer <accessToken>`. ASP.NET Core's JWT
   Bearer middleware validates issuer, audience, signature and expiry automatically; `[Authorize]`
   on `CounsellingController` enforces it.
4. **Refresh** (`/api/auth/refresh-token`) — when the access token expires, the client exchanges the
   refresh token for a new pair. The old refresh token is revoked (rotation) so it can't be replayed.
5. **Forgot / reset password** — a random 32-byte token is generated, **hashed with SHA-256 before
   being stored**, and the raw token is only ever put in the emailed link. `forgot-password` always
   returns the same generic message, whether or not the email exists, to prevent account enumeration.

## Seat-finder logic

`GET /api/counselling/seat-finder?rank=15150&category=Open&instituteTypes=Govt,Private&courseId=&stateId=`

- `sp_SearchSeats` applies category eligibility (General/Open candidates only match `Open` seats;
  reserved-category candidates match `Open` + their own category) and the institute type / course /
  state filters, all inside MySQL.
- `CounsellingService` then buckets the results in C#, mirroring a typical seat-finder UX:
  - **Closed just before you** — round-1 closing rank was better than the user's rank but within
    1,000 ranks (near misses, closest first).
  - **Within reach** — round-1 closing rank was equal to or worse than the user's rank (closest first).

## Setup

### 1. Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- MySQL 8.0+ running locally or reachable over the network

### 2. Create the database
```bash
mysql -u root -p < database/01_schema.sql
mysql -u root -p < database/02_sp_auth.sql
mysql -u root -p < database/03_sp_counselling.sql
mysql -u root -p < database/04_seed_data.sql   # optional sample data
```

### 3. Configure secrets
Edit `src/CounsellingApp.API/appsettings.json` (or better, use `dotnet user-secrets` /
environment variables so real secrets never get committed):

```bash
cd src/CounsellingApp.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=counselling_app;User Id=root;Password=YOUR_PASSWORD;Guid Format=Char36;"
dotnet user-secrets set "JwtSettings:Secret" "$(openssl rand -base64 48)"
```

### 4. Restore, build, run
```bash
dotnet restore
dotnet build
cd src/CounsellingApp.API
dotnet run
```
Swagger UI opens at `https://localhost:7100/swagger` (from `launchSettings.json`) where you can
register, login, click **Authorize** and paste the access token to try `seat-finder`.

## Notes / production checklist

- **Never** commit a real `JwtSettings:Secret` or DB password — the checked-in `appsettings.json`
  has placeholders on purpose. Use user-secrets locally and environment variables / a secrets
  manager (Azure Key Vault, AWS Secrets Manager, etc.) in production.
- All database access goes through stored procedures called via Dapper with parameterized
  `DynamicParameters` — no string-concatenated SQL anywhere, so this is not vulnerable to SQL
  injection as written.
- `Program.cs` sets `Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;` — required for
  snake_case MySQL columns (`full_name`, `token_guid`, ...) to map onto PascalCase C# properties
  (`FullName`, `TokenGuid`, ...). Without it, Dapper only matches by exact case-insensitive name
  and most of the entity mapping in this project would silently come back with default values.
- `SmtpSettings` is empty by default; with no host configured, `EmailService` logs the reset link
  instead of sending an email, so `forgot-password` still works end-to-end locally. Fill in real
  SMTP credentials (or swap in SendGrid/SES) for production.
- The seed data numbers are illustrative only (loosely modelled on a sample seat-finder mockup) —
  replace `04_seed_data.sql` with real data before using this for anything beyond local testing.
- Consider adding: rate limiting on `/api/auth/*`, email verification on register, FluentValidation
  for richer request validation, and structured logging (Serilog) for production observability.
