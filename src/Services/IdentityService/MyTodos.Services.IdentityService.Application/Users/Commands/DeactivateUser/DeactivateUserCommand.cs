using FluentValidation;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Users.Commands.DeactivateUser;

/// <summary>
/// Command to deactivate a user.
/// </summary>
public sealed class DeactivateUserCommand : Command
{
    public Guid UserId { get; init; }

    public DeactivateUserCommand(Guid userId)
    {
        UserId = userId;
    }

    // Parameterless constructor for model binding
    public DeactivateUserCommand()
    {
    }
}

/// <summary>
/// Validator for DeactivateUserCommand.
/// </summary>
public sealed class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}

/// <summary>
/// Handler for deactivating a user.
/// </summary>
public sealed class DeactivateUserCommandHandler : CommandHandler<DeactivateUserCommand>
{
    private readonly IUserWriteRepository _userWriteRepository;
    private readonly IUserPagedListReadRepository _userReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeactivateUserCommandHandler> _logger;

    public DeactivateUserCommandHandler(
        IUserWriteRepository userWriteRepository,
        IUserPagedListReadRepository userReadRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<DeactivateUserCommandHandler> logger)
    {
        _userWriteRepository = userWriteRepository;
        _userReadRepository = userReadRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override async Task<Result> Handle(
        DeactivateUserCommand request,
        CancellationToken ct)
    {
        // Authorization: Global.Admin or Tenant.Admin can deactivate users
        var isGlobalAdmin = _currentUserService.IsGlobalAdmin();
        var isTenantAdmin = _currentUserService.IsTenantAdmin();

        if (!isGlobalAdmin && !isTenantAdmin)
        {
            _logger.LogWarning("User deactivation failed: Current user does not have permission to deactivate users");
            return Forbidden("You do not have permission to deactivate users");
        }

        // Check if user exists
        var user = await _userReadRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            _logger.LogWarning("User deactivation failed: User with ID {UserId} not found", request.UserId);
            return NotFound($"User with ID '{request.UserId}' not found");
        }

        // Tenant.Admin can only deactivate users within their tenant
        if (isTenantAdmin && !isGlobalAdmin)
        {
            var currentUserTenantId = _currentUserService.TenantId;
            if (!currentUserTenantId.HasValue)
            {
                _logger.LogWarning("User deactivation failed: Tenant admin has no tenant ID in claims");
                return Forbidden("No tenant context found");
            }

            var userTenantIds = user.GetTenantIds();
            if (!userTenantIds.Contains(currentUserTenantId.Value))
            {
                _logger.LogWarning("User deactivation failed: Tenant admin attempting to deactivate user in different tenant. User {UserId}, Tenant {TenantId}", request.UserId, currentUserTenantId.Value);
                return Forbidden("You can only deactivate users within your own tenant");
            }
        }

        // Deactivate the user
        user.Deactivate();

        await _userWriteRepository.UpdateAsync(user, ct);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("User deactivated: {UserId}", user.Id);

        return Success();
    }
}
