using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Configs;
using MyTodos.Services.IdentityService.Domain.UserAggregate;
using MyTodos.Services.IdentityService.Domain.UserAggregate.Constants;
using MyTodos.Services.IdentityService.Infrastructure.Persistence.Constants;

namespace MyTodos.Services.IdentityService.Infrastructure.Persistence.Users.Configs;

/// <summary>
/// Entity configuration for User aggregate root.
/// </summary>
public sealed class UserConfig : AggregateRootWithGuidIdConfig<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(TableNames.User);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(UserConstants.FieldLengths.UsernameMaxLength);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(UserConstants.FieldLengths.EmailMaxLength);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(UserConstants.FieldLengths.PasswordHashMaxLength);

        builder.Property(u => u.FirstName)
            .HasMaxLength(UserConstants.FieldLengths.FirstNameMaxLength);

        builder.Property(u => u.LastName)
            .HasMaxLength(UserConstants.FieldLengths.LastNameMaxLength);

        builder.Property(u => u.IsActive)
            .IsRequired();

        builder.Property(u => u.LastLoginAt);

        // Indexes
        builder.HasIndex(u => u.Username)
            .IsUnique();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        // User owns UserRoles collection
        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
