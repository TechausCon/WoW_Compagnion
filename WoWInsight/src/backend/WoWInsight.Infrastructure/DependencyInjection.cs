using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using WoWInsight.Application.Interfaces;
using WoWInsight.Infrastructure.Configuration;
using WoWInsight.Infrastructure.Persistence;
using WoWInsight.Infrastructure.Services;

namespace WoWInsight.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Persistence
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserAccountRepository, UserAccountRepository>();

        // Data Protection
        services.AddDataProtection()
            .PersistKeysToFileSystem(new System.IO.DirectoryInfo("./keys"))
            .SetApplicationName("WoWInsight");

        services.AddScoped<ITokenStore, TokenStore>();

        // Configuration
        services.Configure<BlizzardSettings>(configuration.GetSection(BlizzardSettings.SectionName));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Services
        services.AddScoped<IJwtService, JwtService>();

        // HTTP Clients with Polly
        services.AddHttpClient<IBlizzardService, BlizzardService>()
            .AddPolicyHandler(GetRetryPolicy());

        services.AddHttpClient<IRaiderIoService, RaiderIoService>()
            .AddPolicyHandler(GetRetryPolicy());

        return services;
    }

    private static IAsyncPolicy<System.Net.Http.HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
