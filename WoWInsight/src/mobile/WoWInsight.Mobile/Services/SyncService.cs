using System;
using System.Text.Json;
using System.Threading.Tasks;
using WoWInsight.Mobile.DTOs;
using WoWInsight.Mobile.Models;

namespace WoWInsight.Mobile.Services;

public class SyncService
{
    private readonly BackendApiClient _apiClient;
    private readonly LocalDbService _localDb;

    public SyncService(BackendApiClient apiClient, LocalDbService localDb)
    {
        _apiClient = apiClient;
        _localDb = localDb;
    }

    public async Task SyncCharactersAsync()
    {
        try
        {
            var dtos = await _apiClient.GetCharactersAsync();
            var entities = new System.Collections.Generic.List<Character>();

            foreach (var dto in dtos)
            {
                entities.Add(new Character
                {
                    CharacterKey = dto.CharacterKey,
                    Name = dto.Name,
                    Realm = dto.Realm,
                    Region = dto.Region,
                    Level = dto.Level,
                    Class = dto.Class,
                    Faction = dto.Faction
                });
            }

            await _localDb.SaveCharactersAsync(entities);

            // Trigger sync for each char summary? Or separate?
            // Prompt: "Danach Background Refresh: /me/characters -> upsert ... pro Charakter ... -> upsert"
            // So we should iterate and sync M+ summary too.
            foreach (var c in entities)
            {
                // Fire and forget or sequential?
                // Sequential to avoid slamming backend/raider.io rate limits if any (though backend caches).
                try
                {
                    await SyncMythicPlusAsync(c.CharacterKey);
                }
                catch
                {
                    // Ignore individual failures
                }
            }
        }
        catch (Exception)
        {
            // Sync failed (offline?)
            // We just stop. Local DB has old data.
        }
    }

    public async Task SyncMythicPlusAsync(string characterKey)
    {
        var dto = await _apiClient.GetMythicPlusSummaryAsync(characterKey);

        var entity = new MythicPlusSummary
        {
            CharacterKey = characterKey,
            TotalScore = dto.TotalScore,
            BestRunsJson = JsonSerializer.Serialize(dto.BestRuns),
            RecentRunsJson = JsonSerializer.Serialize(dto.RecentRuns),
            UpdatedAt = dto.UpdatedAt
        };

        await _localDb.SaveMythicPlusSummaryAsync(entity);
    }
}
