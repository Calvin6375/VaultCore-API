using FluentValidation;
using VaultCore.Application.DTOs.Auth;

namespace VaultCore.Application.Validators;

/// <summary>
/// Validates login request.
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
