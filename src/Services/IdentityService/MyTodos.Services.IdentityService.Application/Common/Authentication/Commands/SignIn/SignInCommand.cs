using FluentValidation;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Common.Authentication.DTOs;
using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Common.Authentication.Commands.SignIn;

/// <summary>
/// Command to sign in a user with username/email and password.
/// </summary>
public sealed class SignInCommand : ResponseCommand<SignInResponseDto>
{
    public string UsernameOrEmail { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

/// <summary>
/// Sign-in response containing JWT token and user information.
/// </summary>
public sealed record SignInResponseDto
{
    public string Token { get; init; } = string.Empty;
    public UserAuthDto User { get; init; } = null!;
    public DateTimeOffset ExpiresAt { get; init; }
}

/// <summary>
/// Validator for SignInCommand.
/// </summary>
public sealed class SignInCommandValidator : AbstractValidator<SignInCommand>
{
    public SignInCommandValidator()
    {
        RuleFor(x => x.UsernameOrEmail)
            .NotEmpty()
            .WithMessage("Username or email is required");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters");
    }
}

/// <summary>
/// Handler for user sign-in command.
/// </summary>
public sealed class SignInCommandHandler : ResponseCommandHandler<SignInCommand, SignInResponseDto>
{
    private readonly IUserPagedListReadRepository _userPagedListReadRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SignInCommandHandler> _logger;

    public SignInCommandHandler(
        IUserPagedListReadRepository userPagedListReadRepository,
        IPasswordHashingService passwordHashingService,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        ILogger<SignInCommandHandler> logger)
    {
        _userPagedListReadRepository = userPagedListReadRepository;
        _passwordHashingService = passwordHashingService;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<Result<SignInResponseDto>> Handle(SignInCommand request, CancellationToken ct)
    {
        // Find user by username or email
        var user = await _userPagedListReadRepository.GetByUsernameAsync(request.UsernameOrEmail, ct);
        user ??= await _userPagedListReadRepository.GetByEmailAsync(request.UsernameOrEmail, ct);

        if (user == null)
        {
            _logger.LogWarning("Sign-in failed: User not found for {UsernameOrEmail}", request.UsernameOrEmail);
            return Unauthorized("Invalid username/email or password");
        }

        // Verify password
        if (!_passwordHashingService.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Sign-in failed: Invalid password for user {UserId}", user.Id);
            return Unauthorized("Invalid username/email or password");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            _logger.LogWarning("Sign-in failed: User {UserId} is inactive", user.Id);
            return Forbidden("User account is inactive");
        }

        // Get primary tenant ID (first tenant-scoped role, or null for global users)
        var primaryTenantId = user.UserRoles
            .Where(ur => ur.TenantId.HasValue)
            .Select(ur => ur.TenantId!.Value)
            .FirstOrDefault();

        // Get role names for JWT claims
        var roleNames = user.UserRoles
            .Select(ur => ur.Role.Name)
            .Distinct()
            .ToList();

        // Get all permissions from all roles (aggregate and deduplicate)
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        // Generate JWT token with roles and permissions
        // For global admins (no tenant), use a default system tenant ID for the token
        var tokenTenantId = primaryTenantId != Guid.Empty ? primaryTenantId : Guid.Empty;
        var token = _jwtTokenService.GenerateUserToken(
            user.Id,
            user.Email,
            tokenTenantId,
            roleNames,
            permissions
        );

        // Record login event
        user.RecordLogin();
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("User {UserId} signed in successfully", user.Id);

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
                TenantId = primaryTenantId != Guid.Empty ? primaryTenantId : null,
                Roles = roleNames
            },
            ExpiresAt = DateTimeOffsetHelper.UtcNow.AddHours(1) // Should match JWT settings
        };

        return Success(response);
    }
}
