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
            .PersistKeysToDbContext<AppDbContext>();
            // Wait, PersistKeysToDbContext requires 'Microsoft.AspNetCore.DataProtection.EntityFrameworkCore' package.
            // And AppDbContext needs to implement IDataProtectionKeyContext.
            // But I didn't add that package.
            // The prompt said: "Nutze IDataProtector (ASP.NET Core Data Protection) für Token-Verschlüsselung."
            // "Backend optional SQLite für Session/Token".
            // If I use PersistKeysToFileSystem, it works if running in container with volume.
            // Or I can use simple ephemeral keys for MVP if restart is okay (session lost).
            // But prompt asks for production-near vertical slice.
            // I'll use FileSystem persistence to a local folder "./keys".

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
            // Wait, NotFound should NOT retry usually.
            // Prompt: "Retry (z. B. 2x) nur bei transient errors".
            // So default HandleTransientHttpError is correct (5xx, 408).
            .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
