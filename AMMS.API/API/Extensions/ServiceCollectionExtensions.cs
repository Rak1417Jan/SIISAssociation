using MVEA.Application.Interfaces;
using MVEA.Application.Services;
using MVEA.Domain.Interfaces;
using MVEA.Infrastructure.Data;
using MVEA.Infrastructure.Data.Repositories;
using MVEA.Infrastructure.Data.UnitOfWork;

namespace MVEA.API.Extensions;

/// <summary>
/// Extension methods for service registration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all infrastructure services (Dapper, Unit of Work, Repositories)
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Dapper Context
        services.AddScoped<DapperContext>();
        
        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMLARepository, MLARepository>();
        services.AddScoped<IAssemblyRepository, AssemblyRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IContentRepository, ContentRepository>();
        services.AddScoped<IPostEngagementRepository, PostEngagementRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationDeliveryRepository, NotificationDeliveryRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITicketAttachmentRepository, TicketAttachmentRepository>();
        services.AddScoped<ITicketCommentRepository, TicketCommentRepository>();
        services.AddScoped<IVoterRepository, VoterRepository>();
        services.AddScoped<IFamilyMemberRepository, FamilyMemberRepository>();
        // services.AddScoped<IBoothRepository, BoothRepository>();
        
        return services;
    }

    /// <summary>
    /// Register all application services
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IMLAService, MLAService>();
        services.AddScoped<IAssemblyService, AssemblyService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IContentService, ContentService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IVoterService, VoterService>();
        // Add other services here
        
        return services;
    }

    /// <summary>
    /// Configure API versioning
    /// </summary>
    public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });

        //services.AddVersionedApiExplorer(setup =>
        //{
        //    setup.GroupNameFormat = "'v'VVV";
        //    setup.SubstituteApiVersionInUrl = true;
        //});

        return services;
    }
    // Replace this line:
    // services.AddVersionedApiExplorer(setup =>

    // With this line:
   
    /// <summary>
    /// Configure Swagger with API versioning
    /// </summary>
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "MVEA API",
                Version = "v1",
                Description = "MLA–Voter Engagement Application API",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "MVEA Team"
                }
            });

            // Add JWT Bearer authentication to Swagger
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Configure CORS
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });

            // Production CORS policy (restrictive)
            options.AddPolicy("Production", policy =>
            {
                policy.WithOrigins("https://mvea.app", "https://www.mvea.app")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        return services;
    }
}
