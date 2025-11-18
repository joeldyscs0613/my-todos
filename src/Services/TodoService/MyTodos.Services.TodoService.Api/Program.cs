using MyTodos.BuildingBlocks.Presentation;
using MyTodos.Services.TodoService.Application;
using MyTodos.Services.TodoService.Infrastructure;
using MyTodos.Services.TodoService.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Add Application layer services
builder.Services.AddTodoServiceApplication();

// Add Infrastructure layer services
builder.Services.AddTodoServiceInfrastructure(builder.Configuration);

// Add BuildingBlocks Presentation layer (Controllers, Exception Handling, Permission Authorization, Swagger, CORS)
builder.Services.AddBuildingBlocksPresentation(
    builder.Configuration,
    builder.Environment,
    "MyTodos TodoService API",
    "v1",
    "Todo management service - handles todo items, lists, and task organization");

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
