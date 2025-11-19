using MyTodos.Services.TodoService.Domain.TaskAggregate.Constants;
using MyTodos.SharedKernel.Abstractions;
using MyTodos.SharedKernel.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.TodoService.Domain.TaskAggregate;

/// <summary>
/// Child entity representing a comment on a task.
/// Supports soft delete functionality.
/// </summary>
public sealed class TaskComment : Entity<Guid>, ISoftDeletable
{
    public Guid TaskId { get; private set; }
    public string Text { get; private set; }
    public bool? IsDeleted { get; private set; }

    public void Delete()
    {
        IsDeleted = true;
    }

    public void Restore()
    {
        IsDeleted = false;
    }

    /// <summary>
    /// Only for deserialization. Use Create() factory method.
    /// </summary>
    [Obsolete("Only for deserialization. Use Create() factory method.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private TaskComment()
    {
    }

    private TaskComment(Guid id, Guid taskId, string text)
        : base(id)
    {
        TaskId = taskId;
        Text = text;
    }

    internal static Result<TaskComment> Create(Guid taskId, string text, Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.BadRequest<TaskComment>(TaskConstants.ErrorMessages.CommentTextRequired);
        }

        var comment = new TaskComment(
            Guid.NewGuid(),
            taskId,
            text.Trim());

        return Result.Success(comment);
    }
}
