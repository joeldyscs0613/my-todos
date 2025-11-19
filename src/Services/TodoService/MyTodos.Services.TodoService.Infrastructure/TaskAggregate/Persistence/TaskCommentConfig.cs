using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Configs;
using MyTodos.Services.TodoService.Domain.TaskAggregate;
using MyTodos.Services.TodoService.Domain.TaskAggregate.Constants;
using MyTodos.Services.TodoService.Infrastructure.Persistence.Constants;

namespace MyTodos.Services.TodoService.Infrastructure.TaskAggregate.Persistence;

public sealed class TaskCommentConfig : EntityWithGuidIdConfig<TaskComment>
{
    public override void Configure(EntityTypeBuilder<TaskComment> builder)
    {
        base.Configure(builder);

        builder.ToTable(TableNames.TaskComment);

        builder.Property(c => c.TaskId)
            .IsRequired();

        builder.Property(c => c.Text)
            .IsRequired()
            .HasMaxLength(TaskConstants.FieldLengths.CommentTextMaxLength);

        // Soft Delete
        builder.Property(c => c.IsDeleted)
            .HasDefaultValue(null);

        builder.HasQueryFilter(c => c.IsDeleted == null || c.IsDeleted == false);

        builder.HasIndex(c => c.TaskId);
    }
}
