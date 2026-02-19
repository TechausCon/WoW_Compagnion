using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using WoWInsight.Mobile.Services;
using WoWInsight.Mobile.Views;

namespace WoWInsight.Mobile;

public partial class App : Application
{
    private readonly BackendApiClient _apiClient;

    public App(BackendApiClient apiClient)
    {
        InitializeComponent();
        _apiClient = apiClient;
        MainPage = new AppShell();
    }

    protected override async void OnAppLinkRequestReceived(Uri uri)
    {
        base.OnAppLinkRequestReceived(uri);

        if (uri.Scheme.Equals("wowinsight", StringComparison.OrdinalIgnoreCase) &&
            uri.Host.Equals("auth", StringComparison.OrdinalIgnoreCase))
        {
            // Parse token from query
            // System.Web.HttpUtility is not available in standard net10.0-android/ios without reference.
            // Use custom parsing or Microsoft.AspNetCore.WebUtilities if available, or simple string split.
            var query = uri.Query;
            if (query.StartsWith("?")) query = query.Substring(1);

            var pairs = query.Split('&');
            foreach (var pair in pairs)
            {
                var parts = pair.Split('=');
                if (parts.Length == 2 && parts[0] == "token")
                {
                    var token = parts[1];
                    if (!string.IsNullOrEmpty(token))
                    {
                        await _apiClient.SaveTokenAsync(token);
                        await Shell.Current.GoToAsync("//main/characters");
                    }
                    break;
                }
            }
        }
    }
}
