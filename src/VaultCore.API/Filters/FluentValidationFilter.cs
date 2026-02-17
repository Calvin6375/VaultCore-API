using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VaultCore.API.Filters;

/// <summary>
/// Runs FluentValidation for action arguments and returns 400 if invalid.
/// </summary>
public class FluentValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg == null) continue;
            var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
            var validator = context.HttpContext.RequestServices.GetService(validatorType);
            if (validator == null) continue;
            // Use dynamic to call ValidateAsync<T>(ValidationContext<T>, CancellationToken)
            var result = await ValidateAsync(validator, arg, context.HttpContext.RequestAborted);
            if (result == null || result.IsValid) continue;
            var errors = result.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            context.Result = new BadRequestObjectResult(new { errors });
            return;
        }
        await next();
    }

    private static Task<FluentValidation.Results.ValidationResult> ValidateAsync(object validator, object instance, CancellationToken cancellationToken)
    {
        var contextType = typeof(ValidationContext<>).MakeGenericType(instance.GetType());
        var context = Activator.CreateInstance(contextType, instance)!;
        var method = validator.GetType().GetMethod("ValidateAsync", new[] { contextType, typeof(CancellationToken) });
        if (method == null) return Task.FromResult(new FluentValidation.Results.ValidationResult());
        var task = method.Invoke(validator, new[] { context, cancellationToken }) as Task<FluentValidation.Results.ValidationResult>;
        return task ?? Task.FromResult(new FluentValidation.Results.ValidationResult());
    }
}
