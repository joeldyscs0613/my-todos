using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions;

namespace MyTodos.Services.NotificationService.Infrastructure.Persistence;

public sealed class NotificationServiceDbContext : BaseDbContext
{
    public NotificationServiceDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationServiceDbContext).Assembly);
    }
}
