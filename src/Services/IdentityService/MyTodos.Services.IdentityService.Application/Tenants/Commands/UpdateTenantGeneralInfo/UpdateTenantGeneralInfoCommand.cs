using FluentValidation;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.BuildingBlocks.Application.Contracts.Security;
using MyTodos.Services.IdentityService.Application.Tenants.Contracts;
using MyTodos.Services.IdentityService.Domain.TenantAggregate.Constants;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Tenants.Commands.UpdateTenantGeneralInfo;

/// <summary>
/// Command to update a tenant's general information (name, etc.).
/// This follows CQRS principles - it updates general tenant information fields.
/// </summary>
public sealed class UpdateTenantGeneralInfoCommand : Command
{
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;

    public UpdateTenantGeneralInfoCommand(Guid tenantId, string name)
    {
        TenantId = tenantId;
        Name = name;
    }

    // Parameterless constructor for model binding
    public UpdateTenantGeneralInfoCommand()
    {
    }
}

/// <summary>
/// Validator for UpdateTenantGeneralInfoCommand.
/// </summary>
public sealed class UpdateTenantGeneralInfoCommandValidator : AbstractValidator<UpdateTenantGeneralInfoCommand>
{
    public UpdateTenantGeneralInfoCommandValidator()
    {
        // TenantId is optional in the body - it will be set from the route parameter in the controller
        // Only validate if it's provided to ensure it's not an empty GUID
        When(x => x.TenantId != Guid.Empty, () =>
        {
            RuleFor(x => x.TenantId)
                .NotEmpty()
                .WithMessage("Tenant ID is required");
        });

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(TenantConstants.ErrorMessages.NameRequired)
            .MinimumLength(2)
            .WithMessage("Tenant name must be at least 2 characters")
            .MaximumLength(TenantConstants.FieldLengths.NameMaxLength)
            .WithMessage(string.Format(TenantConstants.ErrorMessages.NameTooLong,
                TenantConstants.FieldLengths.NameMaxLength));
    }
}

/// <summary>
/// Handler for updating a tenant's general information.
/// </summary>
public sealed class UpdateTenantGeneralInfoCommandHandler : CommandHandler<UpdateTenantGeneralInfoCommand>
{
    private readonly ITenantWriteRepository _tenantWriteRepository;
    private readonly ITenantPagedListReadRepository _tenantReadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateTenantGeneralInfoCommandHandler> _logger;

    private readonly ICurrentUserService _currentUserService;

    public UpdateTenantGeneralInfoCommandHandler(
        ITenantWriteRepository tenantWriteRepository,
        ITenantPagedListReadRepository tenantReadRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<UpdateTenantGeneralInfoCommandHandler> logger)
    {
        _tenantWriteRepository = tenantWriteRepository;
        _tenantReadRepository = tenantReadRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override async Task<Result> Handle(
        UpdateTenantGeneralInfoCommand request,
        CancellationToken ct)
    {
        // Only Global Administrators can update tenant information
        if (!_currentUserService.IsGlobalAdmin())
        {
            _logger.LogWarning("Tenant update failed: User is not a Global Administrator");
            return Forbidden("Only Global Administrators can update tenant information");
        }

        // Check if tenant exists
        var tenant = await _tenantReadRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
        {
            _logger.LogWarning("Tenant general info update failed: Tenant with ID {TenantId} not found", request.TenantId);
            return NotFound($"Tenant with ID '{request.TenantId}' not found");
        }

        // Check if another tenant with the same name already exists
        var existingTenant = await _tenantReadRepository.GetByNameAsync(request.Name, ct);
        if (existingTenant != null && existingTenant.Id != request.TenantId)
        {
            _logger.LogWarning("Tenant general info update failed: Tenant with name {Name} already exists", request.Name);
            return Conflict($"A tenant with the name '{request.Name}' already exists");
        }

        // Update tenant general info
        var updateResult = tenant.UpdateGeneralInfo(request.Name);
        if (updateResult.IsFailure)
        {
            _logger.LogWarning("Tenant general info update failed: {Error}", updateResult.FirstError.Description);
            return Failure(updateResult.FirstError.Type, updateResult.FirstError.Description);
        }

        await _tenantWriteRepository.UpdateAsync(tenant, ct);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("Tenant general info updated: {TenantId} - new name: {Name}", tenant.Id, tenant.Name);

        return Success();
    }
}
