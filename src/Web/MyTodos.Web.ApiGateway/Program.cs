using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

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
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add YARP Reverse Proxy
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add Health Checks with downstream service checks
var identityServiceUrl = builder.Configuration["ReverseProxy:Clusters:identity-cluster:Destinations:destination1:Address"]
    ?? "http://localhost:5001";
var todoServiceUrl = builder.Configuration["ReverseProxy:Clusters:todos-cluster:Destinations:destination1:Address"]
    ?? "http://localhost:5011";
var notificationServiceUrl = builder.Configuration["ReverseProxy:Clusters:notifications-cluster:Destinations:destination1:Address"]
    ?? "http://localhost:5250";

builder.Services.AddHealthChecks()
    .AddUrlGroup(new Uri($"{identityServiceUrl}/health"), name: "identity-service")
    .AddUrlGroup(new Uri($"{todoServiceUrl}/health"), name: "todo-service")
    .AddUrlGroup(new Uri($"{notificationServiceUrl}/health"), name: "notification-service");

// Add Swagger (optional - for documentation)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS
app.UseCors();

// Use Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map Health Check endpoint
app.MapHealthChecks("/health");

// Map YARP Reverse Proxy
app.MapReverseProxy();

app.Run();
