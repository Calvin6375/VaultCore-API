using FluentValidation;
using VaultCore.Application.DTOs;

namespace VaultCore.Application.Validators;

/// <summary>
/// Validates deposit request.
/// </summary>
public class DepositRequestValidator : AbstractValidator<DepositRequest>
{
    public DepositRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be positive");
        RuleFor(x => x.Reference).MaximumLength(100).When(x => x.Reference != null);
    }
}
