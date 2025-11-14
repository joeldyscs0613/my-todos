using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Configs;

/// <summary>
/// EF Core configuration for aggregate roots with application-controlled integer identifiers.
/// Database never generates IDs - the application must provide them.
/// Use this for aggregates with externally-defined IDs or when you need control over ID assignment.
/// </summary>
public class AggregateRootWithIntIdNeverConfig<TEntity> : EntityWithIntIdNeverGeneratedConfig<TEntity>
    where TEntity : AggregateRoot<int>
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        builder.Ignore(e => e.DomainEvents);
    }
}
