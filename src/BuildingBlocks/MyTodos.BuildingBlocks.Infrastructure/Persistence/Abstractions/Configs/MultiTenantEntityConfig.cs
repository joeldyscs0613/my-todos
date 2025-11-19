using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Configs;

/// <summary>
/// Base EF Core configuration for multi-tenant entities (non-aggregate roots).
/// Configures the TenantId property for tenant data isolation and inherits all entity configurations.
/// All multi-tenant child entities should use this base configuration to ensure consistent tenant handling.
/// </summary>
/// <typeparam name="TEntity">The entity type that inherits from MultiTenantEntity.</typeparam>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
public class MultiTenantEntityConfig<TEntity, TId> : EntityConfig<TEntity, TId>
    where TEntity : MultiTenantEntity<TId>
    where TId : IComparable
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        // Configure TenantId as required property
        builder.Property(e => e.TenantId)
            .IsRequired()
            .HasComment("Tenant identifier for multi-tenant data isolation");

        // Create index on TenantId for performance (most queries will filter by tenant)
        builder.HasIndex(e => e.TenantId)
            .HasDatabaseName($"IX_{typeof(TEntity).Name}_TenantId");
    }
}
