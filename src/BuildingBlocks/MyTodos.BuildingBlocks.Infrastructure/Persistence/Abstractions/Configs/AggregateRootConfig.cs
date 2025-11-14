using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Configs;

/// <summary>
/// Base EF Core configuration for aggregate roots.
/// Configures audit fields, primary key, and ignores the DomainEvents collection (not persisted).
/// Inherit from this when you need custom ID type configuration for aggregates.
/// </summary>
public class AggregateRootConfig<TEntity, TId> : EntityConfig<TEntity, TId>
    where TEntity : AggregateRoot<TId>
    where TId : IComparable
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        builder.Ignore(e => e.DomainEvents);
    }
}
