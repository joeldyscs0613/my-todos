using FluentValidation;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Common.Authentication.Commands.SignIn;
using MyTodos.Services.IdentityService.Application.Common.Authentication.DTOs;
using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.Services.IdentityService.Domain.UserAggregate;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Common.Authentication.Commands.RegisterFromInvitation;

/// <summary>
/// Command to register a new user from an invitation token.
/// </summary>
public sealed class RegisterFromInvitationCommand : ResponseCommand<SignInResponseDto>
{
    public string InvitationToken { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}

/// <summary>
/// Validator for RegisterFromInvitationCommand.
/// </summary>
public sealed class RegisterFromInvitationCommandValidator : AbstractValidator<RegisterFromInvitationCommand>
{
    public RegisterFromInvitationCommandValidator()
    {
        RuleFor(x => x.InvitationToken)
            .NotEmpty()
            .WithMessage("Invitation token is required");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters")
            .MaximumLength(50)
            .WithMessage("Username must not exceed 50 characters")
            .Matches("^[a-zA-Z0-9_-]+$")
            .WithMessage("Username can only contain letters, numbers, underscores and hyphens");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]")
            .WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]")
            .WithMessage("Password must contain at least one number");

        RuleFor(x => x.FirstName)
            .MaximumLength(50)
            .WithMessage("First name must not exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(50)
            .WithMessage("Last name must not exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));
    }
}

/// <summary>
/// Handler for registering a user from an invitation.
/// </summary>
public sealed class RegisterFromInvitationCommandHandler
    : ResponseCommandHandler<RegisterFromInvitationCommand, SignInResponseDto>
{
    private readonly IUserPagedListReadRepository _userPagedListReadRepository;
    private readonly IUserWriteRepository _userWriteRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterFromInvitationCommandHandler> _logger;

    public RegisterFromInvitationCommandHandler(
        IUserPagedListReadRepository userPagedListReadRepository,
        IUserWriteRepository userWriteRepository,
        IPasswordHashingService passwordHashingService,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        ILogger<RegisterFromInvitationCommandHandler> logger)
    {
        _userPagedListReadRepository = userPagedListReadRepository;
        _userWriteRepository = userWriteRepository;
        _passwordHashingService = passwordHashingService;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<Result<SignInResponseDto>> Handle(
        RegisterFromInvitationCommand request,
        CancellationToken ct)
    {
        // Find invitation by token
        var invitation = await _userPagedListReadRepository.GetUserInvitationByTokenAsync(request.InvitationToken, ct);
        if (invitation == null)
        {
            _logger.LogWarning("Registration failed: Invalid invitation token");
            return BadRequest("Invalid or expired invitation");
        }

        // Validate invitation
        if (!invitation.IsValid())
        {
            _logger.LogWarning(
                "Registration failed: Invitation {InvitationId} is not valid (Status: {Status}, Expired: {IsExpired})",
                invitation.Id, invitation.Status, invitation.IsExpired());
            return BadRequest("Invalid or expired invitation");
        }

        // Check if user already exists with this email
        var existingUser = await _userPagedListReadRepository.GetByEmailAsync(invitation.Email, ct);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed: User already exists with email {Email}", invitation.Email);
            return Conflict("A user with this email already exists");
        }

        // Check if username is already taken
        var existingUsername = await _userPagedListReadRepository.GetByUsernameAsync(request.Username, ct);
        if (existingUsername != null)
        {
            _logger.LogWarning("Registration failed: Username {Username} is already taken", request.Username);
            return Conflict("Username is already taken");
        }

        // Create new user
        var passwordHash = _passwordHashingService.HashPassword(request.Password);
        var userResult = User.Create(
            request.Username,
            invitation.Email,
            passwordHash,
            request.FirstName,
            request.LastName
        );

        if (userResult.IsFailure)
        {
            _logger.LogWarning("User creation failed: {Error}", userResult.FirstError.Description);
            return Failure(userResult.FirstError);
        }

        var user = userResult.Value!;

        // Assign role from invitation
        if (invitation.TenantId.HasValue)
        {
            user.AssignTenantRole(invitation.RoleId, invitation.TenantId.Value);
        }
        else
        {
            user.AssignGlobalRole(invitation.RoleId);
        }

        // Mark invitation as accepted
        invitation.MarkAsAccepted();

        // Save changes
        await _userWriteRepository.AddAsync(user, ct);
        await _userWriteRepository.UpdateUserInvitationAsync(invitation, ct);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("User {UserId} registered successfully from invitation {InvitationId}",
            user.Id, invitation.Id);

        // Get user with roles and permissions for token generation
        var userWithPermissions = await _userPagedListReadRepository.GetByIdAsync(user.Id, ct);
        if (userWithPermissions == null)
        {
            return NotFound("User not found after registration");
        }

        // Get role names for JWT
        var roleNames = userWithPermissions.UserRoles
            .Select(ur => ur.Role.Name)
            .Distinct()
            .ToList();

        // Get all permissions from all roles (aggregate and deduplicate)
        var permissions = userWithPermissions.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        // Generate JWT token with roles and permissions
        var tokenTenantId = invitation.TenantId ?? Guid.Empty;
        var token = _jwtTokenService.GenerateUserToken(
            user.Id,
            user.Email,
            tokenTenantId,
            roleNames,
            permissions
        );

        var response = new SignInResponseDto
        {
            Token = token,
            User = new UserAuthDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                TenantId = invitation.TenantId,
                Roles = roleNames
            },
            ExpiresAt = DateTimeOffsetHelper.UtcNow.AddHours(1)
        };

        return Success(response);
    }
}
