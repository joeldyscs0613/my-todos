using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MyTodos.BuildingBlocks.Application.Contracts.Security;

namespace MyTodos.Services.IdentityService.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for IdentityServiceDbContext.
/// Used by EF Core tools (migrations, etc.).
/// </summary>
public sealed class IdentityServiceDbContextFactory : IDesignTimeDbContextFactory<IdentityServiceDbContext>
{
    public IdentityServiceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityServiceDbContext>();

        // Use SQLite for design-time (migrations)
        optionsBuilder.UseSqlite("Data Source=identityservice.db",
            sqliteOptions =>
            {
                sqliteOptions.MigrationsAssembly(typeof(IdentityServiceDbContext).Assembly.FullName);
            });

        // Create a design-time mock for ICurrentUserService
        var mockCurrentUserService = new DesignTimeCurrentUserService();

        return new IdentityServiceDbContext(optionsBuilder.Options, mockCurrentUserService);
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

        public List<string> GetRoles() => new();
        public List<string> GetPermissions() => new();
        public bool IsGlobalAdmin() => false;
        public bool IsTenantAdmin() => false;
    }
}
