using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HCRS.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureLayer(this IServiceCollection services,IConfiguration configuration)
    {
        services.ConfigureDatabase(configuration);
    }

    public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options=>
        {
            options.UseSqlServer(configuration[SettingsConstants.DB_CONNECTION_STRING]);
        });
    }

}
