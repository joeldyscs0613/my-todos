using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions;
using MyTodos.Services.IdentityService.Domain.PermissionAggregate;
using MyTodos.Services.IdentityService.Domain.RoleAggregate;
using MyTodos.Services.IdentityService.Domain.TenantAggregate;
using MyTodos.Services.IdentityService.Domain.UserAggregate;
using MyTodos.SharedKernel.Contracts;
using System.Reflection;

namespace MyTodos.Services.IdentityService.Infrastructure.Persistence;

/// <summary>
/// Database context for the Identity Service.
/// </summary>
public sealed class IdentityServiceDbContext : BaseDbContext
{
    public IdentityServiceDbContext(DbContextOptions options, ICurrentUserService currentUserService)
        : base(options, currentUserService)
    {
    }

    // DbSets for aggregate roots
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Permission> Permissions => Set<Permission>();

    // DbSets for other entities
    public DbSet<UserInvitation> UserInvitations => Set<UserInvitation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply global query filters for tenant-scoped entities
        ApplyTenantQueryFilters(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private static void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        // Apply query filter to all entities implementing ITenantEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IMultiTenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                // Note: Actual tenant filtering will be implemented via interceptor
                // that reads current tenant from ICurrentUserService
                // This is just a placeholder for the pattern
            }
        }
    }
}
