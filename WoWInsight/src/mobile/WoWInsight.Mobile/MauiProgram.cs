using Microsoft.Extensions.Logging;
using WoWInsight.Mobile.Services;
using WoWInsight.Mobile.ViewModels;
using WoWInsight.Mobile.Views;

namespace WoWInsight.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("LifeCraft_Font.ttf", "LifeCraft");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Config & Auth
        builder.Services.AddSingleton<IAppConfig, AppConfig>();
        builder.Services.AddSingleton<ISecureStorageService, SecureStorageService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();

        // Services
        builder.Services.AddSingleton<ILocalDbService, LocalDbService>();

        // HttpClient registration
        builder.Services.AddHttpClient<IBackendApiClient, BackendApiClient>((serviceProvider, client) =>
        {
            var config = serviceProvider.GetRequiredService<IAppConfig>();
            client.BaseAddress = new Uri(config.ApiBaseUrl);
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            // Ignore SSL errors for development (localhost self-signed)
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            return handler;
        });

        builder.Services.AddSingleton<ISyncService, SyncService>();
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IBrowserService, BrowserService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<CharactersViewModel>();
        builder.Services.AddTransient<CharacterDetailViewModel>();
        builder.Services.AddTransient<WeeklyChecklistViewModel>();

        // Views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<CharactersPage>();
        builder.Services.AddTransient<CharacterDetailPage>();
        builder.Services.AddTransient<WeeklyChecklistPage>();

        return builder.Build();
    }
}
