using AspNetCoreRateLimit;
using Finitech.ApiHost.Configuration;
using Finitech.ApiHost.Services;
using Finitech.Modules.Ledger.Infrastructure;
using Serilog;

// Make Outbox background services available
using Finitech.BuildingBlocks.Infrastructure.Outbox;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
SerilogConfig.ConfigureSerilog(builder);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Finitech API",
        Version = "v1",
        Description = "Production-ready FinTech API Platform"
    });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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

// Security
builder.Services.AddSecurityConfig(builder.Configuration);

// Health Checks
builder.Services.AddHealthChecksConfig(builder.Configuration);

// OpenTelemetry
builder.Services.AddOpenTelemetryConfig(builder.Configuration, builder.Environment);

// Ledger Module
builder.Services.AddLedgerInfrastructure(builder.Configuration);

// Register other services (currently in-memory for demo)
builder.Services.AddSingleton<Finitech.Modules.IdentityAccess.Contracts.IIdentityAccessService, IdentityAccessService>();
builder.Services.AddSingleton<Finitech.Modules.IdentityCompliance.Contracts.IIdentityComplianceService, IdentityComplianceService>();
builder.Services.AddSingleton<Finitech.Modules.Banking.Contracts.IBankingService, BankingService>();
builder.Services.AddSingleton<Finitech.Modules.Wallet.Contracts.IWalletService, WalletService>();
builder.Services.AddSingleton<Finitech.Modules.WalletFMCG.Contracts.IWalletFMCGService, WalletFMCGService>();
builder.Services.AddSingleton<Finitech.Modules.MerchantPayments.Contracts.IMerchantPaymentsService, MerchantPaymentsService>();
builder.Services.AddSingleton<Finitech.Modules.Payments.Contracts.IPaymentsService, PaymentsService>();
builder.Services.AddSingleton<Finitech.Modules.PartyRegistry.Contracts.IPartyRegistryService, PartyRegistryService>();
builder.Services.AddSingleton<Finitech.Modules.FX.Contracts.IFXService, FXService>();
builder.Services.AddSingleton<Finitech.Modules.BranchNetwork.Contracts.IBranchNetworkService, BranchNetworkService>();
builder.Services.AddSingleton<Finitech.Modules.Notifications.Contracts.INotificationsService, NotificationsService>();
builder.Services.AddSingleton<Finitech.Modules.Disputes.Contracts.IDisputesService, DisputesService>();
builder.Services.AddSingleton<Finitech.Modules.Audit.Contracts.IAuditService, AuditService>();

// Hosted Services
builder.Services.AddHostedService<SeedDataService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSecurityHeaders();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("ProductionCors");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("self")
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db") || check.Tags.Contains("self")
});

// Prometheus metrics endpoint
app.MapPrometheusScrapingEndpoint();

// Global exception handling
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Unhandled exception occurred");
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred" });
    }
});

app.Run();
