using FluentValidation;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.TodoService.Application.Projects.Contracts;
using MyTodos.Services.TodoService.Domain.ProjectAggregate.Constants;
using MyTodos.SharedKernel;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.TodoService.Application.Projects.Commands.UpdateProjectGeneralInfo;

public sealed class UpdateProjectGeneralInfoCommand : Command
{
    public Guid ProjectId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? TargetDate { get; init; }
    public string? Color { get; init; }
    public string? Icon { get; init; }
}

public sealed class UpdateProjectGeneralInfoCommandValidator : AbstractValidator<UpdateProjectGeneralInfoCommand>
{
    public UpdateProjectGeneralInfoCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ProjectConstants.FieldLengths.NameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(ProjectConstants.FieldLengths.DescriptionMaxLength)
            .When(x => x.Description != null);

        RuleFor(x => x.Color)
            .MaximumLength(ProjectConstants.FieldLengths.ColorMaxLength)
            .When(x => x.Color != null);

        RuleFor(x => x.Icon)
            .MaximumLength(ProjectConstants.FieldLengths.IconMaxLength)
            .When(x => x.Icon != null);
    }
}

public sealed class UpdateProjectGeneralInfoCommandHandler : CommandHandler<UpdateProjectGeneralInfoCommand>
{
    private readonly IProjectWriteRepository _projectWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProjectGeneralInfoCommandHandler(
        IProjectWriteRepository projectWriteRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _projectWriteRepository = projectWriteRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result> Handle(UpdateProjectGeneralInfoCommand request, CancellationToken ct)
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

        var updateResult = project.UpdateGeneralInfo(
            request.Name,
            request.Description,
            request.StartDate,
            request.TargetDate,
            request.Color,
            request.Icon);

        if (updateResult.IsFailure)
        {
            var error = updateResult.FirstError;
            return Failure(error.Type, error.Description);
        }

        await _projectWriteRepository.UpdateAsync(project, ct);
        await _unitOfWork.CommitAsync(ct);

        return Success();
    }
}
