using MyTodos.Services.TodoService.Domain.TaskAggregate.Constants;
using MyTodos.SharedKernel.Abstractions;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.TodoService.Domain.TaskAggregate;

/// <summary>
/// Value object representing a task tag with normalized name.
/// </summary>
public sealed class TaskTag : ValueObject
{
    public const int MaxTagNameLength = 50;

    public Guid TaskId { get; private set; }
    public string Name { get; private set; }

    /// <summary>
    /// Only for deserialization. Use Create() factory method.
    /// </summary>
    [Obsolete("Only for deserialization. Use Create() factory method.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private TaskTag()
    {
    }

    private TaskTag(Guid taskId, string name)
    {
        TaskId = taskId;
        Name = name;
    }

    internal static Result<TaskTag> Create(Guid taskId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(TaskConstants.ErrorMessages.TagNameRequired);
        }

        if (name.Length > MaxTagNameLength)
        {
            return Result.BadRequest<TaskTag>(
                string.Format(TaskConstants.ErrorMessages.TagNameTooLong, MaxTagNameLength));
        }

        var normalizedName = name.Trim().ToLowerInvariant();

        return Result.Success(new TaskTag(taskId, normalizedName));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
    }
}
