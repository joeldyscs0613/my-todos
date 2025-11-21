using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTodos.Services.IdentityService.Domain.UserAggregate;
using MyTodos.Services.IdentityService.Infrastructure.Persistence.Constants;

namespace MyTodos.Services.IdentityService.Infrastructure.Persistence.Users.Configs;

/// <summary>
/// Entity configuration for UserRole (part of User aggregate).
/// </summary>
public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable(TableNames.UserRole);

        builder.HasKey(ur => ur.Id);

        builder.Property(ur => ur.UserId)
            .IsRequired();

        builder.Property(ur => ur.RoleId)
            .IsRequired();

        builder.Property(ur => ur.TenantId);

        // Relationship to User (cascade delete within aggregate)
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship to Role (restrict - cross-aggregate)
        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship to Tenant (restrict - cross-aggregate)
        builder.HasOne(ur => ur.Tenant)
            .WithMany(t => t.UserRoles)
            .HasForeignKey(ur => ur.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId, ur.TenantId })
            .IsUnique();
    }
}
