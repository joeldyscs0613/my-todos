using FluentValidation;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Permissions.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Permissions.Commands.UpdatePermissionGeneralInfo;

/// <summary>
/// Command to update permission general information (name and description).
/// Code is immutable and cannot be updated.
/// </summary>
public sealed class UpdatePermissionGeneralInfoCommand : Command
{
    public Guid PermissionId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }

    public UpdatePermissionGeneralInfoCommand(Guid permissionId, string name, string? description)
    {
        PermissionId = permissionId;
        Name = name;
        Description = description;
    }
}

/// <summary>
/// Validator for UpdatePermissionGeneralInfoCommand.
/// </summary>
public sealed class UpdatePermissionGeneralInfoCommandValidator : AbstractValidator<UpdatePermissionGeneralInfoCommand>
{
    public UpdatePermissionGeneralInfoCommandValidator()
    {
        RuleFor(x => x.PermissionId)
            .NotEmpty()
            .WithMessage("Permission ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Permission name is required")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}

/// <summary>
/// Handler for updating permission general information.
/// </summary>
public sealed class UpdatePermissionGeneralInfoCommandHandler : CommandHandler<UpdatePermissionGeneralInfoCommand>
{
    private readonly IPermissionReadRepository _permissionReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdatePermissionGeneralInfoCommandHandler> _logger;

    public UpdatePermissionGeneralInfoCommandHandler(
        IPermissionReadRepository permissionReadRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<UpdatePermissionGeneralInfoCommandHandler> logger)
    {
        _permissionReadRepository = permissionReadRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override async Task<Result> Handle(
        UpdatePermissionGeneralInfoCommand request,
        CancellationToken ct)
    {
        // Authorization: Only Global.Admin can update permissions
        if (!_currentUserService.IsGlobalAdmin())
        {
            _logger.LogWarning("Permission update failed: User is not a Global Administrator");
            return Forbidden("Only Global Administrators can update permissions");
        }

        // Get the permission
        var permission = await _permissionReadRepository.GetByIdAsync(request.PermissionId, ct);
        if (permission is null)
        {
            _logger.LogWarning("Permission update failed: Permission with ID {PermissionId} not found", request.PermissionId);
            return NotFound($"Permission with ID '{request.PermissionId}' not found");
        }

        // Update permission using domain method
        var updateResult = permission.Update(request.Name, request.Description);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        // Persist changes
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("Permission general info updated: {PermissionId}", permission.Id);

        return Success();
    }
}
