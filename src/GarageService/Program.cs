using Microsoft.AspNetCore.Mvc;
using GarageService.Data;
using GarageService.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add database context
builder.Services.AddDbContext<GarageDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services for Garage operations
builder.Services.AddScoped<IGarageService, GarageService.Services.GarageService>();

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
    var db = scope.ServiceProvider.GetRequiredService<GarageDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/garage/openapi/{documentName}.json");
    app.MapScalarApiReference("/garage/scalar", options =>
    {
        options.WithOpenApiRoutePattern("/garage/openapi/{documentName}.json");
    });
    app.MapGet("/garage/scalar", () => Results.Redirect("/garage/scalar/v1")).ExcludeFromDescription();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Define endpoints for Garage operations
app.MapPost("/garage/motorcycles", async ([FromHeader(Name = "x-user-id")] int userId, MotorcycleDto motorcycle, IGarageService garageService) =>
{
    await garageService.AddMotorcycleAsync(motorcycle, userId);
    return Results.Created($"/garage/motorcycles/{motorcycle.Id}", motorcycle);
}).WithName("AddMotorcycle").RequireAuthorization();

app.MapGet("/garage/motorcycles", async ([FromHeader(Name = "x-user-id")] int userId, IGarageService garageService) =>
{
    var motorcycles = await garageService.GetMotorcyclesAsync(userId);
    return Results.Ok(motorcycles);
}).WithName("GetMotorcycles");

app.MapGet("/garage/catalog", async (IGarageService garageService) =>
{
    var catalog = await garageService.GetGlobalCatalogAsync();
    return Results.Ok(catalog);
}).WithName("GetGlobalCatalog");

app.Run();
