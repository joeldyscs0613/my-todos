using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Configs;
using MyTodos.Services.TodoService.Domain.ProjectAggregate;
using MyTodos.Services.TodoService.Domain.ProjectAggregate.Constants;
using MyTodos.Services.TodoService.Infrastructure.Persistence.Constants;

namespace MyTodos.Services.TodoService.Infrastructure.ProjectAggregate.Persistence;

public sealed class ProjectConfig : MultiTenantAggregateRootConfig<Project, Guid>
{
    public override void Configure(EntityTypeBuilder<Project> builder)
    {
        base.Configure(builder);

        builder.ToTable(TableNames.Project);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(ProjectConstants.FieldLengths.NameMaxLength);

        builder.Property(p => p.Description)
            .HasMaxLength(ProjectConstants.FieldLengths.DescriptionMaxLength);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.Color)
            .HasMaxLength(ProjectConstants.FieldLengths.ColorMaxLength);

        builder.Property(p => p.Icon)
            .HasMaxLength(ProjectConstants.FieldLengths.IconMaxLength);

        // Soft Delete
        builder.Property(p => p.IsDeleted)
            .HasDefaultValue(null);

        builder.HasQueryFilter(p => p.IsDeleted == null || p.IsDeleted == false);

        // Indexes
        builder.HasIndex(p => p.Status);
    }
}
