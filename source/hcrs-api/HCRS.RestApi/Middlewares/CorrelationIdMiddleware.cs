using HCRS.RestApi.Extensions;
using Medo;
using Serilog.Context;

namespace HCRS.RestApi.Middlewares;

public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault() ?? Uuid7.NewUuid7().ToString();
        context.SetCorrelationId(correlationId);
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] = correlationId;
                return Task.CompletedTask;
            });
            await _next(context);
        }
    }
}
