using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyTodos.Services.NotificationService.Infrastructure.Persistence;

namespace MyTodos.Services.NotificationService.Infrastructure.Seeding;

public sealed class DatabaseSeederService
{
    private readonly NotificationServiceDbContext _context;
    private readonly ILogger<DatabaseSeederService> _logger;

    public DatabaseSeederService(NotificationServiceDbContext context, ILogger<DatabaseSeederService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("NotificationService database migrated successfully");

            // Add seed data here when needed
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the NotificationService database");
            throw;
        }
    }
}
