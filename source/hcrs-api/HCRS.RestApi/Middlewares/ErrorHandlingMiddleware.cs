using HCRS.RestApi.Contracts;
using HCRS.RestApi.Extensions;
using Serilog;

namespace HCRS.RestApi.Middlewares;

public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.GetCorrelationId();

            Log.Error(ex,"Unhandled exception. CorrelationId: {CorrelationId}", correlationId);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(
            new ApiResponse
            {
                IsSuccess = false,
                Message = "An unexpected error occurred.",
                CorrelationId = correlationId
            });
        }
    }
}