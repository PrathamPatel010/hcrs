using Microsoft.EntityFrameworkCore;

namespace HCRS.RestApi.Extensions;

public static class MigrationExtension
{
    public async static Task MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<Program>>();
        try
        {
            logger.LogInformation("Checking for pending database migrations...");

            var context = services.GetRequiredService<HcrsDbContext>();

            var pendingMigrations = context.Database.GetPendingMigrations();

            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending migrations...",pendingMigrations.Count());
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully.");
            }
            else
            {
                logger.LogInformation("Database is already up to date.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations.");
            throw;
        }
    }
}