using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Abstractions.Dtos;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Domain.RoleAggregate.Contracts;
using MyTodos.Services.IdentityService.Domain.TenantAggregate.Contracts;
using MyTodos.Services.IdentityService.Domain.UserAggregate;
using MyTodos.Services.IdentityService.Domain.UserAggregate.Contracts;
using MyTodos.Services.IdentityService.Domain.UserAggregate.DomainEvents;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Features.Users.Commands.InviteUser;

/// <summary>
/// Handler for inviting a user.
/// </summary>
public sealed class InviteUserCommandHandler : CreateCommandHandler<InviteUserCommand, Guid>
{
    private readonly IUserInvitationReadRepository _invitationReadRepository;
    private readonly IUserInvitationWriteRepository _invitationWriteRepository;
    private readonly IUserReadRepository _userReadRepository;
    private readonly IRoleReadRepository _roleReadRepository;
    private readonly ITenantReadRepository _tenantReadRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InviteUserCommandHandler> _logger;

    public InviteUserCommandHandler(
        IUserInvitationReadRepository invitationReadRepository,
        IUserInvitationWriteRepository invitationWriteRepository,
        IUserReadRepository userReadRepository,
        IRoleReadRepository roleReadRepository,
        ITenantReadRepository tenantReadRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger<InviteUserCommandHandler> logger)
    {
        _invitationReadRepository = invitationReadRepository;
        _invitationWriteRepository = invitationWriteRepository;
        _userReadRepository = userReadRepository;
        _roleReadRepository = roleReadRepository;
        _tenantReadRepository = tenantReadRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<Result<CreateCommandResponseDto<Guid>>> Handle(
        InviteUserCommand request,
        CancellationToken ct)
    {
        // Ensure user is authenticated
        if (!_currentUserService.UserId.HasValue)
        {
            return Unauthorized("User must be authenticated to send invitations");
        }

        // Check if user already exists with this email
        var existingUser = await _userReadRepository.GetByEmailAsync(request.Email, ct);
        if (existingUser != null)
        {
            _logger.LogWarning("Invitation failed: User already exists with email {Email}", request.Email);
            return Conflict("A user with this email already exists");
        }

        // Check if there's already a pending invitation for this email
        var existingInvitation = await _invitationReadRepository.ExistsForEmailAsync(request.Email, ct);
        if (existingInvitation)
        {
            _logger.LogWarning("Invitation failed: Pending invitation already exists for {Email}", request.Email);
            return Conflict("A pending invitation already exists for this email");
        }

        // Validate role exists
        var role = await _roleReadRepository.GetByIdAsync(request.RoleId, ct);
        if (role == null)
        {
            _logger.LogWarning("Invitation failed: Role {RoleId} not found", request.RoleId);
            return NotFound("Role not found");
        }

        // If tenant-scoped invitation, validate tenant exists
        if (request.TenantId.HasValue)
        {
            var tenant = await _tenantReadRepository.GetByIdAsync(request.TenantId.Value, ct);
            if (tenant == null)
            {
                _logger.LogWarning("Invitation failed: Tenant {TenantId} not found", request.TenantId);
                return NotFound("Tenant not found");
            }

            if (!tenant.IsActive)
            {
                _logger.LogWarning("Invitation failed: Tenant {TenantId} is inactive", request.TenantId);
                return BadRequest("Cannot invite users to an inactive tenant");
            }
        }

        // Create invitation
        var invitation = UserInvitation.Create(
            _currentUserService.UserId.Value,
            request.Email,
            request.RoleId,
            request.TenantId
        );

        // Add domain event for email notification (simulated)
        var domainEvent = new UserInvitedDomainEvent(
            invitation.Id,
            invitation.Email,
            invitation.InvitedByUserId,
            invitation.TenantId,
            invitation.RoleId,
            invitation.InvitationToken,
            invitation.ExpiresAt
        );

        // Note: Domain events would normally be added to an aggregate root
        // For now, we'll just log it since UserInvitation is not an aggregate root

        await _invitationWriteRepository.AddAsync(invitation, ct);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("User invitation created: {InvitationId} for email {Email} by user {UserId}",
            invitation.Id, invitation.Email, _currentUserService.UserId.Value);

        return Success(invitation.Id);
    }
}
