using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Abstractions.Dtos;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.IdentityService.Domain.TenantAggregate;
using MyTodos.Services.IdentityService.Domain.TenantAggregate.Contracts;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Features.Tenants.Commands.CreateTenant;

/// <summary>
/// Handler for creating a new tenant.
/// </summary>
public sealed class CreateTenantCommandHandler : CreateCommandHandler<CreateTenantCommand, Guid>
{
    private readonly ITenantReadRepository _tenantReadRepository;
    private readonly ITenantWriteRepository _tenantWriteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateTenantCommandHandler> _logger;

    public CreateTenantCommandHandler(
        ITenantReadRepository tenantReadRepository,
        ITenantWriteRepository tenantWriteRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateTenantCommandHandler> logger)
    {
        _tenantReadRepository = tenantReadRepository;
        _tenantWriteRepository = tenantWriteRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<Result<CreateCommandResponseDto<Guid>>> Handle(
        CreateTenantCommand request,
        CancellationToken ct)
    {
        // Check if tenant with same name already exists
        var existingTenant = await _tenantReadRepository.GetByNameAsync(request.Name, ct);
        if (existingTenant != null)
        {
            _logger.LogWarning("Tenant creation failed: Tenant with name {Name} already exists", request.Name);
            return Conflict($"A tenant with the name '{request.Name}' already exists");
        }

        // Create tenant
        var tenant = Tenant.Create(
            request.Name,
            request.Plan,
            request.MaxUsers
        );

        await _tenantWriteRepository.AddAsync(tenant, ct);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("Tenant created: {TenantId} with name {Name}", tenant.Id, tenant.Name);

        return Success(tenant.Id);
    }
}
