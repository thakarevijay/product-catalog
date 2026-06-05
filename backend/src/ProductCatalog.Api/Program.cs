using HealthChecks.UI.Client;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using ProductCatalog.Application;
using ProductCatalog.Infrastructure;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .WriteTo.Console()
    .ReadFrom.Configuration(context.Configuration));

// Application Insights
var aiOptions = new ApplicationInsightsServiceOptions
{
    ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]
};
builder.Services.AddApplicationInsightsTelemetry(aiOptions);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "database",
        tags: new[] { "db", "sql" });

builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
            "http://localhost:4200",
            "https://zealous-sea-0b48d2a03.7.azurestaticapps.net")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseSerilogRequestLogging();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db")
});

app.Run();

Log.CloseAndFlush();
