using FluentValidation;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Roles.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Roles.Commands.DeleteRole;

/// <summary>
/// Command to permanently delete a role from the database.
/// This is a hard delete operation.
/// </summary>
public sealed class DeleteRoleCommand : Command
{
    public Guid RoleId { get; init; }

    public DeleteRoleCommand(Guid roleId)
    {
        RoleId = roleId;
    }
}

/// <summary>
/// Validator for DeleteRoleCommand.
/// </summary>
public sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("Role ID is required");
    }
}

/// <summary>
/// Handler for permanently deleting a role.
/// </summary>
public sealed class DeleteRoleCommandHandler : CommandHandler<DeleteRoleCommand>
{
    private readonly IRoleWriteRepository _roleWriteRepository;
    private readonly IRoleReadRepository _roleReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteRoleCommandHandler> _logger;

    public DeleteRoleCommandHandler(
        IRoleWriteRepository roleWriteRepository,
        IRoleReadRepository roleReadRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<DeleteRoleCommandHandler> logger)
    {
        _roleWriteRepository = roleWriteRepository;
        _roleReadRepository = roleReadRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override async Task<Result> Handle(
        DeleteRoleCommand request,
        CancellationToken ct)
    {
        // Authorization: Only Global.Admin can delete roles
        if (!_currentUserService.IsGlobalAdmin())
        {
            _logger.LogWarning("Role deletion failed: User is not a Global Administrator");
            return Forbidden("Only Global Administrators can delete roles");
        }

        // Check if role exists
        var role = await _roleReadRepository.GetByIdAsync(request.RoleId, ct);
        if (role is null)
        {
            _logger.LogWarning("Role deletion failed: Role with ID {RoleId} not found", request.RoleId);
            return NotFound($"Role with ID '{request.RoleId}' not found");
        }

        // Check if role is assigned to any users
        if (role.UserRoles.Count > 0)
        {
            _logger.LogWarning(
                "Role deletion failed: Role {RoleId} ({Code}) is assigned to {UserCount} user(s)",
                role.Id,
                role.Code,
                role.UserRoles.Count);

            return Conflict(
                $"Cannot delete role '{role.Code}' because it is currently assigned to {role.UserRoles.Count} user(s). " +
                "Please remove the role from all users before deleting it.");
        }

        // Perform hard delete (will cascade delete RolePermissions)
        await _roleWriteRepository.DeleteAsync(role, ct);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("Role deleted: {RoleId} with code {Code}", role.Id, role.Code);

        return Success();
    }
}
