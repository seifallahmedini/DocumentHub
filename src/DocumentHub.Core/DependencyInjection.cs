using DocumentHub.Core.UseCases.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentHub.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<RegisterHandler>();
        services.AddScoped<LoginHandler>();
        return services;
    }
}
