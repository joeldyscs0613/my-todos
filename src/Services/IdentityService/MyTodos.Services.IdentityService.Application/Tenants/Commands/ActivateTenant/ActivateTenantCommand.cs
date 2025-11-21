using FluentValidation;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Tenants.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Tenants.Commands.ActivateTenant;

/// <summary>
/// Command to activate a tenant.
/// </summary>
public sealed class ActivateTenantCommand : Command
{
    public Guid TenantId { get; init; }

    public ActivateTenantCommand(Guid tenantId)
    {
        TenantId = tenantId;
    }

    // Parameterless constructor for model binding
    public ActivateTenantCommand()
    {
    }
}

/// <summary>
/// Validator for ActivateTenantCommand.
/// </summary>
public sealed class ActivateTenantCommandValidator : AbstractValidator<ActivateTenantCommand>
{
    public ActivateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required");
    }
}

/// <summary>
/// Handler for activating a tenant.
/// </summary>
public sealed class ActivateTenantCommandHandler : CommandHandler<ActivateTenantCommand>
{
    private readonly ITenantWriteRepository _tenantWriteRepository;
    private readonly ITenantPagedListReadRepository _tenantReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ActivateTenantCommandHandler> _logger;

    public ActivateTenantCommandHandler(
        ITenantWriteRepository tenantWriteRepository,
        ITenantPagedListReadRepository tenantReadRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<ActivateTenantCommandHandler> logger)
    {
        _tenantWriteRepository = tenantWriteRepository;
        _tenantReadRepository = tenantReadRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override async Task<Result> Handle(
        ActivateTenantCommand request,
        CancellationToken ct)
    {
        // Only Global Administrators can activate tenants
        if (!_currentUserService.IsGlobalAdmin())
        {
            _logger.LogWarning("Tenant activation failed: User is not a Global Administrator");
            return Forbidden("Only Global Administrators can activate tenants");
        }

        // Check if tenant exists
        var tenant = await _tenantReadRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
        {
            _logger.LogWarning("Tenant activation failed: Tenant with ID {TenantId} not found", request.TenantId);
            return NotFound($"Tenant with ID '{request.TenantId}' not found");
        }

        // Activate the tenant
        tenant.Activate();

        await _tenantWriteRepository.UpdateAsync(tenant, ct);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("Tenant activated: {TenantId}", tenant.Id);

        return Success();
    }
}
