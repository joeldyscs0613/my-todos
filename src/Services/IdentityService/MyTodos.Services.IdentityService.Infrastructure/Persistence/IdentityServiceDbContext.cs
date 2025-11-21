using Microsoft.EntityFrameworkCore;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions;
using MyTodos.Services.IdentityService.Domain.PermissionAggregate;
using MyTodos.Services.IdentityService.Domain.RoleAggregate;
using MyTodos.Services.IdentityService.Domain.TenantAggregate;
using MyTodos.Services.IdentityService.Domain.UserAggregate;
using MyTodos.SharedKernel.Contracts;
using System.Reflection;
using MyTodos.BuildingBlocks.Infrastructure.Security;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
