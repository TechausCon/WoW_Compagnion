using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using WoWInsight.Mobile.Services;

namespace WoWInsight.Mobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IBackendApiClient _apiClient;
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IBrowserService _browserService;

    public LoginViewModel(IBackendApiClient apiClient, IAuthService authService, INavigationService navigationService, IDialogService dialogService, IBrowserService browserService)
    {
        _apiClient = apiClient;
        _authService = authService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _browserService = browserService;
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        try
        {
            var url = _apiClient.GetAuthUrl();
            await _browserService.OpenAsync(url);
        }
        catch (Exception ex)
        {
            await _dialogService.DisplayAlertAsync("Error", $"Unable to open browser: {ex.Message}", "OK");
        }
    }

    public async Task CheckLoginStatusAsync()
    {
        if (await _authService.GetTokenAsync() != null)
        {
            // Already logged in
            await _navigationService.GoToAsync("//main/characters");
        }
    }
}
