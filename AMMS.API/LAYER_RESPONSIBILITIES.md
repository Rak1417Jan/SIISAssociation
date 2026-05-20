# MVEA - Layer Responsibilities

## Clean Architecture Layer Responsibilities

This document defines the responsibilities and boundaries of each layer in the MVEA application.

---

## 1. Domain Layer (Innermost)

**Purpose**: Pure business logic, independent of frameworks and infrastructure.

### Responsibilities:
- ✅ Define business entities (User, MLA, Voter, Ticket, etc.)
- ✅ Define business rules and validations
- ✅ Define domain interfaces (IUserRepository, ITicketRepository)
- ✅ Define domain enums (UserRole, TicketStatus, etc.)
- ✅ Define domain value objects (if needed)

### Rules:
- ❌ NO dependencies on other layers
- ❌ NO database access
- ❌ NO HTTP/web references
- ❌ NO external service references
- ✅ Pure C# business logic only

### Key Files:
- `Domain/Entities/*.cs` - Business entities
- `Domain/Enums/*.cs` - Domain enumerations
- `Domain/Interfaces/*.cs` - Repository/service interfaces (contracts only)

---

## 2. Application Layer

**Purpose**: Application-specific business logic, use cases, orchestration.

### Responsibilities:
- ✅ Define use cases (services)
- ✅ Define DTOs for data transfer
- ✅ Define service interfaces
- ✅ Handle validation using FluentValidation
- ✅ Coordinate between domain and infrastructure
- ✅ **Unit of Work** pattern for transaction management
- ✅ Business orchestration

### Unit of Work Usage:
Services use `IUnitOfWork` for managing transactions:
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

### Rules:
- ✅ Depend only on Domain layer interfaces
- ❌ NO direct database access (use repositories via Unit of Work)
- ❌ NO HTTP/web references
- ✅ Can reference infrastructure interfaces (not implementations)

### Key Files:
- `Application/Services/*.cs` - Service implementations
- `Application/Interfaces/*.cs` - Service interfaces
- `Application/DTOs/*.cs` - Request/Response DTOs
- `Application/Validators/*.cs` - FluentValidation validators

---

## 3. Infrastructure Layer

**Purpose**: Technical implementation details, external integrations.

### Responsibilities:
- ✅ Implement repository interfaces using **Dapper**
- ✅ Implement Unit of Work pattern
- ✅ Manage database connections (DapperContext)
- ✅ External service integrations (SMS, WhatsApp, OTP)
- ✅ Security implementations (JWT, encryption)
- ✅ File storage operations

### Dapper Implementation:
- Direct SQL queries for performance
- Parameterized queries to prevent SQL injection
- Connection pooling via DapperContext
- Repository pattern with base repository

### Rules:
- ✅ Implements Domain and Application interfaces
- ✅ Can reference Domain layer
- ❌ NO direct references to API/Presentation layer

### Key Files:
- `Infrastructure/Data/DapperContext.cs` - Connection management
- `Infrastructure/Data/UnitOfWork/*.cs` - Unit of Work implementation
- `Infrastructure/Data/Repositories/*.cs` - Dapper repository implementations
- `Infrastructure/ExternalServices/*.cs` - External service integrations
- `Infrastructure/Security/*.cs` - Security services

---

## 4. API/Presentation Layer (Outermost)

**Purpose**: HTTP interface, request/response handling.

### Responsibilities:
- ✅ Handle HTTP requests/responses
- ✅ Input validation and sanitization
- ✅ Authentication/Authorization
- ✅ API versioning
- ✅ Error handling and formatting
- ✅ Real-time communication (SignalR)

### Rules:
- ✅ Can reference Application layer
- ✅ Can reference Infrastructure layer
- ❌ NO direct business logic
- ✅ HTTP-specific concerns only

### Key Files:
- `API/Controllers/v1/*.cs` - API controllers
- `API/Middleware/*.cs` - Custom middleware
- `API/Filters/*.cs` - Action filters
- `API/Hubs/*.cs` - SignalR hubs

---

## Dependency Flow Diagram

```
┌─────────────────────────────────────┐
│         API Layer                    │
│  (Controllers, Middleware, Hubs)    │
└──────────────┬──────────────────────┘
               │ depends on
               ▼
┌─────────────────────────────────────┐
│      Application Layer               │
│  (Services, DTOs, Validators)       │
│  - Uses IUnitOfWork for transactions│
└──────────────┬──────────────────────┘
               │ depends on
               ▼
┌─────────────────────────────────────┐
│         Domain Layer                 │
│  (Entities, Interfaces, Enums)      │
│  - NO dependencies on outer layers  │
└─────────────────────────────────────┘
               ▲
               │ implements
┌──────────────┴──────────────────────┐
│      Infrastructure Layer            │
│  (Dapper Repos, UnitOfWork,         │
│   External Services, Security)      │
└─────────────────────────────────────┘
```

---

## Unit of Work Pattern

### In Service Layer (Application)

**Purpose**: Manage transactions across multiple repository operations.

```csharp
public class TicketService : ITicketService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITicketRepository _ticketRepository;
    private readonly INotificationRepository _notificationRepository;

    public async Task<TicketResponse> CreateTicketAsync(CreateTicketRequest request, int voterId)
    {
        // Begin transaction
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            // Create ticket
            var ticket = await _ticketRepository.AddAsync(...);
            
            // Create notification
            await _notificationRepository.AddAsync(...);
            
            // Save all changes
            await _unitOfWork.SaveChangesAsync();
            
            // Commit transaction
            await _unitOfWork.CommitTransactionAsync();
            
            return ticket;
        }
        catch
        {
            // Rollback on error
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

### Benefits:
- ✅ Atomic operations across multiple repositories
- ✅ Consistent data state
- ✅ Easy rollback on errors
- ✅ Single transaction per service method

---

## Dapper Repository Pattern

### Base Repository

```csharp
public abstract class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly UnitOfWork _unitOfWork;
    protected readonly IDbConnection _connection;
    protected abstract string TableName { get; }

    public async Task<T?> GetByIdAsync(int id)
    {
        var query = $"SELECT * FROM {TableName} WHERE Id = @Id AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<T>(query, new { Id = id }, _unitOfWork.Transaction);
    }
}
```

### Concrete Repository

```csharp
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByMobileAsync(string mobileNumber);
}

public class UserRepository : BaseRepository<User>, IUserRepository
{
    protected override string TableName => "Users";

    public async Task<User?> GetByMobileAsync(string mobileNumber)
    {
        var query = "SELECT * FROM Users WHERE MobileNumber = @MobileNumber AND IsDeleted = 0";
        return await _connection.QueryFirstOrDefaultAsync<User>(query, new { MobileNumber = mobileNumber }, _unitOfWork.Transaction);
    }
}
```

---

## Communication Rules Between Layers

1. **API → Application**: API calls Application services
2. **Application → Domain**: Application uses Domain interfaces
3. **Application → Infrastructure**: Application uses Infrastructure via interfaces (through DI)
4. **Infrastructure → Domain**: Infrastructure implements Domain interfaces
5. **NO**: Domain → Any outer layer (Domain is independent)
6. **NO**: API → Domain directly (go through Application layer)

---

## Summary

- **Domain**: Pure business logic, no dependencies
- **Application**: Use cases, orchestration, Unit of Work
- **Infrastructure**: Technical implementation (Dapper, external services)
- **API**: HTTP interface, versioning, middleware

This architecture ensures:
- ✅ Testability (each layer can be tested independently)
- ✅ Maintainability (clear separation of concerns)
- ✅ Scalability (components can be replaced/modified independently)
- ✅ Flexibility (easy to add new features)
