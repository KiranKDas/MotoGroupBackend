using Microsoft.AspNetCore.Mvc;
using EventService.Data;
using EventService.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add database context
builder.Services.AddDbContext<EventDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services for Event operations
builder.Services.AddScoped<IEventService, EventService.Services.EventService>();

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
    var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/events/openapi/{documentName}.json");
    app.MapScalarApiReference("/events/scalar", options =>
    {
        options.WithOpenApiRoutePattern("/events/openapi/{documentName}.json");
    });
    app.MapGet("/events/scalar", () => Results.Redirect("/events/scalar/v1")).ExcludeFromDescription();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Define endpoints for Event operations
app.MapPost("/events", async ([FromHeader(Name = "x-user-id")] int userId, EventDto eventDto, IEventService eventService) =>
{
    await eventService.CreateEventAsync(eventDto, userId);
    return Results.Created($"/events/{eventDto.Name}", eventDto);
}).WithName("CreateEvent").RequireAuthorization();

app.MapGet("/events/upcoming", async ([FromHeader(Name = "x-user-id")] int userId, IEventService eventService) =>
{
    var events = await eventService.GetUpcomingEventsAsync(userId);
    return Results.Ok(events);
}).WithName("GetUpcomingEvents");

app.MapGet("/events/attended", async ([FromHeader(Name = "x-user-id")] int userId, IEventService eventService) =>
{
    var events = await eventService.GetAttendedEventsAsync(userId);
    return Results.Ok(events);
}).WithName("GetAttendedEvents");

app.Run();
