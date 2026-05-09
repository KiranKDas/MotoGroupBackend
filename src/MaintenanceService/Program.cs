using Microsoft.AspNetCore.Mvc;
using MaintenanceService.Data;
using MaintenanceService.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add database context
builder.Services.AddDbContext<MaintenanceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services for Maintenance operations
builder.Services.AddScoped<IMaintenanceService, MaintenanceService.Services.MaintenanceService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? "Your_Super_Secret_Key_For_JWT_1234567890");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Apply EF Core migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MaintenanceDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/maintenance/openapi/{documentName}.json");
    app.MapScalarApiReference("/maintenance/scalar", options =>
    {
        options.WithOpenApiRoutePattern("/maintenance/openapi/{documentName}.json");
    });
    app.MapGet("/maintenance/scalar", () => Results.Redirect("/maintenance/scalar/v1")).ExcludeFromDescription();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Define endpoints for Maintenance operations
app.MapPost("/maintenance/logs", async ([FromHeader(Name = "x-user-id")] int userId, MaintenanceLogDto log, IMaintenanceService maintenanceService) =>
{
    await maintenanceService.AddMaintenanceLogAsync(log, userId);
    return Results.Created($"/maintenance/logs/{log.MotorcycleId}", log);
}).WithName("AddMaintenanceLog").RequireAuthorization();

app.MapGet("/maintenance/logs/{motorcycleId}", async ([FromHeader(Name = "x-user-id")] int userId, int motorcycleId, IMaintenanceService maintenanceService) =>
{
    var logs = await maintenanceService.GetMaintenanceLogsAsync(motorcycleId, userId);
    return Results.Ok(logs);
}).WithName("GetMaintenanceLogs");

app.MapGet("/maintenance/status/{motorcycleId}", async ([FromHeader(Name = "x-user-id")] int userId, int motorcycleId, IMaintenanceService maintenanceService) =>
{
    var isSafe = await maintenanceService.CalculateSafetyStatusAsync(motorcycleId, userId);
    return Results.Ok(new { IsSafe = isSafe });
}).WithName("GetSafetyStatus");

app.Run();
