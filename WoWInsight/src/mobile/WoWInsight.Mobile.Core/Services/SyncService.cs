using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WoWInsight.Mobile.DTOs;
using WoWInsight.Mobile.Models;

namespace WoWInsight.Mobile.Services;

public class SyncService : ISyncService
{
    private readonly IBackendApiClient _apiClient;
    private readonly ILocalDbService _localDb;

    public SyncService(IBackendApiClient apiClient, ILocalDbService localDb)
    {
        _apiClient = apiClient;
        _localDb = localDb;
    }

    public async Task<bool> SyncCharactersAsync()
    {
        try
        {
            var dtos = await _apiClient.GetCharactersAsync();
            var entities = new List<Character>();

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

            // Parallel fetching of Mythic+ data with throttling
            using var semaphore = new SemaphoreSlim(5); // Limit concurrency
            var tasks = entities.Select(async c =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await SyncMythicPlusAsync(c.CharacterKey);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to sync M+ for {c.CharacterKey}: {ex.Message}");
                    // Continue despite error
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sync failed: {ex.Message}");
            return false;
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
