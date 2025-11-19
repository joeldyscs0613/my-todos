using MyTodos.Services.TodoService.Domain.TaskAggregate.Constants;
using MyTodos.Services.TodoService.Domain.TaskAggregate.Enums;
using MyTodos.SharedKernel.Abstractions;
using MyTodos.SharedKernel.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.TodoService.Domain.TaskAggregate;

/// <summary>
/// Task aggregate root representing a single task that can be standalone or linked to a project.
/// Multi-tenant entity - all tasks belong to a specific tenant.
/// Supports soft delete functionality.
/// </summary>
public sealed class Task : MultiTenantAggregateRoot<Guid>, ISoftDeletable
{
    private readonly List<TaskTag> _tags = [];
    private readonly List<TaskComment> _comments = [];
    private readonly List<TaskAttachment> _attachments = [];

    // Identity & Ownership
    public Guid? AssigneeUserId { get; private set; }
    public Guid? ProjectId { get; private set; }

    // Details
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Enums.TaskStatus Status { get; private set; }
    public Priority Priority { get; private set; }

    // Dates
    public DateTimeOffset? StartDate { get; private set; }
    public DateTimeOffset? TargetDate { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    // Collections (read-only access)
    public IReadOnlyCollection<TaskTag> Tags => _tags.AsReadOnly();
    public IReadOnlyCollection<TaskComment> Comments => _comments.AsReadOnly();
    public IReadOnlyCollection<TaskAttachment> Attachments => _attachments.AsReadOnly();

    // Soft Delete
    public bool? IsDeleted { get; private set; }

    /// <summary>
    /// Only for deserialization. Use Create() factory method.
    /// </summary>
    [Obsolete("Only for deserialization. Use Create() factory method.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private Task()
    {
    }

    private Task(
        Guid id,
        Guid createdByUserId,
        string title,
        string? description,
        Enums.TaskStatus status,
        Priority priority,
        DateTimeOffset? startDate,
        DateTimeOffset? targetDate)
        : base(id, Guid.Empty)
    {
        Title = title;
        Description = description;
        Status = status;
        Priority = priority;
        StartDate = startDate;
        TargetDate = targetDate;
    }

    public static Result<Task> Create(
        Guid createdByUserId,
        string title,
        string? description = null,
        Priority priority = Priority.NoPriority,
        DateTimeOffset? startDate = null,
        DateTimeOffset? targetDate = null)
    {
        // Domain invariant: title is required (structural validation like max length is handled by FluentValidation)
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Task>(Error.BadRequest(TaskConstants.ErrorMessages.TitleRequired));

        var task = new Task(
            Guid.NewGuid(),
            createdByUserId,
            title.Trim(),
            description?.Trim(),
            Enums.TaskStatus.Open,
            priority,
            startDate,
            targetDate);

        return Result.Success(task);
    }

    public Result UpdateGeneralInfo(
        string title,
        string? description)
    {
        // Domain invariant: title is required (structural validation like max length is handled by FluentValidation)
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(Error.BadRequest(TaskConstants.ErrorMessages.TitleRequired));

        Title = title.Trim();
        Description = description?.Trim();

        return Result.Success();
    }

    public void ChangeStatus(Enums.TaskStatus newStatus)
    {
        Status = newStatus;

        // Business logic: track completion time
        if (newStatus == Enums.TaskStatus.Done && CompletedAt == null)
        {
            CompletedAt = DateTimeOffsetHelper.UtcNow;
        }
        else if (newStatus != Enums.TaskStatus.Done && CompletedAt != null)
        {
            CompletedAt = null;
        }
    }

    /*TODO: Add methods for
         public Priority Priority { get; private set; }

       // Dates
       public DateTimeOffset? StartDate { get; private set; }
       public DateTimeOffset? TargetDate { get; private set; }
       public DateTimeOffset? CompletedAt { get; private set; }
     */

    public Result AssignTo(Guid assigneeUserId, Guid assigneeTenantId)
    {
        if (assigneeTenantId != TenantId)
        {
            return Result.Failure(Error.BadRequest(TaskConstants.ErrorMessages.AssigneeMustBelongToOrganization));
        }

        AssigneeUserId = assigneeUserId;
        return Result.Success();
    }

    public void Unassign()
    {
        AssigneeUserId = null;
    }

    public Result LinkToProject(Guid projectId, Guid projectTenantId)
    {
        if (projectTenantId != TenantId)
        {
            return Result.Failure(Error.BadRequest(TaskConstants.ErrorMessages.ProjectMustBelongToOrganization));
        }

        ProjectId = projectId;
        return Result.Success();
    }

    public void UnlinkFromProject()
    {
        ProjectId = null;
    }

    public Result AddTag(string tagName)
    {
        var tagResult = TaskTag.Create(Id, tagName);
        if (tagResult.IsFailure)
        {
            return Result.Failure(tagResult.FirstError);
        }

        if (_tags.Any(t => t.Name == tagResult.Value!.Name))
        {
            return Result.Success();
        }

        _tags.Add(tagResult.Value!);

        return Result.Success();
    }

    public Result RemoveTag(string tagName)
    {
        var normalizedName = tagName.Trim().ToLowerInvariant();
        var tag = _tags.FirstOrDefault(t => t.Name == normalizedName);

        if (tag != null)
        {
            _tags.Remove(tag);
        }

        return Result.Success();
    }

    public Result AddComment(Guid userId, string text)
    {
        var commentResult = TaskComment.Create(Id, text, userId);
        if (commentResult.IsFailure)
        {
            return Result.Failure(commentResult.FirstError);
        }

        _comments.Add(commentResult.Value!);

        return Result.Success();
    }

    public Result AddAttachment(string fileName, string urlOrStorageKey, string contentType)
    {
        var attachmentResult = TaskAttachment.Create(Id, fileName, urlOrStorageKey, contentType);
        if (attachmentResult.IsFailure)
        {
            return Result.Failure(attachmentResult.FirstError);
        }

        _attachments.Add(attachmentResult.Value!);

        return Result.Success();
    }

    public void Delete()
    {
        IsDeleted = true;
    }

    public void Restore()
    {
        IsDeleted = null;
    }
}
