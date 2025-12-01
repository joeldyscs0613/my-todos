using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions;
using MyTodos.Services.NotificationService.Domain.NotificationAggregate;
using MyTodos.Services.NotificationService.Infrastructure.Persistence.Configs;

namespace MyTodos.Services.NotificationService.Infrastructure.Persistence;

public sealed class NotificationDbContext : BaseDbContext
{
    public NotificationDbContext(DbContextOptions options, ICurrentUserService currentUserService)
        : base(options, currentUserService)
    {
    }

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new NotificationConfig());
    }
}
