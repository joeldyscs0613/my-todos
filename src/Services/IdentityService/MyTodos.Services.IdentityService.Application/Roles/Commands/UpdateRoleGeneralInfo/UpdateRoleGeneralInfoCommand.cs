using FluentValidation;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Roles.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Roles.Commands.UpdateRoleGeneralInfo;

/// <summary>
/// Command to update role general information (name and description).
/// Code and Scope are immutable and cannot be updated.
/// </summary>
public sealed class UpdateRoleGeneralInfoCommand : Command
{
    public Guid RoleId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }

    public UpdateRoleGeneralInfoCommand(Guid roleId, string name, string? description)
    {
        RoleId = roleId;
        Name = name;
        Description = description;
    }
}

/// <summary>
/// Validator for UpdateRoleGeneralInfoCommand.
/// </summary>
public sealed class UpdateRoleGeneralInfoCommandValidator : AbstractValidator<UpdateRoleGeneralInfoCommand>
{
    public UpdateRoleGeneralInfoCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("Role ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Role name is required")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);
    }
}

/// <summary>
/// Handler for updating role general information.
/// </summary>
public sealed class UpdateRoleGeneralInfoCommandHandler : CommandHandler<UpdateRoleGeneralInfoCommand>
{
    private readonly IRoleReadRepository _roleReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateRoleGeneralInfoCommandHandler> _logger;

    public UpdateRoleGeneralInfoCommandHandler(
        IRoleReadRepository roleReadRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<UpdateRoleGeneralInfoCommandHandler> logger)
    {
        _roleReadRepository = roleReadRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override async Task<Result> Handle(
        UpdateRoleGeneralInfoCommand request,
        CancellationToken ct)
    {
        // Authorization: Only Global.Admin can update roles
        if (!_currentUserService.IsGlobalAdmin())
        {
            _logger.LogWarning("Role update failed: User is not a Global Administrator");
            return Forbidden("Only Global Administrators can update roles");
        }

        // Get the role
        var role = await _roleReadRepository.GetByIdAsync(request.RoleId, ct);
        if (role is null)
        {
            _logger.LogWarning("Role update failed: Role with ID {RoleId} not found", request.RoleId);
            return NotFound($"Role with ID '{request.RoleId}' not found");
        }

        // Update role using domain method
        var updateResult = role.Update(request.Name, request.Description);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        // Persist changes
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("Role general info updated: {RoleId}", role.Id);

        return Success();
    }
}
