using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyTodos.Services.TodoService.Infrastructure.Persistence;

namespace MyTodos.Services.TodoService.Infrastructure.Seeding;

public sealed class DatabaseSeederService
{
    private readonly TodoServiceDbContext _context;
    private readonly ILogger<DatabaseSeederService> _logger;

    public DatabaseSeederService(TodoServiceDbContext context, ILogger<DatabaseSeederService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("TodoService database migrated successfully");

            // Add seed data here when needed
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the TodoService database");
            throw;
        }
    }
}
