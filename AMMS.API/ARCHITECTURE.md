# MVEA - Clean Architecture Documentation

## System Architecture Overview

This application follows **Clean Architecture** principles with clear separation of concerns across multiple layers using **.NET Core 8 Web API** with **Dapper** for data access.

## Layer Structure

```
MVEA.API/
├── Domain/                          # Core Business Logic Layer (Innermost)
│   ├── Entities/                   # Domain entities
│   ├── Enums/                      # Domain enumerations
│   ├── ValueObjects/               # Value objects
│   ├── Interfaces/                 # Domain service interfaces
│   └── Exceptions/                 # Domain exceptions
│
├── Application/                     # Application Business Logic Layer
│   ├── DTOs/                       # Data Transfer Objects
│   │   ├── Request/                # Request DTOs
│   │   └── Response/               # Response DTOs
│   ├── Interfaces/                 # Application service interfaces
│   ├── Services/                   # Application services (use cases)
│   ├── Mappings/                   # AutoMapper profiles
│   ├── Validators/                 # FluentValidation validators
│   └── Common/                     # Application common utilities
│
├── Infrastructure/                  # Infrastructure & External Concerns
│   ├── Data/                       # Data Access Layer (Dapper)
│   │   ├── Repositories/           # Dapper repository implementations
│   │   ├── UnitOfWork/             # Unit of Work implementation
│   │   ├── DapperContext.cs        # Dapper connection management
│   │   └── Scripts/                # SQL scripts/queries
│   ├── ExternalServices/           # External service integrations
│   │   ├── SmsService/             # SMS Gateway
│   │   ├── WhatsAppService/        # WhatsApp Business API
│   │   └── OtpService/             # OTP generation/validation
│   ├── Security/                   # Security implementations
│   │   ├── JwtService/             # JWT token handling
│   │   ├── EncryptionService/      # Data encryption
│   │   └── AuditService/           # Audit logging
│   └── Persistence/                # Database migrations, seed data
│
├── API/                             # Presentation/API Layer (Outermost)
│   ├── Controllers/                # API Controllers
│   │   ├── v1/                     # Version 1 controllers
│   │   └── v2/                     # Version 2 controllers (future)
│   ├── Middleware/                 # Custom middleware
│   ├── Filters/                    # Action filters
│   ├── Extensions/                 # Extension methods
│   ├── Configuration/              # API configuration
│   └── Hubs/                       # SignalR hubs for real-time chat
│
└── Shared/                          # Shared utilities across layers
    ├── Constants/                  # Application constants
    ├── Helpers/                    # Helper utilities
    └── Resources/                  # Localization resources
```

## Layer Responsibilities

### 1. Domain Layer (Innermost - Pure Business Logic)
**Purpose**: Core business logic, independent of frameworks and external concerns.

**Responsibilities**:
- Define business entities (User, MLA, Voter, Ticket, etc.)
- Define business rules and validations
- Define domain events (if needed)
- Define domain service interfaces
- **NO dependencies** on other layers

**Key Components**:
- `Entities/`: User, MLA, Voter, Assembly, Ticket, Chat, Content, Notification
- `Enums/`: UserRole, TicketStatus, TicketCategory, ProfileStatus
- `Interfaces/`: IUserRepository, ITicketRepository (contracts only, no implementation)

**Rules**:
- No database, no HTTP, no external services
- Pure C# business logic

### 2. Application Layer (Use Cases & Orchestration)
**Purpose**: Application-specific business logic, use cases, orchestration.

**Responsibilities**:
- Define use cases (services)
- Define DTOs for data transfer
- Define service interfaces
- Handle validation using FluentValidation
- Coordinate between domain and infrastructure
- **Unit of Work** pattern for transaction management

**Key Components**:
- `Services/`: AuthService, MLAService, VoterService, TicketService, ChatService
- `DTOs/Request/`: LoginRequest, CreateTicketRequest
- `DTOs/Response/`: AuthResponse, TicketResponse
- `Interfaces/`: IAuthService, ITicketService
- `Mappings/`: AutoMapper profiles

**Unit of Work in Service Layer**:
- Services use `IUnitOfWork` for transaction management
- Single transaction per service method
- Repository operations within Unit of Work scope

### 3. Infrastructure Layer (Technical Implementation)
**Purpose**: Technical implementation details, external integrations.

**Responsibilities**:
- Data persistence using **Dapper** (not EF Core)
- External service integrations (SMS, WhatsApp)
- Security implementations (JWT, encryption)
- Repository implementations
- Unit of Work implementation

**Key Components**:
- `Data/Repositories/`: Dapper-based repository implementations
- `Data/UnitOfWork/`: Unit of Work with transaction management
- `Data/DapperContext.cs`: Connection management
- `ExternalServices/`: SMS, WhatsApp, OTP services
- `Security/`: JWT, Encryption, Hashing

**Dapper Implementation**:
- Direct SQL queries for better performance
- Parameterized queries to prevent SQL injection
- Connection pooling via DapperContext

### 4. API/Presentation Layer (HTTP Interface)
**Purpose**: HTTP interface, request/response handling.

**Responsibilities**:
- Handle HTTP requests/responses
- Input validation and sanitization
- Authentication/Authorization
- API versioning
- Error handling and formatting
- Real-time communication (SignalR)

**Key Components**:
- `Controllers/v1/`: Versioned API endpoints
- `Middleware/`: Exception handling, logging, request validation
- `Filters/`: Action filters for common concerns
- `Hubs/`: SignalR for chat functionality

## API Versioning Strategy

### Strategy: URL Path Versioning
**Format**: `/api/v{version}/{controller}/{action}`

**Example**:
- `/api/v1/auth/login`
- `/api/v1/tickets`
- `/api/v2/tickets` (future breaking changes)

### Implementation
```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TicketsController : ControllerBase
```

### Version Lifecycle
1. **v1**: Current production version
2. **v2**: Future breaking changes
3. Deprecated versions maintained for 6 months with deprecation notice

### Versioning Rules
- **Breaking Changes** → New version (v1 → v2)
- **Non-breaking Additions** → Same version
- **Bug Fixes** → Same version
- **Deprecation Notice**: 3 months before removal

## Dependency Flow

```
API Layer → Application Layer → Domain Layer
                ↓
         Infrastructure Layer → Domain Layer
```

**Rules**:
- Inner layers have **NO** dependencies on outer layers
- Domain layer is **completely independent**
- Application layer depends only on Domain interfaces
- Infrastructure implements Domain and Application interfaces
- API layer depends on Application and Infrastructure

## Unit of Work Pattern in Service Layer

### Pattern Implementation
```
Application Service → Unit of Work → Repositories → Dapper → SQL Server
```

### Benefits
- Single transaction per service method
- Atomic operations across multiple repositories
- Consistent data state
- Easy rollback on errors

### Example Flow
```csharp
public class TicketService : ITicketService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<TicketResponse> CreateTicketAsync(CreateTicketRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var ticket = await _ticketRepository.AddAsync(...);
            await _notificationRepository.AddAsync(...);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            return ticket;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

## Dapper Repository Pattern

### Repository Structure
```csharp
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByMobileAsync(string mobileNumber);
    Task<User?> GetByEmailAsync(string email);
}

public class UserRepository : IUserRepository
{
    private readonly DapperContext _context;
    
    public async Task<User?> GetByMobileAsync(string mobileNumber)
    {
        var query = "SELECT * FROM Users WHERE MobileNumber = @MobileNumber AND IsDeleted = 0";
        return await _context.Connection.QueryFirstOrDefaultAsync<User>(query, new { MobileNumber = mobileNumber });
    }
}
```

### Benefits of Dapper
- **Performance**: Direct SQL, minimal overhead
- **Flexibility**: Full control over queries
- **Simplicity**: Lightweight ORM
- **SQL Skills**: Leverage existing SQL expertise

## Security Considerations

### 1. Authentication & Authorization
- **JWT Bearer Tokens** for API authentication
- **OTP-based login** for mobile verification
- **Role-based access control (RBAC)** with claims
- **Two-factor authentication (2FA)** optional for MLA accounts

### 2. Data Security
- **Encryption at Rest**: Sensitive voter data encrypted
- **Encryption in Transit**: HTTPS/TLS 1.3
- **Data Masking**: Mobile numbers masked in logs/responses
- **PII Protection**: Voter data access logged and audited

### 3. API Security
- **Rate Limiting**: Prevent abuse (100 requests/min per IP)
- **CORS Configuration**: Restricted to allowed origins
- **Input Validation**: FluentValidation on all inputs
- **SQL Injection Prevention**: Parameterized queries with Dapper
- **XSS Protection**: Input sanitization

### 4. Audit & Compliance
- **Audit Trail**: All data changes logged
- **Session Management**: Secure session handling
- **Compliance**: GDPR/Data Protection ready

## Scalability Considerations

### 1. Database Optimization
- **Indexing**: Strategic indexes on frequently queried columns
- **Connection Pooling**: Dapper connection pooling
- **Query Optimization**: Efficient SQL queries
- **Caching Strategy**: Redis for frequently accessed data

### 2. API Performance
- **Response Caching**: HTTP response caching for static data
- **Pagination**: All list endpoints paginated
- **Async/Await**: All I/O operations async
- **Compression**: Response compression (GZip/Brotli)

### 3. Horizontal Scaling
- **Stateless API**: All API instances stateless (JWT tokens)
- **Load Balancing**: Support for multiple API instances
- **Database Scaling**: Read replicas for read-heavy operations
- **CDN**: Static content via CDN

## API Endpoints Structure

### Authentication APIs (v1)
- `POST /api/v1/auth/send-otp`
- `POST /api/v1/auth/verify-otp`
- `POST /api/v1/auth/login`
- `POST /api/v1/auth/logout`
- `GET /api/v1/auth/device-history`

### User & Role Management (v1)
- `POST /api/v1/users`
- `GET /api/v1/users/{id}`
- `PUT /api/v1/users/{id}`
- `GET /api/v1/roles`
- `POST /api/v1/roles/assign`

### MLA Profile & Assembly (v1)
- `POST /api/v1/mla/profile`
- `GET /api/v1/mla/profile`
- `PUT /api/v1/mla/profile`
- `GET /api/v1/assembly/list`

### Admin Review (v1)
- `GET /api/v1/admin/mla/pending`
- `POST /api/v1/admin/mla/approve`
- `POST /api/v1/admin/mla/reject`
- `GET /api/v1/admin/audit/logs`

### Content Management (v1)
- `POST /api/v1/content`
- `GET /api/v1/content/feed`
- `PUT /api/v1/content/{id}`
- `PUT /api/v1/content/{id}/approve`
- `DELETE /api/v1/content/{id}`

### Notification & Campaign (v1)
- `POST /api/v1/notifications/schedule`
- `GET /api/v1/notifications/templates`
- `GET /api/v1/notifications/logs`

### Chat (v1)
- `GET /api/v1/chat/conversations`
- `GET /api/v1/chat/history/{conversationId}`
- `POST /api/v1/chat/send`
- `PUT /api/v1/chat/tag`

### Grievance/Ticket (v1)
- `POST /api/v1/tickets`
- `GET /api/v1/tickets/{id}`
- `PUT /api/v1/tickets/{id}/status`
- `GET /api/v1/tickets/report`

### Analytics & Reports (v1)
- `GET /api/v1/analytics/engagement`
- `GET /api/v1/analytics/issues`
- `GET /api/v1/analytics/resolution-time`
- `GET /api/v1/analytics/export`

### Voter Verification (v1)
- `POST /api/v1/voters/verify`
- `GET /api/v1/voters/profile`
- `POST /api/v1/voters/family`

## Project Setup Requirements

### NuGet Packages Required
- `Dapper` - Dapper ORM
- `System.Data.SqlClient` or `Microsoft.Data.SqlClient` - SQL Server client
- `AutoMapper` - Object mapping
- `FluentValidation.AspNetCore` - Input validation
- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT authentication
- `BCrypt.Net-Next` - Password hashing
- `Microsoft.AspNetCore.SignalR` - Real-time communication
- `Swashbuckle.AspNetCore` - Swagger/OpenAPI
- `Microsoft.AspNetCore.Mvc.Versioning` - API versioning

### Configuration Files
- `appsettings.json` - Application settings
- `appsettings.Development.json` - Development overrides
- Connection strings, JWT settings, external service credentials

## Environment Configuration

- **Development**: Local SQL Server, mock external services
- **Staging**: Azure SQL Database, test external services
- **Production**: Azure SQL Database, production services, Redis cache
