using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTodos.BuildingBlocks.Infrastructure.Persistence.Abstractions.Configs;
using MyTodos.Services.TodoService.Domain.TaskAggregate;
using MyTodos.Services.TodoService.Domain.TaskAggregate.Constants;
using MyTodos.Services.TodoService.Infrastructure.Persistence.Constants;

namespace MyTodos.Services.TodoService.Infrastructure.Persistence.Tasks.Configs;

public sealed class TaskAttachmentConfig : EntityWithGuidIdConfig<TaskAttachment>
{
    public override void Configure(EntityTypeBuilder<TaskAttachment> builder)
    {
        base.Configure(builder);

        builder.ToTable(TableNames.TaskAttachment);

        builder.Property(a => a.TaskId)
            .IsRequired();

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(TaskConstants.FieldLengths.AttachmentFileNameMaxLength);

        builder.Property(a => a.UrlOrStorageKey)
            .IsRequired()
            .HasMaxLength(TaskConstants.FieldLengths.AttachmentUrlOrStorageKeyMaxLength);

        builder.Property(a => a.ContentType)
            .IsRequired()
            .HasMaxLength(TaskConstants.FieldLengths.AttachmentContentTypeMaxLength);

        // Soft Delete
        builder.Property(a => a.IsDeleted)
            .HasDefaultValue(null);

        builder.HasQueryFilter(a => a.IsDeleted == null || a.IsDeleted == false);

        builder.HasIndex(a => a.TaskId);
    }
}
