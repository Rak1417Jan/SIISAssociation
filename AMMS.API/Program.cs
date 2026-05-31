using AMMS.API.Background;
using AMMS.API.Security;
using System.Threading.Channels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MVEA.Repository.Infrastructure;
using MVEA.Repository.Interfaces;
using MVEA.Repository.IRepository;
using MVEA.Repository.Repositories;
using MVEA.Repository.Repository;
using MVEA.Repository.UnitOfWork;
using MVEA.Application.Services;
using MVEA.Services.Interfaces;
using MVEA.Services.IService;
using MVEA.Services.Messaging;
using MVEA.Services.Service;
using MVEA.Services.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Register API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userIdValue = context.Principal?.FindFirst("UserId")?.Value;
                var jti = context.Principal?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;

                if (!int.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(jti))
                {
                    return;
                }

                using var scope = context.HttpContext.RequestServices.CreateScope();
                var deny = scope.ServiceProvider.GetRequiredService<ITokenDenylistRepository>();
                if (await deny.IsDeniedAsync(userId, jti, context.HttpContext.RequestAborted))
                {
                    context.Fail("Token revoked.");
                }
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MinRole:Support", policy => policy.Requirements.Add(new MinRoleRequirement(RoleLevel.Support)));
    options.AddPolicy("MinRole:Finance", policy => policy.Requirements.Add(new MinRoleRequirement(RoleLevel.Finance)));
    options.AddPolicy("MinRole:Manager", policy => policy.Requirements.Add(new MinRoleRequirement(RoleLevel.Manager)));
    options.AddPolicy("MinRole:Admin", policy => policy.Requirements.Add(new MinRoleRequirement(RoleLevel.Admin)));
    options.AddPolicy("MinRole:SuperAdmin", policy => policy.Requirements.Add(new MinRoleRequirement(RoleLevel.SuperAdmin)));
});

builder.Services.AddSingleton<IAuthorizationHandler, MinRoleRequirementHandler>();

builder.Services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOFWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStaffPasswordResetNotifier, LoggingStaffPasswordResetNotifier>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMasterRepository, MasterRepository>();
builder.Services.AddScoped<IMasterService, MasterService>();
builder.Services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
builder.Services.AddScoped<IAdminDashboardService, MVEA.Services.Service.AdminDashboardService>();
builder.Services.AddScoped<IAdminMembersRepository, AdminMembersRepository>();
builder.Services.AddScoped<IAdminMembersService, MVEA.Services.Service.AdminMembersService>();
builder.Services.AddScoped<IAdminFirmsRepository, AdminFirmsRepository>();
builder.Services.AddScoped<IAdminFirmsService, MVEA.Services.Service.AdminFirmsService>();
builder.Services.AddScoped<ICompanyTypeRepository, CompanyTypeRepository>();
builder.Services.AddScoped<ICompanyTypeService, CompanyTypeService>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IStaffService, MVEA.Services.Service.StaffService>();
builder.Services.AddScoped<IRolesRepository, RolesRepository>();
builder.Services.AddScoped<IRolesService, MVEA.Services.Service.RolesService>();
builder.Services.AddScoped<ITokenDenylistRepository, TokenDenylistRepository>();
builder.Services.AddScoped<IBroadcastRepository, BroadcastRepository>();
builder.Services.AddScoped<IMemberNotificationsRepository, MemberNotificationsRepository>();
builder.Services.AddScoped<IBroadcastService, BroadcastService>();
builder.Services.AddScoped<IMemberNotificationsService, MemberNotificationsService>();

builder.Services.AddScoped<IPlatformRepository, PlatformRepository>();
builder.Services.AddScoped<IPlatformService, PlatformService>();
builder.Services.AddScoped<IOutboundNotifier, LoggingOutboundNotifier>();
builder.Services.AddScoped<AuditLogWriter>();

builder.Services.AddHttpClient("Razorpay", client =>
{
    client.BaseAddress = new Uri("https://api.razorpay.com/v1/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

Channel<int> broadcastChannel = Channel.CreateBounded<int>(new BoundedChannelOptions(500)
{
    FullMode = BoundedChannelFullMode.Wait
});
builder.Services.AddSingleton(broadcastChannel);
builder.Services.AddSingleton(broadcastChannel.Writer);
builder.Services.AddSingleton(broadcastChannel.Reader);
builder.Services.AddSingleton<IBroadcastDispatchQueue, BroadcastDispatchQueue>();
builder.Services.AddHostedService<BroadcastDispatchHostedService>();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
