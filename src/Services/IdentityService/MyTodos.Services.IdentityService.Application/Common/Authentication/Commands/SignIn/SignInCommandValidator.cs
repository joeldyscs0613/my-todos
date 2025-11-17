using FluentValidation;

namespace MyTodos.Services.IdentityService.Application.Features.Authentication.Commands.SignIn;

/// <summary>
/// Validator for SignInCommand.
/// </summary>
public sealed class SignInCommandValidator : AbstractValidator<SignInCommand>
{
    public SignInCommandValidator()
    {
        RuleFor(x => x.UsernameOrEmail)
            .NotEmpty()
            .WithMessage("Username or email is required");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters");
    }
}
