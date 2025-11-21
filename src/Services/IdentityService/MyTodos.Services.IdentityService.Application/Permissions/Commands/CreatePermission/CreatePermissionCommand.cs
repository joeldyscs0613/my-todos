using FluentValidation;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Abstractions.Dtos;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Permissions.Contracts;
using MyTodos.Services.IdentityService.Domain.PermissionAggregate;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Permissions.Commands.CreatePermission;

/// <summary>
/// Command to create a new permission.
/// </summary>
public sealed class CreatePermissionCommand : CreateCommand<Guid>
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

/// <summary>
/// Validator for CreatePermissionCommand.
/// </summary>
public sealed class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Permission code is required")
            .MaximumLength(200);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Permission name is required")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}

/// <summary>
/// Handler for creating a new permission.
/// </summary>
public sealed class CreatePermissionCommandHandler : CreateCommandHandler<CreatePermissionCommand, Guid>
{
    private readonly IPermissionWriteRepository _permissionWriteRepository;
    private readonly IPermissionReadRepository _permissionReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreatePermissionCommandHandler> _logger;

    public CreatePermissionCommandHandler(
        IPermissionWriteRepository permissionWriteRepository,
        IPermissionReadRepository permissionReadRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<CreatePermissionCommandHandler> logger)
    {
        _permissionWriteRepository = permissionWriteRepository;
        _permissionReadRepository = permissionReadRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override async Task<Result<CreateCommandResponseDto<Guid>>> Handle(
        CreatePermissionCommand request,
        CancellationToken ct)
    {
        // Authorization: Only Global.Admin can create permissions
        if (!_currentUserService.IsGlobalAdmin())
        {
            _logger.LogWarning("Permission creation failed: User is not a Global Administrator");
            return Forbidden("Only Global Administrators can create permissions");
        }

        // Check if permission code already exists
        var existingPermission = await _permissionReadRepository.GetByCodeAsync(request.Code, ct);
        if (existingPermission != null)
        {
            _logger.LogWarning("Permission creation failed: Permission with code '{Code}' already exists", request.Code);
            return Conflict($"Permission with code '{request.Code}' already exists");
        }

        // Create permission using domain factory method
        var permissionResult = Permission.Create(request.Code, request.Name, request.Description);
        if (permissionResult.IsFailure)
        {
            _logger.LogWarning("Permission creation failed: {Error}", permissionResult.FirstError.Description);
            return Failure(permissionResult.FirstError);
        }

        var permission = permissionResult.Value!;

        // Persist the permission
        await _permissionWriteRepository.AddAsync(permission, ct);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("Permission created: {PermissionId} with code {Code}", permission.Id, permission.Code);

        return Success(permission.Id);
    }
}
