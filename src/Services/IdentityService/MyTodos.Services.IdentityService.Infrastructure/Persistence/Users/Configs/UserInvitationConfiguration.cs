using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTodos.Services.IdentityService.Domain.UserAggregate;
using MyTodos.Services.IdentityService.Domain.UserAggregate.Constants;
using MyTodos.Services.IdentityService.Infrastructure.Persistence.Constants;

namespace MyTodos.Services.IdentityService.Infrastructure.UserAggregate.Persistence;

/// <summary>
/// Entity configuration for UserInvitation.
/// </summary>
public sealed class UserInvitationConfiguration : IEntityTypeConfiguration<UserInvitation>
{
    public void Configure(EntityTypeBuilder<UserInvitation> builder)
    {
        builder.ToTable(TableNames.UserInvitation);

        builder.HasKey(ui => ui.Id);

        builder.Property(ui => ui.Email)
            .IsRequired()
            .HasMaxLength(UserInvitationConstants.FieldLengths.EmailMaxLength);

        builder.Property(ui => ui.InvitationToken)
            .IsRequired()
            .HasMaxLength(UserInvitationConstants.FieldLengths.InvitationTokenMaxLength);

        builder.Property(ui => ui.InvitedByUserId)
            .IsRequired();

        builder.Property(ui => ui.TenantId);

        builder.Property(ui => ui.RoleId)
            .IsRequired();

        builder.Property(ui => ui.ExpiresAt)
            .IsRequired();

        builder.Property(ui => ui.AcceptedAt);

        builder.Property(ui => ui.Status)
            .IsRequired()
            .HasConversion<int>();

        // Relationships
        builder.HasOne(ui => ui.InvitedByUser)
            .WithMany()
            .HasForeignKey(ui => ui.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ui => ui.Tenant)
            .WithMany()
            .HasForeignKey(ui => ui.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ui => ui.Role)
            .WithMany()
            .HasForeignKey(ui => ui.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ui => ui.InvitationToken)
            .IsUnique();

        builder.HasIndex(ui => ui.Email);

        builder.HasIndex(ui => new { ui.Email, ui.Status });
    }
}
