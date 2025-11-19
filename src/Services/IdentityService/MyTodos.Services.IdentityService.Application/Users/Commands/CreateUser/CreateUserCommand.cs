using FluentValidation;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Roles.Contracts;
using MyTodos.Services.IdentityService.Application.Tenants.Contracts;
using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.Services.IdentityService.Domain.UserAggregate;
using MyTodos.Services.IdentityService.Domain.UserAggregate.Constants;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Users.Commands.CreateUser;

/// <summary>
/// Command to create a new user directly (temporary endpoint for development).
/// This bypasses the invitation flow and creates a user immediately with a default password.
/// </summary>
public sealed class CreateUserCommand : ResponseCommand<CreateUserResponseDto>
{
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public Guid RoleId { get; init; }
    public Guid? TenantId { get; init; }
}

/// <summary>
/// Response DTO for CreateUser command containing the generated credentials.
/// </summary>
public sealed record CreateUserResponseDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

/// <summary>
/// Validator for CreateUserCommand.
/// </summary>
public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(UserConstants.ErrorMessages.EmailRequired)
            .EmailAddress()
            .WithMessage(UserConstants.ErrorMessages.EmailInvalid)
            .MaximumLength(UserConstants.FieldLengths.EmailMaxLength)
            .WithMessage(string.Format(UserConstants.ErrorMessages.EmailTooLong, UserConstants.FieldLengths.EmailMaxLength));

        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("Role is required");

        // FirstName, LastName, and TenantId are optional
        RuleFor(x => x.FirstName)
            .MaximumLength(UserConstants.FieldLengths.FirstNameMaxLength)
            .WithMessage(string.Format(UserConstants.ErrorMessages.FirstNameTooLong, UserConstants.FieldLengths.FirstNameMaxLength))
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(UserConstants.FieldLengths.LastNameMaxLength)
            .WithMessage(string.Format(UserConstants.ErrorMessages.LastNameTooLong, UserConstants.FieldLengths.LastNameMaxLength))
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));
    }
}

/// <summary>
/// Handler for creating a user directly with default credentials.
/// This is a temporary endpoint for development purposes.
/// </summary>
public sealed class CreateUserCommandHandler : ResponseCommandHandler<CreateUserCommand, CreateUserResponseDto>
{
    private const string DefaultPassword = "Password@123!";

    private readonly IUserWriteRepository _userWriteRepository;
    private readonly IUserPagedListReadRepository _userPagedListReadRepository;
    private readonly IRoleReadRepository _roleReadRepository;
    private readonly ITenantPagedListReadRepository _tenantPagedListReadRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IUserWriteRepository userWriteRepository,
        IUserPagedListReadRepository userPagedListReadRepository,
        IRoleReadRepository roleReadRepository,
        ITenantPagedListReadRepository tenantPagedListReadRepository,
        IPasswordHashingService passwordHashingService,
        IUnitOfWork unitOfWork,
        ILogger<CreateUserCommandHandler> logger)
    {
        _userWriteRepository = userWriteRepository;
        _userPagedListReadRepository = userPagedListReadRepository;
        _roleReadRepository = roleReadRepository;
        _tenantPagedListReadRepository = tenantPagedListReadRepository;
        _passwordHashingService = passwordHashingService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<Result<CreateUserResponseDto>> Handle(
        CreateUserCommand request,
        CancellationToken ct)
    {
        // Extract username from email (part before @)
        var username = ExtractUsernameFromEmail(request.Email);

        // Check if user already exists with this email
        var existingUserByEmail = await _userPagedListReadRepository.GetByEmailAsync(request.Email, ct);
        if (existingUserByEmail != null)
        {
            _logger.LogWarning("User creation failed: User already exists with email {Email}", request.Email);
            return Conflict("A user with this email already exists");
        }

        // Check if user already exists with this username
        var existingUserByUsername = await _userPagedListReadRepository.GetByUsernameAsync(username, ct);
        if (existingUserByUsername != null)
        {
            _logger.LogWarning("User creation failed: User already exists with username {Username}", username);
            return Conflict("A user with this username already exists");
        }

        // Validate role exists
        var role = await _roleReadRepository.GetByIdAsync(request.RoleId, ct);
        if (role == null)
        {
            _logger.LogWarning("User creation failed: Role {RoleId} not found", request.RoleId);
            return NotFound("Role not found");
        }

        // If tenant-scoped, validate tenant exists
        if (request.TenantId.HasValue)
        {
            var tenant = await _tenantPagedListReadRepository.GetByIdAsync(request.TenantId.Value, ct);
            if (tenant == null)
            {
                _logger.LogWarning("User creation failed: Tenant {TenantId} not found", request.TenantId);
                return NotFound("Tenant not found");
            }

            if (!tenant.IsActive)
            {
                _logger.LogWarning("User creation failed: Tenant {TenantId} is inactive", request.TenantId);
                return BadRequest("Cannot create users in an inactive tenant");
            }
        }

        // Hash the default password
        var passwordHash = _passwordHashingService.HashPassword(DefaultPassword);

        // Create the user
        var userResult = User.Create(
            username,
            request.Email,
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

        // Assign role
        if (request.TenantId.HasValue)
        {
            user.AssignTenantRole(request.RoleId, request.TenantId.Value);
        }
        else
        {
            user.AssignGlobalRole(request.RoleId);
        }

        // Save to database
        await _userWriteRepository.AddAsync(user, ct);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("User created successfully: {UserId} with username {Username} and email {Email}",
            user.Id, username, request.Email);

        return Success(new CreateUserResponseDto
        {
            UserId = user.Id,
            Username = username,
            Password = DefaultPassword
        });
    }

    private static string ExtractUsernameFromEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        return atIndex > 0 ? email.Substring(0, atIndex) : email;
    }
}
