using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using WoWInsight.Application.DTOs;
using WoWInsight.Application.Interfaces;
using WoWInsight.Domain.Entities;
using WoWInsight.Infrastructure.Configuration;

namespace WoWInsight.Infrastructure.Services;

public class BlizzardService : IBlizzardService
{
    private readonly HttpClient _httpClient;
    private readonly BlizzardSettings _settings;
    private readonly IMemoryCache _cache;

    public BlizzardService(HttpClient httpClient, IOptions<BlizzardSettings> settings, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _cache = cache;
    }

    public string GetAuthorizationUrl(string region, string state, string codeChallenge)
    {
        var baseUrl = region.ToLower() == "us" ? "https://us.battle.net" : "https://eu.battle.net";
        var redirectUri = System.Net.WebUtility.UrlEncode(_settings.RedirectUri);
        var scope = "wow.profile";

        return $"{baseUrl}/oauth/authorize?client_id={_settings.ClientId}&scope={scope}&state={state}&redirect_uri={redirectUri}&response_type=code&code_challenge={codeChallenge}&code_challenge_method=S256";
    }

    public async Task<(string AccessToken, string RefreshToken, int ExpiresIn, string Scope)> ExchangeCodeForTokenAsync(string region, string code, string codeVerifier)
    {
        var baseUrl = region.ToLower() == "us" ? "https://us.battle.net" : "https://eu.battle.net";

        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/oauth/token");
        var collection = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "authorization_code"),
            new("code", code),
            new("redirect_uri", _settings.RedirectUri),
            new("code_verifier", codeVerifier),
            new("client_id", _settings.ClientId),
            new("client_secret", _settings.ClientSecret)
        };

        request.Content = new FormUrlEncodedContent(collection);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = json.GetProperty("access_token").GetString() ?? throw new Exception("No access_token");
        var refreshToken = json.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : string.Empty;
        var expiresIn = json.GetProperty("expires_in").GetInt32();
        var scope = json.GetProperty("scope").GetString() ?? string.Empty;

        return (accessToken, refreshToken ?? string.Empty, expiresIn, scope);
    }

    public async Task<(string AccessToken, string RefreshToken, int ExpiresIn, string Scope)> RefreshTokenAsync(string region, string refreshToken)
    {
        var baseUrl = region.ToLower() == "us" ? "https://us.battle.net" : "https://eu.battle.net";

        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/oauth/token");
        var collection = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "refresh_token"),
            new("refresh_token", refreshToken),
            new("client_id", _settings.ClientId),
            new("client_secret", _settings.ClientSecret)
        };

        request.Content = new FormUrlEncodedContent(collection);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = json.GetProperty("access_token").GetString() ?? throw new Exception("No access_token");
        // Sometimes refresh_token is returned again, sometimes not (keeps old one).
        var newRefreshToken = json.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : refreshToken;
        var expiresIn = json.GetProperty("expires_in").GetInt32();
        var scope = json.GetProperty("scope").GetString() ?? string.Empty;

        return (accessToken, newRefreshToken ?? refreshToken, expiresIn, scope);
    }

    public async Task<UserAccount> GetUserProfileAsync(string region, string accessToken)
    {
        var baseUrl = region.ToLower() == "us" ? "https://us.battle.net" : "https://eu.battle.net";

        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/oauth/userinfo");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        var sub = json.GetProperty("sub").GetString() ?? throw new Exception("No sub");
        var battleTag = json.GetProperty("battletag").GetString() ?? "Unknown";

        return new UserAccount
        {
            Sub = sub,
            BattleTag = battleTag,
            Region = region
        };
    }

    public async Task<List<CharacterDto>> GetCharactersAsync(string region, string accessToken)
    {
        var cacheKey = $"chars:{accessToken.GetHashCode()}";

        if (_cache.TryGetValue(cacheKey, out List<CharacterDto>? cached))
        {
            return cached!;
        }

        var baseUrl = region.ToLower() == "us" ? "https://us.api.blizzard.com" : "https://eu.api.blizzard.com";

        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/profile/user/wow?namespace=profile-{region}&locale=en_US");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
             // If 401, we might throw a specific exception to indicate token issues,
             // but `CharacterService` will handle refreshing via proactive check or try/catch around this.
             // We'll just throw generic for now.
             throw new HttpRequestException($"Blizzard API Error: {response.StatusCode}");
        }

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var accounts = json.GetProperty("wow_accounts").EnumerateArray();

        var characters = new List<CharacterDto>();

        foreach (var account in accounts)
        {
            if (account.TryGetProperty("characters", out var chars))
            {
                foreach (var character in chars.EnumerateArray())
                {
                    var name = character.GetProperty("name").GetString() ?? "";
                    var realm = character.GetProperty("realm").GetProperty("slug").GetString() ?? "";
                    var level = character.GetProperty("level").GetInt32();
                    var playableClass = character.GetProperty("playable_class").GetProperty("name").GetString() ?? "";
                    var faction = character.TryGetProperty("faction", out var f) ? f.GetProperty("name").GetString() ?? "" : "Unknown";

                    characters.Add(new CharacterDto
                    {
                        CharacterKey = $"{region}:{realm}:{name.ToLower()}",
                        Name = name,
                        Realm = realm,
                        Region = region,
                        Level = level,
                        Class = playableClass,
                        Faction = faction
                    });
                }
            }
        }

        _cache.Set(cacheKey, characters, TimeSpan.FromMinutes(5));

        return characters;
    }
}
