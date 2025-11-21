using FluentValidation;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Abstractions.Dtos;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Roles.Contracts;
using MyTodos.Services.IdentityService.Domain.RoleAggregate;
using MyTodos.Services.IdentityService.Domain.RoleAggregate.Enums;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Roles.Commands.CreateRole;

/// <summary>
/// Command to create a new role.
/// </summary>
public sealed class CreateRoleCommand : CreateCommand<Guid>
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public AccessScope Scope { get; init; }
}

/// <summary>
/// Validator for CreateRoleCommand.
/// </summary>
public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Role code is required")
            .MaximumLength(100);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Role name is required")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Scope)
            .IsInEnum()
            .WithMessage("Invalid access scope");
    }
}

/// <summary>
/// Handler for creating a new role.
/// </summary>
public sealed class CreateRoleCommandHandler : CreateCommandHandler<CreateRoleCommand, Guid>
{
    private readonly IRoleWriteRepository _roleWriteRepository;
    private readonly IRoleReadRepository _roleReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateRoleCommandHandler> _logger;

    public CreateRoleCommandHandler(
        IRoleWriteRepository roleWriteRepository,
        IRoleReadRepository roleReadRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CreateRoleCommandHandler> logger)
    {
        _roleWriteRepository = roleWriteRepository;
        _roleReadRepository = roleReadRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override async Task<Result<CreateCommandResponseDto<Guid>>> Handle(
        CreateRoleCommand request,
        CancellationToken ct)
    {
        // Authorization: Only Global.Admin can create roles
        if (!_currentUserService.IsGlobalAdmin())
        {
            _logger.LogWarning("Role creation failed: User is not a Global Administrator");
            return Forbidden("Only Global Administrators can create roles");
        }

        // Check if role code already exists
        var existingRole = await _roleReadRepository.GetByCodeAsync(request.Code, ct);
        if (existingRole != null)
        {
            _logger.LogWarning("Role creation failed: Role with code '{Code}' already exists", request.Code);
            return Conflict($"Role with code '{request.Code}' already exists");
        }

        // Create role using domain factory method
        var roleResult = Role.Create(request.Code, request.Name, request.Scope, request.Description);
        if (roleResult.IsFailure)
        {
            _logger.LogWarning("Role creation failed: {Error}", roleResult.FirstError.Description);
            return Failure(roleResult.FirstError);
        }

        var role = roleResult.Value!;

        // Persist the role
        await _roleWriteRepository.AddAsync(role, ct);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("Role created: {RoleId} with code {Code}", role.Id, role.Code);

        return Success(role.Id);
    }
}
