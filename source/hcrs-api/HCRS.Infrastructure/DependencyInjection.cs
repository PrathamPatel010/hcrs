using HCRS.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HCRS.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureLayer(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddDatabaseContexts(configuration);
    }

    public static void AddDatabaseContexts(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<HcrsDbContext>(options=>
        {
            options.UseSqlServer(configuration[SettingsConstants.DB_CONNECTION_STRING]);
        });
        services.AddScoped<DapperContext>();
    }
}
