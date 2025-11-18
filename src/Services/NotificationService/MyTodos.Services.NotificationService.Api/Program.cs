using MyTodos.BuildingBlocks.Presentation;
using MyTodos.Services.NotificationService.Application;
using MyTodos.Services.NotificationService.Infrastructure;
using MyTodos.Services.NotificationService.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Add Application layer services
builder.Services.AddNotificationServiceApplication();

// Add Infrastructure layer services
builder.Services.AddNotificationServiceInfrastructure(builder.Configuration);

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
app.UseAuthorization();
app.MapControllers();

app.Run();
