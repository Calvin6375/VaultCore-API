namespace VaultCore.API.Middleware;

/// <summary>
/// Ensures each request has a correlation ID for tracing.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var correlationId) || string.IsNullOrEmpty(correlationId))
            context.TraceIdentifier = correlationId = Guid.NewGuid().ToString("N");
        context.Response.Headers[HeaderName] = correlationId!;
        await _next(context);
    }
}
