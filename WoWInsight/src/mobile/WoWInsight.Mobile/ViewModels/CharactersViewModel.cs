using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WoWInsight.Mobile.Models;
using WoWInsight.Mobile.Services;
using WoWInsight.Mobile.Views;

namespace WoWInsight.Mobile.ViewModels;

public partial class CharactersViewModel : ObservableObject
{
    private readonly ILocalDbService _localDb;
    private readonly ISyncService _syncService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    ObservableCollection<Character> characters = new();

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    bool isBusy;

    public CharactersViewModel(ILocalDbService localDb, ISyncService syncService, IDialogService dialogService, INavigationService navigationService)
    {
        _localDb = localDb;
        _syncService = syncService;
        _dialogService = dialogService;
        _navigationService = navigationService;
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            await LoadLocalDataAsync();
            if (Characters.Count == 0)
            {
                await RefreshAsync();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadLocalDataAsync()
    {
        var list = await _localDb.GetCharactersAsync();
        Characters.Clear();
        foreach (var c in list)
        {
            Characters.Add(c);
        }
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsRefreshing = true;
        try
        {
            var success = await _syncService.SyncCharactersAsync();
            if (!success)
            {
                await _dialogService.DisplayAlertAsync("Sync Error", "Failed to synchronize characters. Please check your connection.", "OK");
            }
            await LoadLocalDataAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    public async Task GoToDetailAsync(Character character)
    {
        if (character == null) return;

        var navigationParameter = new Dictionary<string, object>
        {
            { "Character", character }
        };
        await _navigationService.GoToAsync($"{nameof(CharacterDetailPage)}", navigationParameter);
    }
}
