using MyTodos.Services.TodoService.Domain.ProjectAggregate.Constants;
using MyTodos.Services.TodoService.Domain.ProjectAggregate.Enums;
using MyTodos.SharedKernel;
using MyTodos.SharedKernel.Abstractions;
using MyTodos.SharedKernel.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.TodoService.Domain.ProjectAggregate;

/// <summary>
/// Project aggregate root representing a grouping of tasks.
/// Multi-tenant entity - all projects belong to a specific tenant.
/// Supports soft delete functionality.
/// </summary>
public sealed class Project : MultiTenantAggregateRoot<Guid>, ISoftDeletable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProjectStatus Status { get; private set; }
    public Guid? AssigneeUserId { get; private set; }
    public DateTimeOffset? StartDate { get; private set; }
    public DateTimeOffset? TargetDate { get; private set; }
    public string? Color { get; private set; }
    public string? Icon { get; private set; }

    // Soft Delete
    public bool? IsDeleted { get; private set; }

    /// <summary>
    /// Only for deserialization. Use Create() factory method.
    /// </summary>
    [Obsolete("Only for deserialization. Use Create() factory method.", error: true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    private Project()
    {
    }

    private Project(
        Guid id,
        string name,
        string? description,
        DateTimeOffset? startDate,
        DateTimeOffset? targetDate,
        string? color,
        string? icon)
        : base(id, Guid.Empty)
    {
        Name = name;
        Description = description;
        Status = ProjectStatus.Active;
        StartDate = startDate;
        TargetDate = targetDate;
        Color = color;
        Icon = icon;
    }

    public static Result<Project> Create(
        string name,
        string? description = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? targetDate = null,
        string? color = null,
        string? icon = null)
    {
        // Domain invariant: name is required (structural validation like max length is handled by FluentValidation)
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Project>(Error.BadRequest(ProjectConstants.ErrorMessages.NameRequired));

        var project = new Project(
            Guid.NewGuid(),
            name.Trim(),
            description?.Trim(),
            startDate,
            targetDate,
            color?.Trim(),
            icon?.Trim());

        return Result.Success(project);
    }

    public Result UpdateGeneralInfo(
        string name,
        string? description,
        DateTimeOffset? startDate,
        DateTimeOffset? targetDate,
        string? color,
        string? icon)
    {
        // Domain invariant: name is required (structural validation like max length is handled by FluentValidation)
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.BadRequest(ProjectConstants.ErrorMessages.NameRequired));

        Name = name.Trim();
        Description = description?.Trim();
        StartDate = startDate;
        TargetDate = targetDate;
        Color = color?.Trim();
        Icon = icon?.Trim();

        return Result.Success();
    }

    public void ChangeStatus(ProjectStatus newStatus)
    {
        // For now, allow any status transition
        // Future: Add business rules for invalid state transitions (e.g., Archived -> Active might be disallowed)
        Status = newStatus;
    }

    public Result AssignTo(Guid assigneeUserId, Guid assigneeTenantId)
    {
        // Business rule: Assignee must belong to the same tenant as the project
        if (TenantId != assigneeTenantId)
        {
            return Result.Failure(Error.BadRequest(ProjectConstants.ErrorMessages.AssigneeMustBelongToOrganization));
        }

        AssigneeUserId = assigneeUserId;

        return Result.Success();
    }

    public void Unassign()
    {
        AssigneeUserId = null;
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
