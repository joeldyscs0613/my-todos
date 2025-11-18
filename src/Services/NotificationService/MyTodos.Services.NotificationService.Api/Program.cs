using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MyTodos.BuildingBlocks.Presentation;
using MyTodos.Services.NotificationService.Application;
using MyTodos.Services.NotificationService.Infrastructure;
using MyTodos.Services.NotificationService.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Add Application layer services
builder.Services.AddNotificationServiceApplication();

// Add Infrastructure layer services
builder.Services.AddNotificationServiceInfrastructure(builder.Configuration);

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"];

if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT SecretKey is not configured.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Add BuildingBlocks Presentation layer (Controllers, Exception Handling, Permission Authorization, Swagger, CORS)
builder.Services.AddBuildingBlocksPresentation(
    builder.Configuration,
    builder.Environment,
    "MyTodos NotificationService API",
    "v1",
    "Notification service - handles notifications, alerts, and communication");

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeederService>();
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline
// BuildingBlocks middleware (exception handling, Swagger, CORS)
app.UseBuildingBlocksPresentation(app.Environment);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
