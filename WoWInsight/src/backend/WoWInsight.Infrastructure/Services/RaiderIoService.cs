using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using WoWInsight.Application.DTOs;
using WoWInsight.Application.Interfaces;
using WoWInsight.Infrastructure.Configuration;

namespace WoWInsight.Infrastructure.Services;

public class RaiderIoService : IRaiderIoService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly BlizzardSettings _settings; // Reuse caching config from settings if available or hardcode 10 min

    public RaiderIoService(HttpClient httpClient, IMemoryCache cache, IOptions<BlizzardSettings> settings)
    {
        _httpClient = httpClient;
        _cache = cache;
        _settings = settings.Value;
    }

    public async Task<MythicPlusSummaryDto> GetCharacterMythicPlusSummaryAsync(string region, string realm, string name)
    {
        var cacheKey = $"rio:{region}:{realm}:{name}";

        if (_cache.TryGetValue(cacheKey, out MythicPlusSummaryDto? cached))
        {
            return cached!;
        }

        var url = $"https://raider.io/api/v1/characters/profile?region={region}&realm={realm}&name={name}&fields=mythic_plus_scores_by_season:current,mythic_plus_recent_runs,mythic_plus_best_runs";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            // If 404, maybe return empty summary or throw?
            // Prompt: "401/403/404 von extern -> API gibt sinnvolle ProblemDetails zurück".
            // So throw.
            throw new HttpRequestException($"Raider.IO Error: {response.StatusCode}");
        }

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        var summary = new MythicPlusSummaryDto
        {
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Scores
        if (json.TryGetProperty("mythic_plus_scores_by_season", out var seasons) && seasons.EnumerateArray().Any())
        {
             // Usually it's an array of seasons, we requested current.
             // Wait, API returns array of objects with 'scores' inside?
             // Actually request was `mythic_plus_scores_by_season:current`. It returns an array with one element usually.
             var current = seasons.EnumerateArray().First();
             if (current.TryGetProperty("scores", out var scores))
             {
                 summary.TotalScore = scores.TryGetProperty("all", out var all) ? all.GetDouble() : 0;
             }
        }

        // Runs
        summary.RecentRuns = ParseRuns(json, "mythic_plus_recent_runs");
        summary.BestRuns = ParseRuns(json, "mythic_plus_best_runs");

        _cache.Set(cacheKey, summary, TimeSpan.FromMinutes(10)); // Config 10 min

        return summary;
    }

    private List<RunDto> ParseRuns(JsonElement json, string propertyName)
    {
        var list = new List<RunDto>();
        if (json.TryGetProperty(propertyName, out var runs))
        {
            foreach (var run in runs.EnumerateArray())
            {
                list.Add(new RunDto
                {
                    Dungeon = run.GetProperty("dungeon").GetString() ?? "",
                    Level = run.GetProperty("mythic_level").GetInt32(),
                    Score = run.GetProperty("score").GetDouble(),
                    Timed = run.GetProperty("num_keystone_upgrades").GetInt32() > 0,
                    CompletedAt = run.GetProperty("completed_at").GetDateTimeOffset()
                });
            }
        }
        return list;
    }
}
