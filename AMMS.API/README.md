# MVEA - MLA–Voter Engagement Application API

## Overview

MVEA is a governance and engagement platform designed to create a direct, transparent, and structured communication channel between Local MLAs and voters of specific Assembly Constituencies.

**Technology Stack**:
- .NET Core 8 Web API
- **Dapper** (Data Access - not Entity Framework)
- **Unit of Work Pattern** in Service Layer
- SQL Server
- Clean Architecture

---

## Architecture

This application follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────┐
│         API Layer (v1/v2)           │  Controllers, Middleware, SignalR
├─────────────────────────────────────┤
│      Application Layer               │  Services, DTOs, Validators, UnitOfWork
├─────────────────────────────────────┤
│         Domain Layer                 │  Entities, Interfaces, Enums
├─────────────────────────────────────┤
│      Infrastructure Layer            │  Dapper Repositories, External Services
└─────────────────────────────────────┘
```

### Key Architectural Decisions

1. **Dapper over EF Core**: Direct SQL for better performance and control
2. **Unit of Work in Service Layer**: Transaction management across repositories
3. **API Versioning**: URL path versioning (`/api/v1/...`)
4. **Clean Architecture**: Dependency flow from outer to inner layers

---

## Project Structure

```
MVEA.API/
├── Domain/                          # Business logic (innermost)
│   ├── Entities/                   # User, MLA, Voter, Ticket, etc.
│   ├── Enums/                      # UserRole, TicketStatus, etc.
│   └── Interfaces/                 # Repository interfaces
│
├── Application/                     # Use cases & orchestration
│   ├── DTOs/                       # Request/Response DTOs
│   ├── Interfaces/                 # Service interfaces
│   ├── Services/                   # Service implementations (uses UnitOfWork)
│   └── Validators/                 # FluentValidation validators
│
├── Infrastructure/                  # Technical implementation
│   ├── Data/                       # Dapper repositories
│   │   ├── DapperContext.cs        # Connection management
│   │   ├── UnitOfWork/             # Unit of Work implementation
│   │   └── Repositories/           # Repository implementations
│   ├── ExternalServices/           # SMS, WhatsApp, OTP services
│   └── Security/                   # JWT, Encryption services
│
├── API/                             # HTTP interface (outermost)
│   ├── Controllers/v1/             # Version 1 API endpoints
│   ├── Middleware/                 # Exception handling, logging
│   └── Extensions/                 # Service registration extensions
│
└── Shared/                          # Shared utilities
    └── Constants/                  # Application constants
```

---

## Key Features

### ✅ Authentication APIs
- `POST /api/v1/auth/send-otp` - Send OTP to mobile number
- `POST /api/v1/auth/verify-otp` - Verify OTP
- `POST /api/v1/auth/login` - Login with OTP/password
- `POST /api/v1/auth/logout` - Logout and invalidate token
- `GET /api/v1/auth/device-history` - Get login history

### ✅ User & Role Management
- `POST /api/v1/users` - Create user
- `GET /api/v1/users/{id}` - Get user profile
- `PUT /api/v1/users/{id}` - Update user
- `GET /api/v1/roles` - Get all roles
- `POST /api/v1/roles/assign` - Assign role to user

### ✅ MLA Profile & Assembly
- `POST /api/v1/mla/profile` - Create/submit MLA profile
- `GET /api/v1/mla/profile` - Get MLA profile
- `PUT /api/v1/mla/profile` - Update MLA profile
- `GET /api/v1/assembly/list` - Get assembly list

### ✅ Grievance/Ticket Management
- `POST /api/v1/tickets` - Create ticket
- `GET /api/v1/tickets/{id}` - Get ticket details
- `PUT /api/v1/tickets/{id}/status` - Update ticket status
- `GET /api/v1/tickets/report` - Get ticket reports

### ✅ Chat, Content, Notifications, Analytics
- Chat APIs for voter-MLA communication
- Content management for MLA posts
- Notification scheduling and delivery
- Analytics and reporting endpoints

---

## Quick Start

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or SQL Server Express)
- Visual Studio 2022 or VS Code

### Setup Steps

1. **Clone repository**
```bash
git clone <repository-url>
cd MVEA.API
```

2. **Restore packages**
```bash
dotnet restore
```

3. **Configure database connection**
Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MVEA;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

4. **Install NuGet packages** (see `SETUP_INSTRUCTIONS.md`)

5. **Run the application**
```bash
dotnet run
```

6. **Access Swagger UI**
```
https://localhost:5001/swagger
```

---

## Documentation

- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Complete architecture documentation
- **[LAYER_RESPONSIBILITIES.md](LAYER_RESPONSIBILITIES.md)** - Layer responsibilities and patterns
- **[SECURITY_SCALABILITY.md](SECURITY_SCALABILITY.md)** - Security and scalability considerations
- **[SETUP_INSTRUCTIONS.md](SETUP_INSTRUCTIONS.md)** - Detailed setup guide

---

## Unit of Work Pattern

Services use `IUnitOfWork` for transaction management:

```csharp
public class TicketService : ITicketService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<TicketResponse> CreateTicketAsync(...)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Multiple repository operations
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

---

## Dapper Repository Pattern

Repositories use Dapper for data access:

```csharp
public class UserRepository : BaseRepository<User>, IUserRepository
{
    public async Task<User?> GetByMobileAsync(string mobileNumber)
    {
        var query = "SELECT * FROM Users WHERE MobileNumber = @MobileNumber AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<User>(
            query, new { MobileNumber = mobileNumber }, _unitOfWork.Transaction);
    }
}
```

---

## API Versioning

URL path versioning: `/api/v1/...`, `/api/v2/...`

- **v1**: Current production version
- **v2**: Future breaking changes

---

## Security Features

- ✅ JWT Bearer Authentication
- ✅ OTP-based login
- ✅ Role-based access control (RBAC)
- ✅ SQL injection prevention (parameterized queries)
- ✅ Input validation (FluentValidation)
- ✅ Rate limiting
- ✅ CORS configuration
- ✅ Audit logging
- ✅ Data encryption at rest/transit

---

## Scalability Features

- ✅ Async/await for all I/O operations
- ✅ Pagination on list endpoints
- ✅ Response caching
- ✅ Database indexing
- ✅ Connection pooling
- ✅ Stateless API design
- ✅ Horizontal scaling ready
- ✅ Redis cache support

---

## Contributing

1. Follow Clean Architecture principles
2. Use Unit of Work pattern in services
3. Implement all repository operations with Dapper
4. Add appropriate validation using FluentValidation
5. Document API endpoints with XML comments
6. Write unit tests for services

---

## License

This project is proprietary software for MVEA.

---

## Support

For issues or questions, contact the development team.
