using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

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

        return new IdentityServiceDbContext(optionsBuilder.Options);
    }
}
