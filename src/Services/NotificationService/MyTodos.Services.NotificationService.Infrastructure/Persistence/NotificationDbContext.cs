using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions;

namespace MyTodos.Services.NotificationService.Infrastructure.Persistence;

public sealed class NotificationDbContext : BaseDbContext
{
    public NotificationDbContext(DbContextOptions options, ICurrentUserService currentUserService)
        : base(options, currentUserService)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // No entities configured yet for NotificationService
        // This is a minimal stub to allow the service to compile
    }
}
