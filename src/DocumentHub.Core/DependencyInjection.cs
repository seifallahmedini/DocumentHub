using DocumentHub.Core.UseCases.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentHub.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationHandlers(this IServiceCollection services)
    {
        services.AddScoped<RegisterHandler>();
        return services;
    }
}
