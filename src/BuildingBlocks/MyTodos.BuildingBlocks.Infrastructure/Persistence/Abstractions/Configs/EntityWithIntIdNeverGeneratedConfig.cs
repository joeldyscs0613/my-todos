using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Configs;

/// <summary>
/// EF Core configuration for entities with application-controlled integer identifiers.
/// Database never generates IDs - the application must provide them.
/// Use this for entities with externally-defined IDs or when you need control over ID assignment.
/// </summary>
public class EntityWithIntIdNeverGeneratedConfig<TEntity> : EntityConfig<TEntity, int>
    where TEntity : Entity<int>
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        builder
            .Property(a => a.Id)
            .ValueGeneratedNever();
    }
}
