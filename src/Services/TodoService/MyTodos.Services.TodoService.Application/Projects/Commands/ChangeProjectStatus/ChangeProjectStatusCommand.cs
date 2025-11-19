using FluentValidation;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.TodoService.Application.Projects.Contracts;
using MyTodos.Services.TodoService.Domain.ProjectAggregate.Enums;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.TodoService.Application.Projects.Commands.ChangeProjectStatus;

public sealed class ChangeProjectStatusCommand : Command
{
    public Guid ProjectId { get; init; }
    public ProjectStatus Status { get; init; }
}

public sealed class ChangeProjectStatusCommandValidator : AbstractValidator<ChangeProjectStatusCommand>
{
    public ChangeProjectStatusCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();

        // For future, we can have a workflow validation configured in the DB to know which statuses are allowed
        // from current status and validate if the provided status is allowed to be updated from
    }
}

/// <summary>
/// Changes the status with the provided one. Later we can use a configured status change workflow
/// </summary>
public sealed class ChangeProjectStatusCommandHandler : CommandHandler<ChangeProjectStatusCommand>
{
    private readonly IProjectWriteRepository _projectWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeProjectStatusCommandHandler(
        IProjectWriteRepository projectWriteRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _projectWriteRepository = projectWriteRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result> Handle(ChangeProjectStatusCommand request, CancellationToken ct)
    {
        var project = await _projectWriteRepository.GetByIdAsync(request.ProjectId, ct);
        if (project == null)
        {
            return NotFound($"Project with ID '{request.ProjectId}' not found");
        }

        if (!_currentUserService.TenantId.HasValue)
        {
            return Unauthorized("Tenant ID is not available");
        }

        if (project.TenantId != _currentUserService.TenantId.Value)
        {
            return Forbidden("Access denied to this resource");
        }

        // Domain method no longer returns Result - void method
        // Future business rules for invalid state transitions would throw DomainException
        project.ChangeStatus(request.Status);

        await _projectWriteRepository.UpdateAsync(project, ct);
        await _unitOfWork.CommitAsync(ct);

        return Success();
    }
}
