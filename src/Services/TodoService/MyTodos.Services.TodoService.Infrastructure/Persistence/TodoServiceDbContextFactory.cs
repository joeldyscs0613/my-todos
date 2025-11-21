using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MyTodos.BuildingBlocks.Application.Contracts.Security;

namespace MyTodos.Services.TodoService.Infrastructure.Persistence;

/// <summary>
/// Design-time DbContext factory for EF Core migrations.
/// </summary>
public sealed class TodoServiceDbContextFactory : IDesignTimeDbContextFactory<TodoServiceDbContext>
{
    public TodoServiceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TodoServiceDbContext>();
        optionsBuilder.UseSqlite("Data Source=todoservice.db");

        // Use a mock ICurrentUserService for design-time migrations
        var currentUserService = new DesignTimeCurrentUserService();

        return new TodoServiceDbContext(optionsBuilder.Options, currentUserService);
    }

    /// <summary>
    /// Design-time implementation of ICurrentUserService for EF Core migrations.
    /// </summary>
    private sealed class DesignTimeCurrentUserService : ICurrentUserService
    {
        public Guid? UserId => null;
        public Guid? TenantId => null;
        public string? Username => null;
        public bool IsAuthenticated => false;
        public List<string> GetRoles()
        {
            return new List<string>();
        }

        public List<string> GetPermissions()
        {
            return new List<string>();
        }

        public bool IsGlobalAdmin() => false;

        public bool IsTenantAdmin() => false;
    }
}
