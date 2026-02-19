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
            var token = ExtractParam(uri, "token");
            var refreshToken = ExtractParam(uri, "refreshToken");

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(refreshToken))
            {
                await _authService.SaveTokensAsync(token, refreshToken);
                // Navigate to main page
                await Shell.Current.GoToAsync("//main/characters");
            }
        }
    }

    private string? ExtractParam(Uri uri, string key)
    {
        var query = uri.Query;
        if (string.IsNullOrEmpty(query)) return null;

        if (query.StartsWith("?")) query = query.Substring(1);

        var pairs = query.Split('&');
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=');
            if (parts.Length == 2 && parts[0] == key)
            {
                return Uri.UnescapeDataString(parts[1]);
            }
        }
        return null;
    }
}
