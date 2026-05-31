namespace HCRS.RestApi.Extensions;

public static class HttpContextExtension
{
    public static string? GetCorrelationId (this HttpContext context)
    {
        return context.Items[Middlewares.CorrelationIdMiddleware.HeaderName]?.ToString();
    }

    public static void SetCorrelationId (this HttpContext context, string correlationId)
    {
        context.Items[Middlewares.CorrelationIdMiddleware.HeaderName] = correlationId;
    }
}
