using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTodos.SharedKernel.Abstractions;

namespace MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Configs;

/// <summary>
/// Base EF Core configuration for all entities.
/// Configures audit fields (CreatedDate, CreatedBy, ModifiedDate, ModifiedBy) and primary key.
/// Inherit from this when you need custom ID type configuration.
/// </summary>
public class EntityConfig<TEntity, TId> : IEntityTypeConfiguration<TEntity>
    where TEntity : Entity<TId>
    where TId : IComparable
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(e => e.CreatedDate)
            .IsRequired();

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(256);

        builder.Property(e => e.ModifiedBy)
            .HasMaxLength(256);

        builder.Property(e => e.ModifiedDate);

        builder.HasKey(e => e.Id);
    }
}
