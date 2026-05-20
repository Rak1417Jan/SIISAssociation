# MVEA - Security & Scalability Considerations

## Security Considerations

### 1. Authentication & Authorization

#### JWT Bearer Authentication
- **Token-based authentication** using JWT (JSON Web Tokens)
- **Access tokens**: Short-lived (24 hours)
- **Refresh tokens**: Longer-lived (7 days)
- **Token validation**: Validate issuer, audience, expiration, signature

```csharp
// JWT Configuration
- Secret Key: Minimum 32 characters (HS256)
- Issuer: MVEA.API
- Audience: MVEA.Users
- Expiration: 1440 minutes (24 hours)
```

#### OTP-Based Login
- **6-digit OTP** for mobile verification
- **10-minute expiration** for OTPs
- **Rate limiting**: Max 3 OTP requests per hour per mobile number
- **Secure storage**: OTPs hashed before storage

#### Role-Based Access Control (RBAC)
- **User Roles**: SystemAdmin, MLA, MLATeamMember, Voter
- **Policy-based authorization** using claims
- **Endpoint-level authorization** with `[Authorize(Roles = "...")]`

### 2. Data Security

#### Encryption at Rest
- **Sensitive fields encrypted** (voter mobile numbers, PII)
- **Encryption key management**: Azure Key Vault (production)
- **Database encryption**: Transparent Data Encryption (TDE)

#### Encryption in Transit
- **HTTPS/TLS 1.3** for all API communications
- **Certificate pinning** for mobile apps
- **Secure WebSocket** (WSS) for SignalR connections

#### Data Masking
- **Mobile numbers masked** in logs: `91****1234`
- **PII redaction** in audit logs
- **Sanitized responses** for public endpoints

### 3. API Security

#### Input Validation
- **FluentValidation** on all DTOs
- **SQL Injection Prevention**: Parameterized queries with Dapper
- **XSS Protection**: Input sanitization
- **File Upload Validation**: File type, size limits

#### Rate Limiting
- **Per IP**: 100 requests/minute
- **Per User**: 1000 requests/hour (authenticated)
- **Endpoint-specific**: 
  - OTP: 3 requests/hour
  - Ticket creation: 5 tickets/day per voter
  - Login attempts: 5 attempts/hour

#### CORS Configuration
```csharp
// Development
AllowAnyOrigin, AllowAnyMethod, AllowAnyHeader

// Production
WithOrigins("https://mvea.app")
AllowCredentials()
AllowSpecificMethods(["GET", "POST", "PUT", "DELETE"])
```

#### API Key Protection
- **Swagger UI**: Protected in production
- **API Documentation**: Requires authentication
- **Health checks**: Public, limited information

### 4. Database Security

#### Connection Security
- **Encrypted connections**: TrustServerCertificate or valid certificates
- **Connection pooling**: Limited pool size (100 connections max)
- **Connection timeout**: 30 seconds

#### SQL Injection Prevention
- **Parameterized queries**: All Dapper queries use parameters
```csharp
// ✅ Safe
var query = "SELECT * FROM Users WHERE MobileNumber = @MobileNumber";
await connection.QueryAsync<User>(query, new { MobileNumber = mobileNumber });

// ❌ Unsafe (NEVER do this)
var query = $"SELECT * FROM Users WHERE MobileNumber = '{mobileNumber}'";
```

#### Access Control
- **Least privilege**: Database user with minimal required permissions
- **Read replicas**: Separate read/write connections
- **Audit logging**: All data modifications logged

### 5. Audit & Compliance

#### Audit Trail
- **All data changes logged**: Created, Updated, Deleted
- **User activity logging**: Login, logout, actions
- **Field-level auditing**: Track what changed

#### Compliance
- **GDPR Ready**: Right to deletion, data export
- **Data Retention**: Configurable retention policies
- **Access Logs**: IP, timestamp, user, endpoint

---

## Scalability Considerations

### 1. Database Optimization

#### Indexing Strategy
```sql
-- Primary indexes
CREATE INDEX IX_Users_MobileNumber ON Users(MobileNumber) WHERE IsDeleted = 0;
CREATE INDEX IX_Tickets_VoterId ON Tickets(VoterId) WHERE IsDeleted = 0;
CREATE INDEX IX_Tickets_Status ON Tickets(Status) WHERE IsDeleted = 0;
CREATE INDEX IX_Tickets_AssemblyId ON Tickets(AssemblyId) WHERE IsDeleted = 0;

-- Composite indexes
CREATE INDEX IX_Tickets_VoterId_Status ON Tickets(VoterId, Status);
CREATE INDEX IX_ChatMessages_ChatId_CreatedAt ON ChatMessages(ChatId, CreatedAt DESC);
```

#### Query Optimization
- **Pagination**: All list endpoints paginated (default 20, max 100)
- **Selective columns**: Use projections, avoid SELECT *
- **Query caching**: Redis for frequently accessed data
- **Connection pooling**: Dapper connection pooling enabled

#### Database Scaling
- **Read Replicas**: Separate read/write databases
- **Sharding**: By Assembly ID for large-scale deployment
- **Partitioning**: Tables partitioned by date if needed

### 2. API Performance

#### Caching Strategy
```csharp
// Response Caching
[ResponseCache(Duration = 300)] // 5 minutes
public async Task<ActionResult> GetAssemblyList()

// In-Memory Cache (Redis in production)
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});
```

#### Async/Await
- **All I/O operations async**: Database, external services
- **Non-blocking operations**: Don't block threads
- **ConfigureAwait(false)**: For library code

#### Compression
- **Response Compression**: GZip/Brotli
- **Static Files**: CDN for images, videos

### 3. Horizontal Scaling

#### Stateless API
- **JWT tokens**: No server-side session storage
- **All API instances identical**: Share nothing
- **Load balancer ready**: Round-robin, sticky sessions (if needed)

#### Load Balancing
```
[Load Balancer]
    ├── API Instance 1
    ├── API Instance 2
    └── API Instance N
        └── [Database] / [Redis Cache]
```

#### Session State
- **JWT**: Stateless, no session storage
- **SignalR**: Redis backplane for multi-server
- **Cache**: Redis shared cache

### 4. Real-Time Communication (SignalR)

#### Scaling SignalR
```csharp
// Redis Backplane for multi-server
services.AddSignalR()
    .AddStackExchangeRedis("localhost:6379", options =>
    {
        options.Configuration.ChannelPrefix = "MVEA";
    });
```

#### Connection Management
- **Connection limits**: 10,000 concurrent connections per server
- **Message size limits**: 32KB per message
- **Heartbeat**: 30-second heartbeat interval

### 5. External Services

#### SMS/WhatsApp Service
- **Async processing**: Background jobs for bulk messages
- **Queue-based**: Azure Service Bus or RabbitMQ
- **Retry logic**: Exponential backoff (3 retries)
- **Rate limiting**: Respect provider limits

#### File Storage
- **Azure Blob Storage**: Scalable file storage
- **CDN**: Azure CDN for public assets
- **File size limits**: 10MB per file (configurable)

### 6. Monitoring & Observability

#### Application Insights
- **Telemetry**: Requests, dependencies, exceptions
- **Performance counters**: CPU, memory, response times
- **Custom metrics**: Business metrics (tickets created, etc.)

#### Logging
- **Structured logging**: JSON format with Serilog
- **Log levels**: Error, Warning, Information
- **Centralized logging**: Application Insights or ELK stack

#### Health Checks
```csharp
services.AddHealthChecks()
    .AddSqlServer(connectionString, name: "database")
    .AddRedis(redisConnection, name: "cache");

app.MapHealthChecks("/health");
```

---

## Performance Targets

### API Response Times
- **P50 (Median)**: < 200ms
- **P95**: < 500ms
- **P99**: < 1000ms

### Throughput
- **Read operations**: 10,000 requests/second
- **Write operations**: 1,000 requests/second
- **Concurrent users**: 50,000+

### Availability
- **Uptime SLA**: 99.9% (8.76 hours downtime/year)
- **Graceful degradation**: Partial functionality if services fail
- **Auto-scaling**: Scale up/down based on load

---

## Scalability Architecture

```
                    [Load Balancer]
                          |
        ┌─────────────────┼─────────────────┐
        |                 |                 |
   [API Server 1]   [API Server 2]   [API Server N]
        |                 |                 |
        └─────────────────┼─────────────────┘
                          |
              ┌───────────┴───────────┐
              |                       |
       [Primary DB]             [Read Replica]
              |                       |
        ┌─────┴─────┐                |
    [Redis Cache] [Blob Storage]     |
              |                      |
        [SignalR Backplane]          |
```

---

## Security Checklist

- ✅ HTTPS/TLS 1.3 enabled
- ✅ JWT token validation
- ✅ SQL injection prevention (parameterized queries)
- ✅ XSS protection (input sanitization)
- ✅ CORS configured
- ✅ Rate limiting enabled
- ✅ Input validation (FluentValidation)
- ✅ File upload validation
- ✅ Audit logging enabled
- ✅ Error messages don't expose sensitive data
- ✅ Secure password hashing (BCrypt)
- ✅ Session timeout configured
- ✅ Database encryption at rest
- ✅ Connection string encryption

---

## Scalability Checklist

- ✅ Async/await for all I/O operations
- ✅ Pagination on list endpoints
- ✅ Response caching configured
- ✅ Database indexes on key columns
- ✅ Connection pooling enabled
- ✅ Stateless API design
- ✅ Health checks implemented
- ✅ Monitoring/telemetry configured
- ✅ Load balancer ready
- ✅ Redis cache for frequent data
- ✅ CDN for static assets
