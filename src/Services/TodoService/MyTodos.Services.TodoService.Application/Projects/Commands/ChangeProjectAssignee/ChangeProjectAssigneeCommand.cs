using FluentValidation;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.TodoService.Application.Projects.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.TodoService.Application.Projects.Commands.ChangeProjectAssignee;

public sealed class ChangeProjectAssigneeCommand : Command
{
    public Guid ProjectId { get; init; }
    public Guid? AssigneeUserId { get; init; }
}

public sealed class ChangeProjectAssigneeCommandValidator : AbstractValidator<ChangeProjectAssigneeCommand>
{
    public ChangeProjectAssigneeCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
    }
}

public sealed class ChangeProjectAssigneeCommandHandler : CommandHandler<ChangeProjectAssigneeCommand>
{
    private readonly IProjectWriteRepository _projectWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeProjectAssigneeCommandHandler(
        IProjectWriteRepository projectWriteRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _projectWriteRepository = projectWriteRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result> Handle(ChangeProjectAssigneeCommand request, CancellationToken ct)
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

        if (request.AssigneeUserId.HasValue)
        {
            var tenantId = _currentUserService.TenantId.Value;
            var assignResult = project.AssignTo(request.AssigneeUserId.Value, tenantId);
            if (assignResult.IsFailure)
            {
                return assignResult;
            }
        }
        else
        {
            project.Unassign();
        }

        await _projectWriteRepository.UpdateAsync(project, ct);
        await _unitOfWork.CommitAsync(ct);

        return Success();
    }
}
