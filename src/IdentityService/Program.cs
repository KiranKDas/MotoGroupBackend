using IdentityService.Models;
using IdentityService.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add database context
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Add authentication services
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

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Rider", policy => policy.RequireRole("Rider"));
    options.AddPolicy("ClubCaptain", policy => policy.RequireRole("ClubCaptain"));
});

var app = builder.Build();

// Apply EF Core migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/identity/openapi/{documentName}.json");
    app.MapScalarApiReference("/identity/scalar", options =>
    {
        options.WithOpenApiRoutePattern("/identity/openapi/{documentName}.json");
    });
    app.MapGet("/identity/scalar", () => Results.Redirect("/identity/scalar/v1")).ExcludeFromDescription();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Define endpoints for user registration and login
app.MapPost("/identity/register", async (UserDto userDto, IdentityDbContext db) =>
{
    if (await db.Users.AnyAsync(u => u.Username == userDto.Username))
    {
        return Results.BadRequest("Username already exists.");
    }

    // It is critical to hash passwords before storing them.
    var passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

    var user = new User
    {
        Username = userDto.Username,
        PasswordHash = passwordHash, // Store the secure hash, not the plain-text password.
        Role = userDto.Role // Explicitly use the role from the DTO.
    };
    
    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok("User registered successfully.");
}).WithName("RegisterUser");

app.MapPost("/identity/login", async (LoginDto loginDto, IdentityDbContext db, IConfiguration config) =>
{
    // Find user by username first
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);

    // Verify the user exists and that the provided password matches the stored hash.
    if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    // Actual JWT token generation
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(config["Jwt:Key"] ?? "Your_Super_Secret_Key_For_JWT_1234567890");
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return Results.Ok(new { Token = tokenHandler.WriteToken(token), Role = user.Role, UserId = user.Id });
}).WithName("LoginUser");

app.MapPut("/identity/users/{username}/role", async (string username, RoleUpdateDto roleUpdate, IdentityDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    user.Role = roleUpdate.Role;
    await db.SaveChangesAsync();

    return Results.Ok(new { Message = "User role updated successfully.", NewRole = user.Role });
}).WithName("UpdateUserRole").RequireAuthorization();

app.Run();
