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
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Services
        builder.Services.AddSingleton<LocalDbService>();
        builder.Services.AddSingleton<BackendApiClient>();
        builder.Services.AddSingleton<SyncService>();

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
