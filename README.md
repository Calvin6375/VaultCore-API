# VaultCore API

Production-quality MVP for a secure digital banking REST API built with **ASP.NET Core 8**, Clean Architecture, PostgreSQL, and JWT authentication.

## Tech Stack

- **ASP.NET Core 8** Web API
- **Entity Framework Core 8** with PostgreSQL
- **JWT** authentication with refresh tokens
- **Clean Architecture**: Domain, Application, Infrastructure, API
- **Repository + Unit of Work**
- **FluentValidation**, **AutoMapper**, **Serilog**, **Swagger**
- **Docker** support

## Security

- JWT access + refresh tokens
- Role-Based Access Control (Admin, Customer, Support)
- BCrypt password hashing
- Configuration via environment variables
- API rate limiting (fixed window per user/IP)
- Input validation (FluentValidation)
- Audit logging for sensitive actions
- Correlation ID and request logging

## Getting Started

### Prerequisites

- .NET 8 SDK
- PostgreSQL 14+ (or use Docker)

### Local run

1. **Clone and restore**

   ```bash
   dotnet restore
   ```

2. **Configure**

   Copy `.env.example` to `.env` or set environment variables / `appsettings.Development.json`:

   - `ConnectionStrings__DefaultConnection` – PostgreSQL connection string
   - `Jwt__Secret` – at least 32 characters

3. **Create database and run migrations**

   Migrations run automatically on startup. Ensure PostgreSQL is running and the connection string is correct.

4. **Run the API**

   ```bash
   dotnet run --project src/VaultCore.API
   ```

   - API: http://localhost:5000  
   - Swagger: http://localhost:5000/swagger  
   - Health: http://localhost:5000/health  

### Docker

```bash
docker-compose up --build
```

- API: http://localhost:5000  
- PostgreSQL: localhost:5432 (user: `postgres`, password: `postgres`, db: `VaultCore`)

### Seeded data

After first run:

- **Admin**: `admin@vaultcore.local` / `Admin@123` (change in production)
- Roles: Admin, Customer, Support (new users get Customer by default)

## API Overview

| Area            | Endpoints |
|-----------------|-----------|
| **Auth**        | `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/refresh`, `POST /api/auth/logout` |
| **Users**       | `GET /api/users/me`, `PUT /api/users/{id}`, `GET /api/users/{id}`, `GET /api/users?page=&pageSize=&search=`, `POST /api/users/{id}/roles` |
| **Wallets**     | `GET /api/wallets/me`, `GET /api/wallets/user/{userId}`, `POST /api/wallets/{id}/freeze`, `POST /api/wallets/{id}/unfreeze` |
| **Transactions**| `POST /api/transactions/deposit/{userId}`, `POST /api/transactions/withdraw`, `POST /api/transactions/transfer`, `GET /api/transactions/{id}`, `GET /api/transactions/me/history`, `GET /api/transactions` (admin) |
| **Admin**       | `GET /api/admin/users`, `GET /api/admin/audit-logs` |

Protected routes require `Authorization: Bearer <access_token>`.

## Project structure

```
src/
  VaultCore.API/           # Controllers, middleware, filters
  VaultCore.Application/   # DTOs, validators, application services
  VaultCore.Domain/        # Entities, enums, repository interfaces
  VaultCore.Infrastructure/# EF Core, repositories, auth, audit, seed
tests/
  VaultCore.UnitTests/
  VaultCore.IntegrationTests/
```

## Adding migrations

From the repo root:

```bash
dotnet ef migrations add YourMigrationName --project src/VaultCore.Infrastructure --startup-project src/VaultCore.API
```

## Testing

```bash
dotnet test
```

- Unit tests: core services (auth, transactions, etc.)
- Integration tests: auth and transaction flows (in-memory DB)

## API collection

- **Swagger UI**: http://localhost:5000/swagger (when running).
- **Postman**: Import `postman/VaultCore-API.postman_collection.json`. Set `baseUrl` to `http://localhost:5000` and after login set `accessToken` from the response.

## License

See LICENSE file.
