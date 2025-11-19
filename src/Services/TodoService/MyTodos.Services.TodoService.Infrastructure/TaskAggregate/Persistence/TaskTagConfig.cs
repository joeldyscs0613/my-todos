using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTodos.Services.TodoService.Domain.TaskAggregate;
using MyTodos.Services.TodoService.Infrastructure.Persistence.Constants;

namespace MyTodos.Services.TodoService.Infrastructure.TaskAggregate.Persistence;

public sealed class TaskTagConfig : IEntityTypeConfiguration<TaskTag>
{
    public void Configure(EntityTypeBuilder<TaskTag> builder)
    {
        builder.ToTable(TableNames.TaskTag);

        builder.HasKey(t => new { t.TaskId, t.Name });

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(TaskTag.MaxTagNameLength);
    }
}
