using FluentValidation;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.IdentityService.Application.Tenants.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Tenants.Commands.DeleteTenant;

/// <summary>
/// Command to permanently delete a tenant from the database (Global Admin only).
/// This is a hard delete operation.
/// </summary>
public sealed class DeleteTenantCommand : Command
{
    public Guid TenantId { get; init; }

    public DeleteTenantCommand(Guid tenantId)
    {
        TenantId = tenantId;
    }
}

/// <summary>
/// Validator for DeleteTenantCommand.
/// </summary>
public sealed class DeleteTenantCommandValidator : AbstractValidator<DeleteTenantCommand>
{
    public DeleteTenantCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required");
    }
}

/// <summary>
/// Handler for permanently deleting a tenant.
/// </summary>
public sealed class DeleteTenantCommandHandler : CommandHandler<DeleteTenantCommand>
{
    private readonly ITenantWriteRepository _tenantWriteRepository;
    private readonly ITenantPagedListReadRepository _tenantReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteTenantCommandHandler> _logger;

    public DeleteTenantCommandHandler(
        ITenantWriteRepository tenantWriteRepository,
        ITenantPagedListReadRepository tenantReadRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteTenantCommandHandler> logger)
    {
        _tenantWriteRepository = tenantWriteRepository;
        _tenantReadRepository = tenantReadRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<Result> Handle(
        DeleteTenantCommand request,
        CancellationToken ct)
    {
        // Check if tenant exists
        var tenant = await _tenantReadRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
        {
            _logger.LogWarning("Tenant deletion failed: Tenant with ID {TenantId} not found", request.TenantId);
            return NotFound($"Tenant with ID '{request.TenantId}' not found");
        }

        // Call domain method to mark for deletion and raise domain event
        // This allows business logic to execute (e.g., delete users, clean up resources)
        tenant.Delete();

        // Perform hard delete
        await _tenantWriteRepository.DeleteAsync(tenant, ct);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("Tenant deleted: {TenantId} with name {Name}", tenant.Id, tenant.Name);

        return Success();
    }
}
