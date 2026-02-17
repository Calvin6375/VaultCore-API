using FluentValidation;
using VaultCore.Application.DTOs;
using VaultCore.Domain.Enums;

namespace VaultCore.Application.Validators;

/// <summary>
/// Validates assign role request. Role must be Admin, Customer, or Support.
/// </summary>
public class AssignRoleRequestValidator : AbstractValidator<AssignRoleRequest>
{
    private static readonly string[] ValidRoles = Enum.GetNames(typeof(RoleType));

    public AssignRoleRequestValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty()
            .Must(name => ValidRoles.Contains(name, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Role must be one of: {string.Join(", ", ValidRoles)}");
    }
}
