using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WoWInsight.Mobile.Models;
using WoWInsight.Mobile.Services;

namespace WoWInsight.Mobile.ViewModels;

public partial class WeeklyChecklistViewModel : ObservableObject
{
    private readonly LocalDbService _localDb;

    [ObservableProperty]
    ObservableCollection<CharacterChecklist> characterChecklists = new();

    public WeeklyChecklistViewModel(LocalDbService localDb)
    {
        _localDb = localDb;
    }

    public async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var chars = await _localDb.GetCharactersAsync();
        CharacterChecklists.Clear();
        foreach (var c in chars)
        {
            var checklist = await _localDb.GetWeeklyChecklistAsync(c.CharacterKey);
            CharacterChecklists.Add(new CharacterChecklist(c, checklist, _localDb));
        }
    }
}

public partial class CharacterChecklist : ObservableObject
{
    private readonly LocalDbService _localDb;
    public Character Character { get; }
    public WeeklyChecklist Checklist { get; }

    public CharacterChecklist(Character character, WeeklyChecklist checklist, LocalDbService localDb)
    {
        Character = character;
        Checklist = checklist;
        _localDb = localDb;
    }

    public bool IsWeeklyChestDone
    {
        get => Checklist.IsWeeklyChestDone;
        set
        {
            if (Checklist.IsWeeklyChestDone != value)
            {
                Checklist.IsWeeklyChestDone = value;
                OnPropertyChanged();
                SaveAsync();
            }
        }
    }

    public bool IsRaidDone
    {
        get => Checklist.IsRaidDone;
        set
        {
            if (Checklist.IsRaidDone != value)
            {
                Checklist.IsRaidDone = value;
                OnPropertyChanged();
                SaveAsync();
            }
        }
    }

    public bool IsWorldBossDone
    {
        get => Checklist.IsWorldBossDone;
        set
        {
            if (Checklist.IsWorldBossDone != value)
            {
                Checklist.IsWorldBossDone = value;
                OnPropertyChanged();
                SaveAsync();
            }
        }
    }

    private async void SaveAsync()
    {
        await _localDb.SaveWeeklyChecklistAsync(Checklist);
    }
}
