using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VaultCore.Application.Services;

namespace VaultCore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(Mapping.MappingProfile).Assembly);
        services.AddValidatorsFromAssemblyContaining<Validators.RegisterRequestValidator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IAuditQueryService, AuditQueryService>();
        return services;
    }
}
