using Microsoft.Extensions.DependencyInjection;
using WoWInsight.Application.Interfaces;
using WoWInsight.Application.Services;

namespace WoWInsight.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICharacterService, CharacterService>();
        // IJwtService implementation will be in Infrastructure? Or Application?
        // Usually implementation details like JWT generation using libraries are Infrastructure.
        // But logic can be Application.
        // The prompt says "Api: ... DI wiring".
        // I'll register Application services here.
        // Interface implementations from Infrastructure are registered in Infrastructure.

        return services;
    }
}
