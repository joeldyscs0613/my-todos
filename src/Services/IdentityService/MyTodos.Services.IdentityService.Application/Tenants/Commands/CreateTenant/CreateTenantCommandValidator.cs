using FluentValidation;
using MyTodos.Services.IdentityService.Domain.TenantAggregate.Enums;

namespace MyTodos.Services.IdentityService.Application.Features.Tenants.Commands.CreateTenant;

/// <summary>
/// Validator for CreateTenantCommand.
/// </summary>
public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tenant name is required")
            .MinimumLength(2)
            .WithMessage("Tenant name must be at least 2 characters")
            .MaximumLength(200)
            .WithMessage("Tenant name must not exceed 200 characters");

        RuleFor(x => x.Plan)
            .IsInEnum()
            .WithMessage("Invalid tenant plan");

        RuleFor(x => x.MaxUsers)
            .GreaterThan(0)
            .WithMessage("Max users must be greater than 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("Max users cannot exceed 10000");
    }
}
