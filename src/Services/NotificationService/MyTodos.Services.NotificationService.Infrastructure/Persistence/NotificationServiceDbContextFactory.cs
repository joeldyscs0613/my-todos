using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MyTodos.BuildingBlocks.Application.Contracts.Security;

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

        // Create a design-time mock for ICurrentUserService
        var mockCurrentUserService = new DesignTimeCurrentUserService();

        return new NotificationServiceDbContext(optionsBuilder.Options, mockCurrentUserService);
    }

    /// <summary>
    /// Design-time mock implementation of ICurrentUserService.
    /// Returns null for all properties as no user context exists during migrations.
    /// </summary>
    private sealed class DesignTimeCurrentUserService : ICurrentUserService
    {
        public Guid? UserId => null;
        public string? Username => null;
        public Guid? TenantId => null;
        public bool IsAuthenticated => false;
    }
}
