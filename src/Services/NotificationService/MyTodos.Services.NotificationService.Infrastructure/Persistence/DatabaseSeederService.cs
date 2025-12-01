using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MyTodos.Services.NotificationService.Infrastructure.Persistence;

public sealed class DatabaseSeederService
{
    private readonly NotificationDbContext _context;
    private readonly ILogger<DatabaseSeederService> _logger;

    public DatabaseSeederService(
        NotificationDbContext context,
        ILogger<DatabaseSeederService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        try
        {
            // Apply pending migrations
            _logger.LogInformation("Applying NotificationService database migrations...");
            await _context.Database.MigrateAsync(ct);
            _logger.LogInformation("NotificationService database migrations applied successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply NotificationService database migrations");
            throw;
        }
    }
}
