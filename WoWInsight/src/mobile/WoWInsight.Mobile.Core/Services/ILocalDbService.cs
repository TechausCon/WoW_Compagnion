using System.Collections.Generic;
using System.Threading.Tasks;
using WoWInsight.Mobile.Models;

namespace WoWInsight.Mobile.Services;

public interface ILocalDbService
{
    Task<List<Character>> GetCharactersAsync();
    Task SaveCharactersAsync(IEnumerable<Character> characters);
    Task<Character?> GetCharacterAsync(string key);
    Task<MythicPlusSummary?> GetMythicPlusSummaryAsync(string characterKey);
    Task SaveMythicPlusSummaryAsync(MythicPlusSummary summary);
    Task<WeeklyChecklist> GetWeeklyChecklistAsync(string characterKey);
    Task SaveWeeklyChecklistAsync(WeeklyChecklist checklist);
}
