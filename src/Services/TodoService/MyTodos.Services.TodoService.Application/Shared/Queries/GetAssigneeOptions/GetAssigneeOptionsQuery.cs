using FluentValidation;
using MyTodos.BuildingBlocks.Application.Abstractions.Queries;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.TodoService.Application.Shared.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.TodoService.Application.Shared.Queries.GetAssigneeOptions;

public sealed class GetAssigneeOptionsQuery : Query<AssigneeOptionsDto>
{
}

public sealed class GetAssigneeOptionsQueryValidator : AbstractValidator<GetAssigneeOptionsQuery>
{
    public GetAssigneeOptionsQueryValidator()
    {
        // No validation needed - query uses current user's tenant from JWT
    }
}

public sealed class GetAssigneeOptionsQueryHandler : QueryHandler<GetAssigneeOptionsQuery, AssigneeOptionsDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAssigneeOptionsRepository _assigneeOptionsRepository;

    public GetAssigneeOptionsQueryHandler(
        ICurrentUserService currentUserService,
        IAssigneeOptionsRepository assigneeOptionsRepository)
    {
        _currentUserService = currentUserService;
        _assigneeOptionsRepository = assigneeOptionsRepository;
    }

    public override async Task<Result<AssigneeOptionsDto>> Handle(GetAssigneeOptionsQuery request, CancellationToken ct)
    {
        if (!_currentUserService.TenantId.HasValue)
        {
            return Unauthorized("Tenant ID is not available");
        }

        var users = await _assigneeOptionsRepository.GetUsersByTenantIdAsync(
            _currentUserService.TenantId.Value,
            ct);

        var userOptions = users
            .Select(u => new UserOptionDto(u.Id, u.FullName))
            .OrderBy(u => u.Name)
            .ToList();

        return Success(new AssigneeOptionsDto(userOptions));
    }
}

/// <summary>
/// Response DTO for assignee options
/// </summary>
public sealed record AssigneeOptionsDto(List<UserOptionDto> Users);

/// <summary>
/// Represents a user option for assignment
/// </summary>
public sealed record UserOptionDto(Guid Id, string Name);
