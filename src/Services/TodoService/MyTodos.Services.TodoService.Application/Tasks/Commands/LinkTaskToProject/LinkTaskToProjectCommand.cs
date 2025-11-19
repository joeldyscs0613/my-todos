using FluentValidation;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.TodoService.Application.Projects.Contracts;
using MyTodos.Services.TodoService.Application.Tasks.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.TodoService.Application.Tasks.Commands.LinkTaskToProject;

public sealed class LinkTaskToProjectCommand : Command
{
    public Guid TaskId { get; init; }
    public Guid? ProjectId { get; init; }
}

public sealed class LinkTaskToProjectCommandValidator : AbstractValidator<LinkTaskToProjectCommand>
{
    public LinkTaskToProjectCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
    }
}

public sealed class LinkTaskToProjectCommandHandler : CommandHandler<LinkTaskToProjectCommand>
{
    private readonly ITaskWriteRepository _taskWriteRepository;
    private readonly IProjectReadRepository _projectReadRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public LinkTaskToProjectCommandHandler(
        ITaskWriteRepository taskWriteRepository,
        IProjectReadRepository projectReadRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _taskWriteRepository = taskWriteRepository;
        _projectReadRepository = projectReadRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result> Handle(LinkTaskToProjectCommand request, CancellationToken ct)
    {
        var task = await _taskWriteRepository.GetByIdAsync(request.TaskId, ct);
        if (task == null)
        {
            return NotFound($"Task with ID '{request.TaskId}' not found");
        }

        if (!_currentUserService.TenantId.HasValue)
        {
            return Unauthorized("Tenant ID is not available");
        }

        if (task.TenantId != _currentUserService.TenantId.Value)
        {
            return Forbidden("Access denied to this resource");
        }

        if (request.ProjectId.HasValue)
        {
            var project = await _projectReadRepository.GetByIdAsync(request.ProjectId.Value, ct);
            if (project == null)
            {
                return NotFound($"Project with ID '{request.ProjectId}' not found");
            }

            if (project.TenantId != _currentUserService.TenantId.Value)
            {
                return Forbidden("Access denied to this resource");
            }

            var linkResult = task.LinkToProject(project.Id, project.TenantId);
            if (linkResult.IsFailure)
            {
                return linkResult;
            }
        }
        else
        {
            // Domain method is now void - structural validation handled by FluentValidation
            task.UnlinkFromProject();
        }

        await _taskWriteRepository.UpdateAsync(task, ct);
        await _unitOfWork.CommitAsync(ct);

        return Success();
    }
}
