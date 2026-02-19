using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using WoWInsight.Mobile.Models;

namespace WoWInsight.Mobile.Services;

public class LocalDbService : ILocalDbService
{
    private readonly AsyncLazy<SQLiteAsyncConnection> _database;

    public LocalDbService()
    {
        _database = new AsyncLazy<SQLiteAsyncConnection>(async () =>
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "WoWInsight.db");
            var db = new SQLiteAsyncConnection(dbPath);

            await db.CreateTableAsync<Character>();
            await db.CreateTableAsync<MythicPlusSummary>();
            await db.CreateTableAsync<WeeklyChecklist>();

            return db;
        });
    }

    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        return await _database.Value;
    }

    public async Task<List<Character>> GetCharactersAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<Character>().ToListAsync();
    }

    public async Task SaveCharactersAsync(IEnumerable<Character> characters)
    {
        var db = await GetConnectionAsync();
        await db.RunInTransactionAsync(tran =>
        {
            foreach (var c in characters)
            {
                tran.InsertOrReplace(c);
            }
        });
    }

    public async Task<Character?> GetCharacterAsync(string key)
    {
        var db = await GetConnectionAsync();
        return await db.Table<Character>().FirstOrDefaultAsync(c => c.CharacterKey == key);
    }

    public async Task<MythicPlusSummary?> GetMythicPlusSummaryAsync(string characterKey)
    {
        var db = await GetConnectionAsync();
        return await db.Table<MythicPlusSummary>().FirstOrDefaultAsync(s => s.CharacterKey == characterKey);
    }

    public async Task SaveMythicPlusSummaryAsync(MythicPlusSummary summary)
    {
        var db = await GetConnectionAsync();
        await db.InsertOrReplaceAsync(summary);
    }

    public async Task<WeeklyChecklist> GetWeeklyChecklistAsync(string characterKey)
    {
        var db = await GetConnectionAsync();
        var item = await db.Table<WeeklyChecklist>().FirstOrDefaultAsync(c => c.CharacterKey == characterKey);
        if (item == null)
        {
            item = new WeeklyChecklist { CharacterKey = characterKey };
            await db.InsertAsync(item);
        }
        return item;
    }

    public async Task SaveWeeklyChecklistAsync(WeeklyChecklist checklist)
    {
        var db = await GetConnectionAsync();
        checklist.LastUpdated = DateTimeOffset.UtcNow;
        await db.UpdateAsync(checklist);
    }
}

public class AsyncLazy<T> : Lazy<Task<T>>
{
    public AsyncLazy(Func<T> valueFactory) :
        base(() => Task.Factory.StartNew(valueFactory)) { }

    public AsyncLazy(Func<Task<T>> taskFactory) :
        base(() => Task.Factory.StartNew(taskFactory).Unwrap()) { }
}
