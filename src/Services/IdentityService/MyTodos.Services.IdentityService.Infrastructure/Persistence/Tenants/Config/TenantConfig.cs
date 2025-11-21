using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Configs;
using MyTodos.Services.IdentityService.Domain.TenantAggregate;
using MyTodos.Services.IdentityService.Domain.TenantAggregate.Constants;
using MyTodos.Services.IdentityService.Infrastructure.Persistence.Constants;

namespace MyTodos.Services.IdentityService.Infrastructure.Persistence.Tenants.Config;

/// <summary>
/// Entity configuration for Tenant aggregate root.
/// </summary>
public sealed class TenantConfig : AggregateRootWithGuidIdConfig<Tenant>
{
    public override void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable(TableNames.Tenant);

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(TenantConstants.FieldLengths.NameMaxLength);

        builder.Property(t => t.IsActive)
            .IsRequired();

        // Indexes
        builder.HasIndex(t => t.Name)
            .IsUnique();
    }
}
