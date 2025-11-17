using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Configs;
using MyTodos.Services.IdentityService.Domain.PermissionAggregate;
using MyTodos.Services.IdentityService.Domain.PermissionAggregate.Constants;
using MyTodos.Services.IdentityService.Infrastructure.Persistence.Constants;

namespace MyTodos.Services.IdentityService.Infrastructure.Persistence.Permissions.Configurations;

/// <summary>
/// Entity configuration for Permission aggregate root.
/// </summary>
public sealed class PermissionConfiguration : AggregateRootWithGuidIdConfig<Permission>
{
    public override void Configure(EntityTypeBuilder<Permission> builder)
    {
        base.Configure(builder);
        
        builder.ToTable(TableNames.Permission);
        
        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(PermissionConstants.FieldLengths.CodeMaxLength);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(PermissionConstants.FieldLengths.NameMaxLength);

        builder.Property(p => p.Description)
            .HasMaxLength(PermissionConstants.FieldLengths.DescriptionMaxLength);

        // Indexes
        builder.HasIndex(p => p.Code)
            .IsUnique();
    }
}
