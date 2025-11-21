using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using MyTodos.BuildingBlocks.Presentation;
using MyTodos.Services.IdentityService.Application;
using MyTodos.Services.IdentityService.Infrastructure;
using MyTodos.Services.IdentityService.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// Add Application layer services
builder.Services.AddIdentityServiceApplication();

// Add Infrastructure layer services
builder.Services.AddIdentityServiceInfrastructure(builder.Configuration);

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };

    // Don't challenge on endpoints with AllowAnonymous
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnChallenge = context =>
        {
            var endpoint = context.HttpContext.GetEndpoint();
            var allowAnonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null;

            if (allowAnonymous)
            {
                // Clear the failure and skip challenge for anonymous endpoints
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status200OK;
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    };
});

// Add BuildingBlocks Presentation layer (Controllers, Exception Handling, Permission Authorization, Swagger, CORS)
builder.Services.AddBuildingBlocksPresentation(
    builder.Configuration,
    builder.Environment,
    "MyTodos IdentityService API",
    "v1",
    "Identity and Access Management service - handles users, roles, permissions, and authentication");

// Add Health Checks
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration.GetConnectionString("IdentityServiceDb")
    ?? "Data Source=identityservice.db";

var rabbitMqHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
var rabbitMqUsername = builder.Configuration["RabbitMQ:Username"] ?? "guest";
var rabbitMqPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";

builder.Services.AddHealthChecks()
    .AddSqlite(connectionString, name: "database")
    .AddRabbitMQ(
        rabbitConnectionString: $"amqp://{rabbitMqUsername}:{rabbitMqPassword}@{rabbitMqHost}:5672",
        name: "rabbitmq");

var app = builder.Build();

// Configure the HTTP request pipeline

// Health check middleware - MUST BE FIRST to bypass all other middleware
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/health")
    {
        context.Response.StatusCode = 200;
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync("Healthy");
        return;
    }
    await next();
});

// BuildingBlocks middleware (exception handling, Swagger, CORS)
app.UseBuildingBlocksPresentation(app.Environment);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map health check with AllowAnonymous to bypass FallbackPolicy
app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.Run();
