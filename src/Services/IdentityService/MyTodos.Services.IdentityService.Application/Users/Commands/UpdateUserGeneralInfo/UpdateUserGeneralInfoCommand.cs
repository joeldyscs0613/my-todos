using FluentValidation;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.Services.IdentityService.Domain.UserAggregate.Constants;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Users.Commands.UpdateUserGeneralInfo;

/// <summary>
/// Command to update a user's general information (first name, last name).
/// This follows CQRS principles - it updates general user profile fields.
/// </summary>
public sealed class UpdateUserGeneralInfoCommand : Command
{
    public Guid UserId { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }

    public UpdateUserGeneralInfoCommand(Guid userId, string? firstName, string? lastName)
    {
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
    }

    // Parameterless constructor for model binding
    public UpdateUserGeneralInfoCommand()
    {
    }
}

/// <summary>
/// Validator for UpdateUserGeneralInfoCommand.
/// </summary>
public sealed class UpdateUserGeneralInfoCommandValidator : AbstractValidator<UpdateUserGeneralInfoCommand>
{
    public UpdateUserGeneralInfoCommandValidator()
    {
        // UserId is optional in the body - it will be set from the route parameter in the controller
        // Only validate if it's provided to ensure it's not an empty GUID
        When(x => x.UserId != Guid.Empty, () =>
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");
        });

        // FirstName is optional but has max length if provided
        When(x => !string.IsNullOrWhiteSpace(x.FirstName), () =>
        {
            RuleFor(x => x.FirstName)
                .MaximumLength(UserConstants.FieldLengths.FirstNameMaxLength)
                .WithMessage(string.Format(UserConstants.ErrorMessages.FirstNameTooLong,
                    UserConstants.FieldLengths.FirstNameMaxLength));
        });

        // LastName is optional but has max length if provided
        When(x => !string.IsNullOrWhiteSpace(x.LastName), () =>
        {
            RuleFor(x => x.LastName)
                .MaximumLength(UserConstants.FieldLengths.LastNameMaxLength)
                .WithMessage(string.Format(UserConstants.ErrorMessages.LastNameTooLong,
                    UserConstants.FieldLengths.LastNameMaxLength));
        });
    }
}

/// <summary>
/// Handler for updating a user's general information.
/// </summary>
public sealed class UpdateUserGeneralInfoCommandHandler : CommandHandler<UpdateUserGeneralInfoCommand>
{
    private readonly IUserWriteRepository _userWriteRepository;
    private readonly IUserPagedListReadRepository _userReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateUserGeneralInfoCommandHandler> _logger;

    public UpdateUserGeneralInfoCommandHandler(
        IUserWriteRepository userWriteRepository,
        IUserPagedListReadRepository userReadRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<UpdateUserGeneralInfoCommandHandler> logger)
    {
        _userWriteRepository = userWriteRepository;
        _userReadRepository = userReadRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override async Task<Result> Handle(
        UpdateUserGeneralInfoCommand request,
        CancellationToken ct)
    {
        // Authorization: Global.Admin or Tenant.Admin can update users
        var isGlobalAdmin = _currentUserService.IsGlobalAdmin();
        var isTenantAdmin = _currentUserService.IsTenantAdmin();

        if (!isGlobalAdmin && !isTenantAdmin)
        {
            _logger.LogWarning("User update failed: Current user does not have permission to update users");
            return Forbidden("You do not have permission to update users");
        }

        // Check if user exists
        var user = await _userReadRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            _logger.LogWarning("User update failed: User with ID {UserId} not found", request.UserId);
            return NotFound($"User with ID '{request.UserId}' not found");
        }

        // Tenant.Admin can only update users within their tenant
        if (isTenantAdmin && !isGlobalAdmin)
        {
            var currentUserTenantId = _currentUserService.TenantId;
            if (!currentUserTenantId.HasValue)
            {
                _logger.LogWarning("User update failed: Tenant admin has no tenant ID in claims");
                return Forbidden("No tenant context found");
            }

            var userTenantIds = user.GetTenantIds();
            if (!userTenantIds.Contains(currentUserTenantId.Value))
            {
                _logger.LogWarning("User update failed: Tenant admin attempting to update user in different tenant. User {UserId}, Tenant {TenantId}", request.UserId, currentUserTenantId.Value);
                return Forbidden("You can only update users within your own tenant");
            }
        }

        // Update the user's profile
        user.UpdateProfile(request.FirstName, request.LastName);

        await _userWriteRepository.UpdateAsync(user, ct);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("User general info updated: {UserId}", user.Id);

        return Success();
    }
}
