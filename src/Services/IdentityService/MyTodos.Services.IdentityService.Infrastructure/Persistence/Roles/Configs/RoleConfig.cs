using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Configs;
using MyTodos.Services.IdentityService.Domain.RoleAggregate;
using MyTodos.Services.IdentityService.Domain.RoleAggregate.Constants;
using MyTodos.Services.IdentityService.Infrastructure.Persistence.Constants;

namespace MyTodos.Services.IdentityService.Infrastructure.Persistence.Roles.Configs;

/// <summary>
/// Entity configuration for Role aggregate root.
/// </summary>
public sealed class RoleConfig : AggregateRootWithGuidIdConfig<Role>
{
    public override void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable(TableNames.Role);

        builder.Property(r => r.Code)
            .IsRequired()
            .HasMaxLength(RoleConstants.FieldLengths.CodeMaxLength);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(RoleConstants.FieldLengths.NameMaxLength);

        builder.Property(r => r.Description)
            .HasMaxLength(RoleConstants.FieldLengths.DescriptionMaxLength);

        builder.Property(r => r.Scope)
            .IsRequired()
            .HasConversion<int>();

        // Indexes
        builder.HasIndex(r => r.Code)
            .IsUnique();

        // Role owns RolePermissions collection
        builder.HasMany(r => r.RolePermissions)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
