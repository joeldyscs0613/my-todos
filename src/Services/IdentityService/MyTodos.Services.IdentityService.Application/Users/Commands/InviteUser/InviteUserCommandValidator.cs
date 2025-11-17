using FluentValidation;

namespace MyTodos.Services.IdentityService.Application.Users.Commands.InviteUser;

/// <summary>
/// Validator for InviteUserCommand.
/// </summary>
public sealed class InviteUserCommandValidator : AbstractValidator<InviteUserCommand>
{
    public InviteUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email address")
            .MaximumLength(256)
            .WithMessage("Email must not exceed 256 characters");

        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("Role is required");

        // TenantId is optional (for global roles)
    }
}
