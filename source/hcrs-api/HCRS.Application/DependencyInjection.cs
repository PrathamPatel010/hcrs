using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HCRS.Application;

public static class DependencyInjection
{
    public static void AddApplicationLayer(this IServiceCollection services,IConfiguration configuration)
    {
        // Application layer sevices will be here
    }
}
