using FluentValidation;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Users.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Users.Commands.DeleteUser;

/// <summary>
/// Command to permanently delete a user from the database.
/// This is a hard delete operation that is only available to Global.Admin.
/// Tenant admins should use DeactivateUser instead.
/// </summary>
public sealed class DeleteUserCommand : Command
{
    public Guid UserId { get; init; }

    public DeleteUserCommand(Guid userId)
    {
        UserId = userId;
    }

    // Parameterless constructor for model binding
    public DeleteUserCommand()
    {
    }
}

/// <summary>
/// Validator for DeleteUserCommand.
/// </summary>
public sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}

/// <summary>
/// Handler for permanently deleting a user.
/// Only Global.Admin can delete users.
/// </summary>
public sealed class DeleteUserCommandHandler : CommandHandler<DeleteUserCommand>
{
    private readonly IUserWriteRepository _userWriteRepository;
    private readonly IUserPagedListReadRepository _userReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(
        IUserWriteRepository userWriteRepository,
        IUserPagedListReadRepository userReadRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<DeleteUserCommandHandler> logger)
    {
        _userWriteRepository = userWriteRepository;
        _userReadRepository = userReadRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override async Task<Result> Handle(
        DeleteUserCommand request,
        CancellationToken ct)
    {
        // Authorization: Only Global.Admin can delete users
        // Tenant admins should use DeactivateUser instead
        if (!_currentUserService.IsGlobalAdmin())
        {
            _logger.LogWarning("User deletion failed: Only Global Administrators can permanently delete users");
            return Forbidden("Only Global Administrators can permanently delete users. Use deactivate for tenant-level user management.");
        }

        // Check if user exists
        var user = await _userReadRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            _logger.LogWarning("User deletion failed: User with ID {UserId} not found", request.UserId);
            return NotFound($"User with ID '{request.UserId}' not found");
        }

        // Prevent self-deletion
        var currentUserId = _currentUserService.UserId;
        if (currentUserId.HasValue && currentUserId.Value == request.UserId)
        {
            _logger.LogWarning("User deletion failed: User {UserId} attempted to delete themselves", request.UserId);
            return Conflict("You cannot delete your own user account");
        }

        // Perform hard delete
        await _userWriteRepository.DeleteAsync(user, ct);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("User permanently deleted: {UserId} ({Email})", user.Id, user.Email);

        return Success();
    }
}
