using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using WoWInsight.Mobile.DTOs;
using WoWInsight.Mobile.Models;
using WoWInsight.Mobile.Services;

namespace WoWInsight.Mobile.ViewModels;

[QueryProperty(nameof(Character), "Character")]
public partial class CharacterDetailViewModel : ObservableObject
{
    private readonly LocalDbService _localDb;
    private readonly SyncService _syncService;

    [ObservableProperty]
    Character character;

    [ObservableProperty]
    MythicPlusSummary summary;

    [ObservableProperty]
    List<RunDto> recentRuns = new();

    [ObservableProperty]
    List<RunDto> bestRuns = new();

    [ObservableProperty]
    bool isRefreshing;

    public CharacterDetailViewModel(LocalDbService localDb, SyncService syncService)
    {
        _localDb = localDb;
        _syncService = syncService;
    }

    async partial void OnCharacterChanged(Character value)
    {
        if (value != null)
        {
            await LoadSummaryAsync();
            // Trigger refresh
            IsRefreshing = true;
            await RefreshAsync();
            IsRefreshing = false;
        }
    }

    private async Task LoadSummaryAsync()
    {
        if (Character == null) return;

        var sum = await _localDb.GetMythicPlusSummaryAsync(Character.CharacterKey);
        if (sum != null)
        {
            Summary = sum;
            if (!string.IsNullOrEmpty(sum.RecentRunsJson))
            {
                RecentRuns = JsonSerializer.Deserialize<List<RunDto>>(sum.RecentRunsJson) ?? new();
            }
            if (!string.IsNullOrEmpty(sum.BestRunsJson))
            {
                BestRuns = JsonSerializer.Deserialize<List<RunDto>>(sum.BestRunsJson) ?? new();
            }
        }
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        if (Character == null) return;
        IsRefreshing = true;
        try
        {
            await _syncService.SyncMythicPlusAsync(Character.CharacterKey);
            await LoadSummaryAsync();
        }
        catch
        {
            // Ignore offline error
        }
        finally
        {
            IsRefreshing = false;
        }
    }
}
