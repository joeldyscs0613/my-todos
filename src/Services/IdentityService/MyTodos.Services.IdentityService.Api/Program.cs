using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
});

// Add BuildingBlocks Presentation layer (Controllers, Exception Handling, Permission Authorization, Swagger, CORS)
builder.Services.AddBuildingBlocksPresentation(
    builder.Configuration,
    builder.Environment,
    "MyTodos IdentityService API",
    "v1",
    "Identity and Access Management service - handles users, roles, permissions, and authentication");

var app = builder.Build();

// Configure the HTTP request pipeline
// BuildingBlocks middleware (exception handling, Swagger, CORS)
app.UseBuildingBlocksPresentation(app.Environment);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
