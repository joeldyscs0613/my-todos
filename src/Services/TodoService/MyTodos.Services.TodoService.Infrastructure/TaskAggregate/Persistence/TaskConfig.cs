using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Configs;
using MyTodos.Services.TodoService.Domain.TaskAggregate.Constants;
using MyTodos.Services.TodoService.Infrastructure.Persistence.Constants;
using TaskEntity = MyTodos.Services.TodoService.Domain.TaskAggregate.Task;

namespace MyTodos.Services.TodoService.Infrastructure.TaskAggregate.Persistence;

public sealed class TaskConfig : MultiTenantAggregateRootConfig<TaskEntity, Guid>
{
    public override void Configure(EntityTypeBuilder<TaskEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable(TableNames.Task);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(TaskConstants.FieldLengths.TitleMaxLength);

        builder.Property(t => t.Description)
            .HasMaxLength(TaskConstants.FieldLengths.DescriptionMaxLength);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasConversion<string>();

        // Soft Delete
        builder.Property(t => t.IsDeleted)
            .HasDefaultValue(null);

        builder.HasQueryFilter(t => t.IsDeleted == null || t.IsDeleted == false);

        // Indexes
        builder.HasIndex(t => t.ProjectId);
        builder.HasIndex(t => t.AssigneeUserId);
        builder.HasIndex(t => t.Status);

        // Relationships
        builder.HasMany(t => t.Tags)
            .WithOne()
            .HasForeignKey(tag => tag.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Comments)
            .WithOne()
            .HasForeignKey(c => c.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Attachments)
            .WithOne()
            .HasForeignKey(a => a.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
