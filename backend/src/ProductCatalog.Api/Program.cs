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

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
            "http://localhost:4200",
            "https://zealous-sea-0b48d2a03.7.azurestaticapps.net")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

// Enable Scalar in all environments for portfolio demo
app.MapOpenApi();
app.MapScalarApiReference();

app.UseSerilogRequestLogging();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();

Log.CloseAndFlush();
