using ClubService.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure PostgreSQL database context (ensure you add the connection string to appsettings.json)
builder.Services.AddDbContext<ClubDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Port=5436;Database=clubdb;Username=motouser;Password=motopassword"));

// Configure CORS so the React frontend can make requests
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();  

// Apply EF Core migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClubDbContext>();
    db.Database.Migrate();
}

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options => { options.RouteTemplate = "clubs/openapi/{documentName}.json"; });
    app.MapScalarApiReference("/clubs/scalar", options =>
    {
        options.WithOpenApiRoutePattern("/clubs/openapi/{documentName}.json");
    });
    app.MapGet("/clubs/scalar", () => Results.Redirect("/clubs/scalar/v1")).ExcludeFromDescription();
}

app.MapControllers();
app.Run();