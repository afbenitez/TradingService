using Microsoft.EntityFrameworkCore;
using Serilog;
using FluentValidation;
using TradingService.Data;
using TradingService.Services;
using TradingService.Services.Interfaces;
using TradingService.Mappings;
using TradingService.Validators;
using TradingService.Models.DTOs;
using System.Reflection;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/trading-service-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Configure URLs explicitly to ensure consistent behavior
builder.WebHost.UseUrls("https://localhost:7179", "http://localhost:5100");

// Add Serilog
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Configure Database
// DEFAULT: PostgreSQL for persistent data storage
// OPTION: In-Memory database for testing/development (see instructions below)
var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase", false);

if (useInMemoryDatabase)
{
    // TO USE IN-MEMORY DATABASE:
    // 1. Set "UseInMemoryDatabase": true in appsettings.json or appsettings.Development.json
    // 2. Or set environment variable: UseInMemoryDatabase=true
    // 3. Or uncomment the lines below and comment the PostgreSQL section
    
    builder.Services.AddDbContext<TradingDbContext>(options =>
        options.UseInMemoryDatabase("TradingDb"));
    Log.Information("[DATABASE] Using In-Memory Database (data will be lost on restart)");
}
else
{
    // PostgreSQL Database (DEFAULT)
    // Requires PostgreSQL to be running (use docker-compose up postgres -d)
    builder.Services.AddDbContext<TradingDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    Log.Information("[DATABASE] Using PostgreSQL Database");
}

// ALTERNATIVE: Force In-Memory Database (uncomment to override configuration)
// builder.Services.AddDbContext<TradingDbContext>(options =>
//     options.UseInMemoryDatabase("TradingDb"));
// Log.Information("[DATABASE] Using In-Memory Database (forced override)");

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(TradeMappingProfile));

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateTradeDtoValidator>();

// Add custom services
builder.Services.AddScoped<ITradeService, TradeService>();

// Add RabbitMQ service with error handling
try
{
    builder.Services.AddSingleton<IMessageQueueService, RabbitMQService>();
    Log.Information("[RABBITMQ] RabbitMQ service registered");
}
catch (Exception ex)
{
    Log.Warning(ex, "[RABBITMQ] Could not initialize RabbitMQ service - continuing without message queue");
    // Register a dummy service if RabbitMQ fails
    builder.Services.AddSingleton<IMessageQueueService, DummyMessageQueueService>();
}

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MeDirect Trading Service API",
        Version = "v1",
        Description = "A microservice for handling trading operations with PostgreSQL persistence and RabbitMQ messaging",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "MeDirect Trading Service Team",
            Email = "af.benitez@uniandes.edu.co"
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add health checks with error handling
try
{
    var rabbitConnectionString = builder.Configuration.GetConnectionString("RabbitMQ") ?? "amqp://guest:guest@localhost:5672";
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<TradingDbContext>()
        .AddRabbitMQ(rabbitConnectionString, name: "rabbitmq");
}
catch (Exception ex)
{
    Log.Warning(ex, "[HEALTHCHECK] Could not configure RabbitMQ health check - continuing with basic health checks");
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<TradingDbContext>();
}

var app = builder.Build();

// Configure Swagger to be available in all environments for API testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Trading Service API v1");
    c.RoutePrefix = ""; // Serve Swagger UI at the app's root (/)
});

// Database initialization with better error handling
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
    
    if (useInMemoryDatabase)
    {
        // For in-memory database, just ensure it's created
        context.Database.EnsureCreated();
        Log.Information("[SUCCESS] In-Memory database initialized");
    }
    else
    {
        // For PostgreSQL, apply migrations or create database
        try
        {
            context.Database.Migrate();
            Log.Information("[SUCCESS] PostgreSQL migrations applied successfully");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "[WARNING] Could not apply migrations, attempting to create database");
            try
            {
                context.Database.EnsureCreated();
                Log.Information("[SUCCESS] PostgreSQL database created");
            }
            catch (Exception createEx)
            {
                Log.Error(createEx, "[ERROR] Failed to initialize database. Please ensure PostgreSQL is running.");
                Log.Warning("[TIP] Suggestion: Set UseInMemoryDatabase=true in appsettings.Development.json for easier testing");
                throw;
            }
        }
    }
}
catch (Exception ex)
{
    Log.Error(ex, "[ERROR] Database initialization failed");
    throw;
}

// Add logging middleware first
app.UseSerilogRequestLogging();

// Configure CORS before routing
app.UseCors("AllowAll");

// HTTPS redirection - Only in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Authorization
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Add health check endpoint
app.MapHealthChecks("/health");

// Add a simple root endpoint for testing
app.MapGet("/", () => "[RUNNING] MeDirect Trading Service is running! Go to /swagger for API documentation.");

try
{
    Log.Information("[STARTUP] Starting MeDirect Trading Service");
    Log.Information("[INFO] Environment: {Environment}", app.Environment.EnvironmentName);
    Log.Information("[INFO] Database: {DatabaseType}", useInMemoryDatabase ? "In-Memory" : "PostgreSQL");
    Log.Information("[INFO] Swagger UI: Available at root URL");
    Log.Information("[INFO] Health checks: /health");
    
    // Log the URLs where the service will be available
    Log.Information("[URLS] Service URLs:");
    Log.Information("   [HTTP]  HTTP:  http://localhost:5100");
    Log.Information("   [HTTPS] HTTPS: https://localhost:7179");
    Log.Information("[TIP] If HTTPS fails, use HTTP URL or trust the dev certificate with: dotnet dev-certs https --trust");
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "[FATAL] Trading Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Entry point class for the MeDirect Trading Service application.
/// This partial class declaration is required for integration testing to access the application's configuration.
/// </summary>
public partial class Program { }
