using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using MyTodos.BuildingBlocks.Presentation;
using MyTodos.Services.TodoService.Application;
using MyTodos.Services.TodoService.Infrastructure;
using MyTodos.Services.TodoService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add Application layer services
builder.Services.AddTodoServiceApplication();

// Add Infrastructure layer services
builder.Services.AddTodoServiceInfrastructure(builder.Configuration);

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

        // Don't challenge on endpoints with AllowAnonymous
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnChallenge = context =>
            {
                var endpoint = context.HttpContext.GetEndpoint();
                var allowAnonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null;

                if (allowAnonymous)
                {
                    // Skip the default challenge behavior for anonymous endpoints
                    context.HandleResponse();
                }

                return Task.CompletedTask;
            }
        };
    });

// Add BuildingBlocks Presentation layer (Controllers, Exception Handling, Permission Authorization, Swagger, CORS)
builder.Services.AddBuildingBlocksPresentation(
    builder.Configuration,
    builder.Environment,
    "MyTodos TodoService API",
    "v1",
    "Todo management service - handles todo items, lists, and task organization");

// Add Health Checks
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration.GetConnectionString("TodoServiceDb")
    ?? "Data Source=todoservice.db";

var rabbitMqHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
var rabbitMqUsername = builder.Configuration["RabbitMQ:Username"] ?? "guest";
var rabbitMqPassword = builder.Configuration["RabbitMQ:Password"] ?? "guest";

builder.Services.AddHealthChecks()
    .AddSqlite(connectionString, name: "database")
    .AddRabbitMQ(
        rabbitConnectionString: $"amqp://{rabbitMqUsername}:{rabbitMqPassword}@{rabbitMqHost}:5672",
        name: "rabbitmq");

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeederService>();
    await seeder.SeedAsync();
}

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
