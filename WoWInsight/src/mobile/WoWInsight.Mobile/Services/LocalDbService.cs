using SQLite;
using System.Collections.Generic;
using System.IO;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;
using WoWInsight.Mobile.Models;

namespace WoWInsight.Mobile.Services;

public class LocalDbService
{
    private SQLiteAsyncConnection _database;

    public LocalDbService()
    {
    }

    private async Task Init()
    {
        if (_database != null)
            return;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "WoWInsight.db");
        _database = new SQLiteAsyncConnection(dbPath);

        await _database.CreateTableAsync<Character>();
        await _database.CreateTableAsync<MythicPlusSummary>();
        await _database.CreateTableAsync<WeeklyChecklist>();
    }

    public async Task<List<Character>> GetCharactersAsync()
    {
        await Init();
        return await _database.Table<Character>().ToListAsync();
    }

    public async Task SaveCharactersAsync(IEnumerable<Character> characters)
    {
        await Init();
        // Upsert
        foreach (var c in characters)
        {
            await _database.InsertOrReplaceAsync(c);
        }
        // Should we delete characters not in the list?
        // For sync, yes. But let's keep it simple: upsert.
        // If user deletes char on blizzard side, it remains here until we purge.
        // I'll leave as upsert.
    }

    public async Task<Character?> GetCharacterAsync(string key)
    {
        await Init();
        return await _database.Table<Character>().FirstOrDefaultAsync(c => c.CharacterKey == key);
    }

    public async Task<MythicPlusSummary?> GetMythicPlusSummaryAsync(string characterKey)
    {
        await Init();
        return await _database.Table<MythicPlusSummary>().FirstOrDefaultAsync(s => s.CharacterKey == characterKey);
    }

    public async Task SaveMythicPlusSummaryAsync(MythicPlusSummary summary)
    {
        await Init();
        await _database.InsertOrReplaceAsync(summary);
    }

    public async Task<WeeklyChecklist> GetWeeklyChecklistAsync(string characterKey)
    {
        await Init();
        var item = await _database.Table<WeeklyChecklist>().FirstOrDefaultAsync(c => c.CharacterKey == characterKey);
        if (item == null)
        {
            item = new WeeklyChecklist { CharacterKey = characterKey };
            await _database.InsertAsync(item);
        }
        return item;
    }

    public async Task SaveWeeklyChecklistAsync(WeeklyChecklist checklist)
    {
        await Init();
        checklist.LastUpdated = System.DateTimeOffset.UtcNow;
        await _database.InsertOrReplaceAsync(checklist); // Update doesn't work if ID is 0 and existing. But here we fetch first.
        // Actually InsertOrReplace works based on PK.
        // But Checklist has ID PK. CharacterKey is just a field.
        // If I fetch by Key, I get the ID.
        // So Update is fine.
        await _database.UpdateAsync(checklist);
    }
}
