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
    private readonly IBackendApiClient _apiClient;
    private readonly IAuthService _authService;

    public LoginViewModel(IBackendApiClient apiClient, IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
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
        if (await _authService.GetTokenAsync() != null)
        {
            // Already logged in
            // Use GoToAsync to navigate to main page
            await Shell.Current.GoToAsync("//main/characters");
        }
    }
}
