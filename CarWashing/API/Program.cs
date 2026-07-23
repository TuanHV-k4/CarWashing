using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using API.Helpers;
using API.Middlewares;
using API.Services;
using BusinessLayer.Helpers;
using BusinessLayer.IService;
using BusinessLayer.IService.AI;
using BusinessLayer.IService.Loyalty;
using BusinessLayer.IService.Operations;
using BusinessLayer.Service;
using BusinessLayer.Service.AI;
using BusinessLayer.Service.Loyalty;
using BusinessLayer.Service.Operations;
using BusinessLayer.Validators;
using DataAccessLayer.Context;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ─── CORS ────────────────────────────────────────────────────
var corsSettings = builder.Configuration.GetSection("CorsSettings").Get<CorsSettings>() ?? new CorsSettings();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        if (corsSettings.AllowedOrigins.Length > 0)
        {
            policy.WithOrigins(corsSettings.AllowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// ─── Controllers ─────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
    });
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IOperationsService, OperationsService>();
builder.Services.AddScoped<IStaffingService, StaffingService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IBranchWorkforceService, BranchWorkforceService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IWorkloadReportService, WorkloadReportService>();
builder.Services.AddScoped<IBookingReadService>(provider => provider.GetRequiredService<IOperationsService>() as IBookingReadService
    ?? throw new InvalidOperationException("Operations service must implement IBookingReadService."));
builder.Services.AddScoped<ILoyaltyService, LoyaltyService>();
builder.Services.AddScoped<ILoyaltyTierQuery>(provider => provider.GetRequiredService<ILoyaltyService>());
builder.Services.AddScoped<IWashCompletionService>(provider => (LoyaltyService)provider.GetRequiredService<ILoyaltyService>());
builder.Services.AddScoped<ILoyaltyMaintenanceService, LoyaltyMaintenanceService>();
builder.Services.AddHostedService<LoyaltyMaintenanceHostedService>();

// ─── Swagger + JWT Bearer Authorization ──────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AutoWash Pro API",
        Version = "v1",
        Description = "Smart Car Washing Management System API"
    });

    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: eyJhbGciOiJIUzI1NiIs...",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ─── Database ────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseNpgsql(
        builder.Configuration.GetConnectionString("MyDB"))
        .AddInterceptors(new UtcDateTimeCommandInterceptor());
});

// ─── JWT Configuration ───────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

var secretKey = jwtSettings["SecretKey"]!;
var issuer = jwtSettings["Issuer"]!;
var audience = jwtSettings["Audience"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var version = context.Principal?.FindFirst("auth_version")?.Value;
            if (!Guid.TryParse(userId, out var id) || !int.TryParse(version, out var tokenVersion)) { context.Fail("Invalid session."); return; }
            var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            var currentVersion = await db.Users.Where(user => user.UserID == id).Select(user => (int?)user.AuthVersion).FirstOrDefaultAsync(context.HttpContext.RequestAborted);
            if (currentVersion is null || currentVersion != tokenVersion) context.Fail("Session has expired.");
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
    options.AddPolicy("StaffOrAdmin", policy => policy.RequireRole("Staff", "Admin"));
    options.AddPolicy("StaffManagerOrAdmin", policy => policy.RequireRole("Staff", "BranchManager", "Admin"));
    options.AddPolicy("BranchManagerOrAdmin", policy => policy.RequireRole("BranchManager", "Admin"));
    options.AddPolicy("CatalogRead", policy => policy.RequireRole("Admin", "BranchManager"));
});

// ─── Rate Limiting (AI) ──────────────────────────────────────
var aiSettings = builder.Configuration.GetSection("AiSettings").Get<AiSettings>() ?? new AiSettings();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/problem+json";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            title = "Too Many Requests",
            detail = "Bạn đang gửi quá nhiều yêu cầu AI. Vui lòng chờ một chút rồi thử lại.",
            status = StatusCodes.Status429TooManyRequests
        }, cancellationToken);
    };
    options.AddPolicy("AiCustomer", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = aiSettings.RateLimitPerMinute,
                Window = TimeSpan.FromMinutes(1)
            }));
    options.AddPolicy("AiAdmin", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.User.Identity?.Name ?? "admin-anon",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = aiSettings.AdminRateLimitPerMinute,
                Window = TimeSpan.FromMinutes(1)
            }));
    options.AddPolicy("PasswordReset", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 3, Window = TimeSpan.FromMinutes(15), QueueLimit = 0 }));
});

// ─── FluentValidation ────────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// ─── Configuration ───────────────────────────────────────────
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<PasswordResetSettings>(builder.Configuration.GetSection("PasswordResetSettings"));
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));
builder.Services.Configure<AiSettings>(builder.Configuration.GetSection("AiSettings"));
builder.Services.Configure<AttendanceSettings>(builder.Configuration.GetSection("AttendanceSettings"));

// ─── Dependency Injection ────────────────────────────────────
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ICurrentCustomerService, CurrentCustomerService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IVehicleOwnershipValidator, VehicleOwnershipValidator>();
builder.Services.AddScoped<IWashHistoryService, WashHistoryService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IBehavioralLogService, BehavioralLogService>();
builder.Services.AddScoped<IBehavioralLogWriter>(provider => provider.GetRequiredService<IBehavioralLogService>() as IBehavioralLogWriter
    ?? throw new InvalidOperationException("Behavioral log service must implement IBehavioralLogWriter."));
builder.Services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
builder.Services.AddScoped<IAdminDashboardReadService, AdminDashboardReadService>();
builder.Services.AddScoped<IServiceBusinessService, ServiceBusinessService>();

// AI layer
builder.Services.AddSingleton<AiConversationStore>();
builder.Services.AddHttpClient<IGenerativeAIClient, GeminiClient>();
if (aiSettings.UseMockCustomerContext)
{
    builder.Services.AddScoped<ICustomerAIContextProvider, MockCustomerAIContextProvider>();
    builder.Services.AddScoped<IServiceSuggestionContextProvider>(provider => (IServiceSuggestionContextProvider)provider.GetRequiredService<ICustomerAIContextProvider>());
}
else
{
    builder.Services.AddScoped<ICustomerAIContextProvider, CustomerAIContextProvider>();
    builder.Services.AddScoped<IServiceSuggestionContextProvider>(provider => (IServiceSuggestionContextProvider)provider.GetRequiredService<ICustomerAIContextProvider>());
}
builder.Services.AddScoped<IAdminAIContextProvider, AdminAIContextProvider>();
builder.Services.AddScoped<IAIService, AiService>();
builder.Services.AddScoped<IAiInsightsService, AiInsightsService>();

// ─── Build ───────────────────────────────────────────────────
var app = builder.Build();

// ─── Middleware Pipeline ─────────────────────────────────────
app.UseGlobalExceptionHandling();

app.UseCors("DefaultCors");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AutoWash Pro API v1");
});

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
