using HCRS.Application;
using HCRS.Infrastructure;
using HCRS.RestApi.Extensions;
using HCRS.RestApi.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

try
{
    // Add Services and Dependencies
    builder.ConfigureSerilogLogging();
    builder.Services.AddApplicationLayer(builder.Configuration);
    builder.Services.AddInfrastructureLayer(builder.Configuration);
    builder.ConfigureSwagger();
    builder.ConfigureControllers();


    var app = builder.Build();

    Log.Information("Starting HCRS API");
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseSerilogLogging();
    app.UseMiddleware<ErrorHandlingMiddleware>();
    if (app.Environment.IsDevelopment())
    {
        await app.MigrateDatabase();
        await app.SeedDatabaseAsync();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (HostAbortedException)
{
    // Ignore EF Core design-time host abort
}
catch (Exception ex)
{
    Log.Fatal(ex, "HCRS API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}