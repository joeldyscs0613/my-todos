using FluentValidation;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Permissions.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Permissions.Commands.DeletePermission;

/// <summary>
/// Command to permanently delete a permission from the database.
/// This is a hard delete operation.
/// </summary>
public sealed class DeletePermissionCommand : Command
{
    public Guid PermissionId { get; init; }

    public DeletePermissionCommand(Guid permissionId)
    {
        PermissionId = permissionId;
    }
}

/// <summary>
/// Validator for DeletePermissionCommand.
/// </summary>
public sealed class DeletePermissionCommandValidator : AbstractValidator<DeletePermissionCommand>
{
    public DeletePermissionCommandValidator()
    {
        RuleFor(x => x.PermissionId)
            .NotEmpty()
            .WithMessage("Permission ID is required");
    }
}

/// <summary>
/// Handler for permanently deleting a permission.
/// </summary>
public sealed class DeletePermissionCommandHandler : CommandHandler<DeletePermissionCommand>
{
    private readonly IPermissionWriteRepository _permissionWriteRepository;
    private readonly IPermissionReadRepository _permissionReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeletePermissionCommandHandler> _logger;

    public DeletePermissionCommandHandler(
        IPermissionWriteRepository permissionWriteRepository,
        IPermissionReadRepository permissionReadRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<DeletePermissionCommandHandler> logger)
    {
        _permissionWriteRepository = permissionWriteRepository;
        _permissionReadRepository = permissionReadRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override async Task<Result> Handle(
        DeletePermissionCommand request,
        CancellationToken ct)
    {
        // Authorization: Only Global.Admin can delete permissions
        if (!_currentUserService.IsGlobalAdmin())
        {
            _logger.LogWarning("Permission deletion failed: User is not a Global Administrator");
            return Forbidden("Only Global Administrators can delete permissions");
        }

        // Check if permission exists
        var permission = await _permissionReadRepository.GetByIdAsync(request.PermissionId, ct);
        if (permission is null)
        {
            _logger.LogWarning("Permission deletion failed: Permission with ID {PermissionId} not found", request.PermissionId);
            return NotFound($"Permission with ID '{request.PermissionId}' not found");
        }

        // Check if permission is assigned to any roles
        if (permission.RolePermissions.Any())
        {
            _logger.LogWarning(
                "Permission deletion failed: Permission {PermissionId} ({Code}) is assigned to {RoleCount} role(s)",
                permission.Id,
                permission.Code,
                permission.RolePermissions.Count);

            return Conflict(
                $"Cannot delete permission '{permission.Code}' because it is currently assigned to {permission.RolePermissions.Count} role(s). " +
                "Please remove the permission from all roles before deleting it.");
        }

        // Perform hard delete
        await _permissionWriteRepository.DeleteAsync(permission, ct);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("Permission deleted: {PermissionId} with code {Code}", permission.Id, permission.Code);

        return Success();
    }
}
