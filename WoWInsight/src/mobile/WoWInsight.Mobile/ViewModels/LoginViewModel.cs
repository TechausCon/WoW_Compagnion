using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using WoWInsight.Mobile.Services;
using WoWInsight.Mobile.Views;

namespace WoWInsight.Mobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly BackendApiClient _apiClient;

    public LoginViewModel(BackendApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        try
        {
            var url = _apiClient.GetAuthUrl();
            await Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Unable to open browser: {ex.Message}", "OK");
        }
    }

    public async Task CheckLoginStatusAsync()
    {
        var token = await _apiClient.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            // Already logged in
            await Shell.Current.GoToAsync("//main/characters");
        }
    }
}
