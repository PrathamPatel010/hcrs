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
}
