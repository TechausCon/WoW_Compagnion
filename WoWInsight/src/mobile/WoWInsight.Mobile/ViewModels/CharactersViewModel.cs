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
    private readonly LocalDbService _localDb;
    private readonly SyncService _syncService;

    [ObservableProperty]
    ObservableCollection<Character> characters = new();

    [ObservableProperty]
    bool isRefreshing;

    public CharactersViewModel(LocalDbService localDb, SyncService syncService)
    {
        _localDb = localDb;
        _syncService = syncService;
    }

    public async Task InitializeAsync()
    {
        await LoadLocalDataAsync();
        // Trigger background sync if empty or just always?
        // Prompt says "Background Refresh".
        // Let's trigger refresh on init.
        IsRefreshing = true;
        await RefreshAsync();
        IsRefreshing = false;
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
            await _syncService.SyncCharactersAsync();
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
        await Shell.Current.GoToAsync($"{nameof(CharacterDetailPage)}", navigationParameter);
    }
}
