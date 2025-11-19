using MyTodos.SharedKernel.Abstractions;
using MyTodos.SharedKernel.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.TodoService.Domain.TaskAggregate;

/// <summary>
/// Child entity representing file attachment metadata for a task.
/// Supports soft delete functionality.
/// </summary>
public sealed class TaskAttachment : Entity<Guid>, ISoftDeletable
{
    public Guid TaskId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string UrlOrStorageKey { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
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
    private TaskAttachment()
    {
    }

    private TaskAttachment(
        Guid id,
        Guid taskId,
        string fileName,
        string urlOrStorageKey,
        string contentType)
        : base(id)
    {
        TaskId = taskId;
        FileName = fileName;
        UrlOrStorageKey = urlOrStorageKey;
        ContentType = contentType;
    }

    internal static Result<TaskAttachment> Create(
        Guid taskId,
        string fileName,
        string urlOrStorageKey,
        string contentType)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Result.BadRequest<TaskAttachment>($"{nameof(fileName)} cannot be null or whitespace.");
        }

        var attachment = new TaskAttachment(
            Guid.NewGuid(),
            taskId,
            fileName.Trim(),
            urlOrStorageKey.Trim(),
            contentType.Trim());

        return Result.Success(attachment);
    }
}
