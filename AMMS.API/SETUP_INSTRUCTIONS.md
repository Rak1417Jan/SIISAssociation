# MVEA API - Setup Instructions

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or SQL Server Express)
- Visual Studio 2022 or VS Code with C# extension

## Required NuGet Packages

Add the following packages to `MVEA.API.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
  <PackageReference Include="Dapper" Version="2.1.28" />
  <PackageReference Include="Microsoft.Data.SqlClient" Version="5.1.2" />
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Versioning" Version="5.1.0" />
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer" Version="5.1.0" />
  <PackageReference Include="AutoMapper" Version="12.0.1" />
  <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
  <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
  <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
  <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.1.0" />
  <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.0" />
</ItemGroup>
```

## Configuration

### appsettings.json Configuration

Add connection string and settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MVEA;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "YourSecretKeyHere_Minimum32CharactersForHS256",
    "Issuer": "MVEA.API",
    "Audience": "MVEA.Users",
    "ExpirationInMinutes": 1440,
    "RefreshTokenExpirationInDays": 7
  }
}
```

### Program.cs Configuration

See `API/Extensions/ServiceCollectionExtensions.cs` for service registration examples.

## Dependency Injection Setup

Register services in `Program.cs`:

```csharp
// Add Infrastructure
builder.Services.AddScoped<DapperContext>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();

// Add Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITicketService, TicketService>();

// Add API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});
```

## Database Setup

1. Create database:
```sql
CREATE DATABASE MVEA;
```

2. Run SQL scripts (to be created in `Infrastructure/Data/Scripts/`)

3. Connection string format:
```
Server=(localdb)\mssqllocaldb;Database=MVEA;Trusted_Connection=True;MultipleActiveResultSets=true
```

## Running the Application

```bash
dotnet restore
dotnet build
dotnet run
```

API will be available at: `https://localhost:5001` or `http://localhost:5000`

Swagger UI: `https://localhost:5001/swagger`

## Project Structure

```
MVEA.API/
├── Domain/                    # Business entities and interfaces
├── Application/               # Use cases, DTOs, services
├── Infrastructure/            # Data access, external services
├── API/                      # Controllers, middleware
└── Shared/                   # Shared utilities
```
