using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using WoWInsight.Mobile.Services;
using WoWInsight.Mobile.Views;

namespace WoWInsight.Mobile;

public partial class App : Application
{
    private readonly IAuthService _authService;

    public App(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        MainPage = new AppShell();
    }

    protected override async void OnAppLinkRequestReceived(Uri uri)
    {
        base.OnAppLinkRequestReceived(uri);

        if (uri.Scheme.Equals("wowinsight", StringComparison.OrdinalIgnoreCase) &&
            uri.Host.Equals("auth", StringComparison.OrdinalIgnoreCase))
        {
            var token = ExtractToken(uri);
            if (!string.IsNullOrEmpty(token))
            {
                await _authService.SaveTokenAsync(token);
                // Navigate to main page
                await Shell.Current.GoToAsync("//main/characters");
            }
        }
    }

    private string? ExtractToken(Uri uri)
    {
        var query = uri.Query;
        if (string.IsNullOrEmpty(query)) return null;

        if (query.StartsWith("?")) query = query.Substring(1);

        var pairs = query.Split('&');
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=');
            if (parts.Length == 2 && parts[0] == "token")
            {
                return Uri.UnescapeDataString(parts[1]);
            }
        }
        return null;
    }
}
