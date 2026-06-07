using DocumentHub.Core.UseCases.Auth;
using DocumentHub.Core.UseCases.Users;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentHub.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<RegisterHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<InviteHandler>();
        services.AddScoped<RemoveMemberHandler>();
        services.AddScoped<GetMembersHandler>();
        return services;
    }
}
