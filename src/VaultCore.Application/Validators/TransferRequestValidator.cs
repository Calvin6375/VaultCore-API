using FluentValidation;
using VaultCore.Application.DTOs;

namespace VaultCore.Application.Validators;

/// <summary>
/// Validates transfer request.
/// </summary>
public class TransferRequestValidator : AbstractValidator<TransferRequest>
{
    public TransferRequestValidator()
    {
        RuleFor(x => x.ToUserId).NotEmpty().WithMessage("Recipient user is required");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be positive");
        RuleFor(x => x.Reference).MaximumLength(100).When(x => x.Reference != null);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description != null);
    }
}
