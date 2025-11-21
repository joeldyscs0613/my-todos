using FluentValidation;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Users.Commands.ActivateUser;

/// <summary>
/// Command to activate a user.
/// </summary>
public sealed class ActivateUserCommand : Command
{
    public Guid UserId { get; init; }

    public ActivateUserCommand(Guid userId)
    {
        UserId = userId;
    }

    // Parameterless constructor for model binding
    public ActivateUserCommand()
    {
    }
}

/// <summary>
/// Validator for ActivateUserCommand.
/// </summary>
public sealed class ActivateUserCommandValidator : AbstractValidator<ActivateUserCommand>
{
    public ActivateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}

/// <summary>
/// Handler for activating a user.
/// </summary>
public sealed class ActivateUserCommandHandler : CommandHandler<ActivateUserCommand>
{
    private readonly IUserWriteRepository _userWriteRepository;
    private readonly IUserPagedListReadRepository _userReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ActivateUserCommandHandler> _logger;

    public ActivateUserCommandHandler(
        IUserWriteRepository userWriteRepository,
        IUserPagedListReadRepository userReadRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<ActivateUserCommandHandler> logger)
    {
        _userWriteRepository = userWriteRepository;
        _userReadRepository = userReadRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override async Task<Result> Handle(
        ActivateUserCommand request,
        CancellationToken ct)
    {
        // Authorization: Global.Admin or Tenant.Admin can activate users
        var isGlobalAdmin = _currentUserService.IsGlobalAdmin();
        var isTenantAdmin = _currentUserService.IsTenantAdmin();

        if (!isGlobalAdmin && !isTenantAdmin)
        {
            _logger.LogWarning("User activation failed: Current user does not have permission to activate users");
            return Forbidden("You do not have permission to activate users");
        }

        // Check if user exists
        var user = await _userReadRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            _logger.LogWarning("User activation failed: User with ID {UserId} not found", request.UserId);
            return NotFound($"User with ID '{request.UserId}' not found");
        }

        // Tenant.Admin can only activate users within their tenant
        if (isTenantAdmin && !isGlobalAdmin)
        {
            var currentUserTenantId = _currentUserService.TenantId;
            if (!currentUserTenantId.HasValue)
            {
                _logger.LogWarning("User activation failed: Tenant admin has no tenant ID in claims");
                return Forbidden("No tenant context found");
            }

            var userTenantIds = user.GetTenantIds();
            if (!userTenantIds.Contains(currentUserTenantId.Value))
            {
                _logger.LogWarning("User activation failed: Tenant admin attempting to activate user in different tenant. User {UserId}, Tenant {TenantId}", request.UserId, currentUserTenantId.Value);
                return Forbidden("You can only activate users within your own tenant");
            }
        }

        // Activate the user
        user.Activate();

        await _userWriteRepository.UpdateAsync(user, ct);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("User activated: {UserId}", user.Id);

        return Success();
    }
}
