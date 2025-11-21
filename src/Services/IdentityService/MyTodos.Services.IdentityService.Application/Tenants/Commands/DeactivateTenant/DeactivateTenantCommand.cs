using FluentValidation;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Tenants.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Tenants.Commands.DeactivateTenant;

/// <summary>
/// Command to deactivate a tenant.
/// </summary>
public sealed class DeactivateTenantCommand : Command
{
    public Guid TenantId { get; init; }

    public DeactivateTenantCommand(Guid tenantId)
    {
        TenantId = tenantId;
    }

    // Parameterless constructor for model binding
    public DeactivateTenantCommand()
    {
    }
}

/// <summary>
/// Validator for DeactivateTenantCommand.
/// </summary>
public sealed class DeactivateTenantCommandValidator : AbstractValidator<DeactivateTenantCommand>
{
    public DeactivateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required");
    }
}

/// <summary>
/// Handler for deactivating a tenant.
/// </summary>
public sealed class DeactivateTenantCommandHandler : CommandHandler<DeactivateTenantCommand>
{
    private readonly ITenantWriteRepository _tenantWriteRepository;
    private readonly ITenantPagedListReadRepository _tenantReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeactivateTenantCommandHandler> _logger;

    public DeactivateTenantCommandHandler(
        ITenantWriteRepository tenantWriteRepository,
        ITenantPagedListReadRepository tenantReadRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<DeactivateTenantCommandHandler> logger)
    {
        _tenantWriteRepository = tenantWriteRepository;
        _tenantReadRepository = tenantReadRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override async Task<Result> Handle(
        DeactivateTenantCommand request,
        CancellationToken ct)
    {
        // Only Global Administrators can deactivate tenants
        if (!_currentUserService.IsGlobalAdmin())
        {
            _logger.LogWarning("Tenant deactivation failed: User is not a Global Administrator");
            return Forbidden("Only Global Administrators can deactivate tenants");
        }

        // Check if tenant exists
        var tenant = await _tenantReadRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
        {
            _logger.LogWarning("Tenant deactivation failed: Tenant with ID {TenantId} not found", request.TenantId);
            return NotFound($"Tenant with ID '{request.TenantId}' not found");
        }

        // Deactivate the tenant
        tenant.Deactivate();

        await _tenantWriteRepository.UpdateAsync(tenant, ct);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("Tenant deactivated: {TenantId}", tenant.Id);

        return Success();
    }
}
