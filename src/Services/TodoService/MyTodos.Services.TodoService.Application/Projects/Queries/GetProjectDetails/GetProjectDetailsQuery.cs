using FluentValidation;
using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.TodoService.Application.Projects.Contracts;
using MyTodos.Services.TodoService.Domain.ProjectAggregate.Enums;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.TodoService.Application.Projects.Queries.GetProjectDetails;

public sealed class GetProjectDetailsQuery(Guid projectId) : Query<ProjectDetailsResponseDto>
{
    public Guid ProjectId { get; init; } = projectId;
}

public sealed class GetProjectDetailsQueryValidator : AbstractValidator<GetProjectDetailsQuery>
{
    public GetProjectDetailsQueryValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
    }
}

public sealed class GetProjectDetailsQueryHandler : QueryHandler<GetProjectDetailsQuery, ProjectDetailsResponseDto>
{
    private readonly IProjectReadRepository _projectReadRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetProjectDetailsQueryHandler(
        IProjectReadRepository projectReadRepository,
        ICurrentUserService currentUserService)
    {
        _projectReadRepository = projectReadRepository;
        _currentUserService = currentUserService;
    }

    public override async Task<Result<ProjectDetailsResponseDto>> Handle(GetProjectDetailsQuery request, CancellationToken ct)
    {
        var project = await _projectReadRepository.GetByIdWithDetailsAsync(request.ProjectId, ct);

        if (project == null)
        {
            return NotFound($"Project with ID '{request.ProjectId}' not found");
        }

        // Enforce tenant isolation
        if (!_currentUserService.TenantId.HasValue)
        {
            return Unauthorized("Tenant ID is not available");
        }

        if (project.TenantId != _currentUserService.TenantId.Value)
        {
            return Forbidden("Access denied");
        }

        var dto = new ProjectDetailsResponseDto(
            project.Id,
            project.TenantId,
            project.CreatedBy,
            project.Name,
            project.Description,
            project.Status,
            project.StartDate,
            project.TargetDate,
            project.Color,
            project.Icon,
            project.CreatedDate,
            project.ModifiedDate);

        return Success(dto);
    }
}

public sealed record ProjectDetailsResponseDto(
    Guid Id,
    Guid TenantId,
    string CreatedBy,
    string Name,
    string? Description,
    ProjectStatus Status,
    DateTimeOffset? StartDate,
    DateTimeOffset? TargetDate,
    string? Color,
    string? Icon,
    DateTimeOffset CreatedDate,
    DateTimeOffset? ModifiedDate);
