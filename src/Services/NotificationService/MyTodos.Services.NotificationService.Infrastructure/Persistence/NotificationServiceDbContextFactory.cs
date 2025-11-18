using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MyTodos.Services.NotificationService.Infrastructure.Persistence;

/// <summary>
/// Design-time DbContext factory for EF Core migrations.
/// </summary>
public sealed class NotificationServiceDbContextFactory : IDesignTimeDbContextFactory<NotificationServiceDbContext>
{
    public NotificationServiceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NotificationServiceDbContext>();
        optionsBuilder.UseSqlite("Data Source=notificationservice.db");

        return new NotificationServiceDbContext(optionsBuilder.Options);
    }
}
