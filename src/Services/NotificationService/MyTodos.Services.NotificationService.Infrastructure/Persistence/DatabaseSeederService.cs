using Microsoft.Extensions.Logging;

namespace MyTodos.Services.NotificationService.Infrastructure.Persistence;

public sealed class DatabaseSeederService
{
    private readonly ILogger<DatabaseSeederService> _logger;

    public DatabaseSeederService(ILogger<DatabaseSeederService> logger)
    {
        _logger = logger;
    }

    public Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // No database or seeding needed yet for NotificationService
        _logger.LogInformation("NotificationService database seeding skipped (no database configured yet)");
        return Task.CompletedTask;
    }
}
