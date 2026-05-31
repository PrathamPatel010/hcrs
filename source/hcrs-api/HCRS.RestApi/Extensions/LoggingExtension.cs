using Serilog;
namespace HCRS.RestApi.Extensions;

public static class LoggingExtension
{
    public static void ConfigureSerilogLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();   
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();
        builder.Host.UseSerilog();
    }

    public static void UseSerilogLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("CorrelationId",httpContext.GetCorrelationId());
                };
        });
    }
}
