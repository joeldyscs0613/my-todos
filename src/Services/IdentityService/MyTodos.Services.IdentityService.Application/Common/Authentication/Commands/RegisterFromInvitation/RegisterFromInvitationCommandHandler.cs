using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Features.Authentication.DTOs;
using MyTodos.Services.IdentityService.Domain.UserAggregate;
using MyTodos.Services.IdentityService.Domain.UserAggregate.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Features.Authentication.Commands.RegisterFromInvitation;

/// <summary>
/// Handler for registering a user from an invitation.
/// </summary>
public sealed class RegisterFromInvitationCommandHandler
    : ResponseCommandHandler<RegisterFromInvitationCommand, SignInResponseDto>
{
    private readonly IUserInvitationReadRepository _invitationReadRepository;
    private readonly IUserInvitationWriteRepository _invitationWriteRepository;
    private readonly IUserReadRepository _userReadRepository;
    private readonly IUserWriteRepository _userWriteRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterFromInvitationCommandHandler> _logger;

    public RegisterFromInvitationCommandHandler(
        IUserInvitationReadRepository invitationReadRepository,
        IUserInvitationWriteRepository invitationWriteRepository,
        IUserReadRepository userReadRepository,
        IUserWriteRepository userWriteRepository,
        IPasswordHashingService passwordHashingService,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        ILogger<RegisterFromInvitationCommandHandler> logger)
    {
        _invitationReadRepository = invitationReadRepository;
        _invitationWriteRepository = invitationWriteRepository;
        _userReadRepository = userReadRepository;
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
        var invitation = await _invitationReadRepository.GetByTokenAsync(request.InvitationToken, ct);
        if (invitation == null)
        {
            _logger.LogWarning("Registration failed: Invalid invitation token");
            return BadRequest("Invalid or expired invitation");
        }

        // Validate invitation
        if (!invitation.IsValid())
        {
            _logger.LogWarning("Registration failed: Invitation {InvitationId} is not valid (Status: {Status}, Expired: {IsExpired})",
                invitation.Id, invitation.Status, invitation.IsExpired());
            return BadRequest("Invalid or expired invitation");
        }

        // Check if user already exists with this email
        var existingUser = await _userReadRepository.GetByEmailAsync(invitation.Email, ct);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed: User already exists with email {Email}", invitation.Email);
            return Conflict("A user with this email already exists");
        }

        // Check if username is already taken
        var existingUsername = await _userReadRepository.GetByUsernameAsync(request.Username, ct);
        if (existingUsername != null)
        {
            _logger.LogWarning("Registration failed: Username {Username} is already taken", request.Username);
            return Conflict("Username is already taken");
        }

        // Create new user
        var passwordHash = _passwordHashingService.HashPassword(request.Password);
        var user = User.Create(
            request.Username,
            invitation.Email,
            passwordHash,
            request.FirstName,
            request.LastName
        );

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
        await _invitationWriteRepository.UpdateAsync(invitation, ct);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("User {UserId} registered successfully from invitation {InvitationId}",
            user.Id, invitation.Id);

        // Get user with roles and permissions for token generation
        var userWithPermissions = await _userReadRepository.GetByIdWithFullPermissionsAsync(user.Id, ct);
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
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        return Success(response);
    }
}
