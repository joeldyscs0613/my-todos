using FluentValidation;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Abstractions.Dtos;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.TodoService.Application.Projects.Contracts;
using MyTodos.Services.TodoService.Domain.ProjectAggregate;
using MyTodos.Services.TodoService.Domain.ProjectAggregate.Constants;
using MyTodos.SharedKernel;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.TodoService.Application.Projects.Commands.CreateProject;

public sealed class CreateProjectCommand : CreateCommand<Guid>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? TargetDate { get; init; }
    public string? Color { get; init; }
    public string? Icon { get; init; }
}

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ProjectConstants.ErrorMessages.NameRequired)
            .MaximumLength(ProjectConstants.FieldLengths.NameMaxLength)
            .WithMessage(string.Format(ProjectConstants.ErrorMessages.NameTooLong, 
                ProjectConstants.FieldLengths.NameMaxLength));

        RuleFor(x => x.Description)
            .MaximumLength(ProjectConstants.FieldLengths.DescriptionMaxLength)
            .WithMessage(string.Format(ProjectConstants.ErrorMessages.DescriptionTooLong, 
                ProjectConstants.FieldLengths.DescriptionMaxLength))
            .When(x => x.Description != null);

        RuleFor(x => x.Color)
            .MaximumLength(ProjectConstants.FieldLengths.ColorMaxLength)
            .WithMessage(string.Format(ProjectConstants.ErrorMessages.ColorTooLong, 
                ProjectConstants.FieldLengths.ColorMaxLength))
            .When(x => x.Color != null);

        RuleFor(x => x.Icon)
            .MaximumLength(ProjectConstants.FieldLengths.IconMaxLength)
            .WithMessage(string.Format(ProjectConstants.ErrorMessages.IconTooLong, 
                ProjectConstants.FieldLengths.IconMaxLength))
            .When(x => x.Icon != null);
    }
}

public sealed class CreateProjectCommandHandler : CreateCommandHandler<CreateProjectCommand, Guid>
{
    private readonly IProjectWriteRepository _projectWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProjectCommandHandler(
        IProjectWriteRepository projectWriteRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _projectWriteRepository = projectWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public override async Task<Result<CreateCommandResponseDto<Guid>>> Handle(
        CreateProjectCommand request,
        CancellationToken ct)
    {
        var projectResult = Project.Create(
            request.Name,
            request.Description,
            request.StartDate,
            request.TargetDate,
            request.Color,
            request.Icon);

        if (projectResult.IsFailure)
        {
            return Failure(projectResult.FirstError);
        }

        var project = projectResult.Value!;

        await _projectWriteRepository.AddAsync(project, ct);
        await _unitOfWork.CommitAsync(ct);

        return Success(project.Id);
    }
}
