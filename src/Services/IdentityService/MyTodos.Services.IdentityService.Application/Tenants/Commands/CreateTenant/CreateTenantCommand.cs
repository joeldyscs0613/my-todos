using FluentValidation;
using Microsoft.Extensions.Logging;
using MyTodos.BuildingBlocks.Application.Abstractions.Commands;
using MyTodos.BuildingBlocks.Application.Abstractions.Dtos;
using MyTodos.BuildingBlocks.Application.Contracts.Persistence;
using MyTodos.Services.IdentityService.Application.Tenants.Contracts;
using MyTodos.Services.IdentityService.Domain.TenantAggregate;
using MyTodos.Services.IdentityService.Domain.TenantAggregate.Constants;
using MyTodos.Services.IdentityService.Domain.TenantAggregate.Enums;
using MyTodos.SharedKernel.Helpers;

namespace MyTodos.Services.IdentityService.Application.Tenants.Commands.CreateTenant;

/// <summary>
/// Command to create a new tenant (Global Admin only).
/// </summary>
public sealed class CreateTenantCommand : CreateCommand<Guid>
{
    public string Name { get; init; } = string.Empty;
    public TenantPlan Plan { get; init; } = TenantPlan.Free;
    public int MaxUsers { get; init; } = 5;
}

/// <summary>
/// Validator for CreateTenantCommand.
/// </summary>
public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(TenantConstants.ErrorMessages.NameRequired)
            .MinimumLength(2)
            .WithMessage("Tenant name must be at least 2 characters")
            .MaximumLength(TenantConstants.FieldLengths.NameMaxLength)
            .WithMessage(string.Format(TenantConstants.ErrorMessages.NameTooLong, TenantConstants.FieldLengths.NameMaxLength));

        RuleFor(x => x.Plan)
            .IsInEnum()
            .WithMessage("Invalid tenant plan");

        RuleFor(x => x.MaxUsers)
            .GreaterThan(0)
            .WithMessage("Max users must be greater than 0")
            .LessThanOrEqualTo(TenantConstants.Invariants.MaxNumberOfUsersAllowed)
            .WithMessage($"Max users cannot exceed {TenantConstants.Invariants.MaxNumberOfUsersAllowed}");
    }
}

/// <summary>
/// Handler for creating a new tenant.
/// </summary>
public sealed class CreateTenantCommandHandler : CreateCommandHandler<CreateTenantCommand, Guid>
{
    private readonly ITenantPagedListReadRepository _tenantPagedListReadRepository;
    private readonly ITenantWriteRepository _tenantWriteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateTenantCommandHandler> _logger;

    public CreateTenantCommandHandler(
        ITenantPagedListReadRepository tenantPagedListReadRepository,
        ITenantWriteRepository tenantWriteRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateTenantCommandHandler> logger)
    {
        _tenantPagedListReadRepository = tenantPagedListReadRepository;
        _tenantWriteRepository = tenantWriteRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<Result<CreateCommandResponseDto<Guid>>> Handle(
        CreateTenantCommand request,
        CancellationToken ct)
    {
        // Check if tenant with same name already exists
        var existingTenant = await _tenantPagedListReadRepository.GetByNameAsync(request.Name, ct);
        if (existingTenant != null)
        {
            _logger.LogWarning("Tenant creation failed: Tenant with name {Name} already exists", request.Name);
            return Conflict($"A tenant with the name '{request.Name}' already exists");
        }

        // Create tenant
        var tenantResult = Tenant.Create(
            request.Name,
            request.Plan,
            request.MaxUsers
        );

        if (tenantResult.IsFailure)
        {
            _logger.LogWarning("Tenant creation failed: {Error}", tenantResult.FirstError.Description);
            return Failure(tenantResult.FirstError);
        }

        var tenant = tenantResult.Value!;

        await _tenantWriteRepository.AddAsync(tenant, ct);
        await _unitOfWork.CommitAsync(ct);

        _logger.LogInformation("Tenant created: {TenantId} with name {Name}", tenant.Id, tenant.Name);

        return Success(tenant.Id);
    }
}
