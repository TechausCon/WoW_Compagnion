using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using WoWInsight.Mobile.DTOs;

namespace WoWInsight.Mobile.Services;

public class BackendApiClient : IBackendApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;
    private readonly IAppConfig _appConfig;

    public BackendApiClient(HttpClient httpClient, IAuthService authService, IAppConfig appConfig)
    {
        _httpClient = httpClient;
        _authService = authService;
        _appConfig = appConfig;

        if (_httpClient.BaseAddress == null)
        {
             _httpClient.BaseAddress = new Uri(_appConfig.ApiBaseUrl);
        }

        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    private async Task EnsureAuthHeaderAsync()
    {
        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    private async Task<bool> TryRefreshAsync()
    {
        var refreshToken = await _authService.GetRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken)) return false;

        try
        {
            var content = JsonContent.Create(new RefreshTokenRequest { RefreshToken = refreshToken });
            var response = await _httpClient.PostAsync("auth/blizzard/refresh", content);

            if (response.IsSuccessStatusCode)
            {
                var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
                if (tokens != null)
                {
                    await _authService.SaveTokensAsync(tokens.AccessToken, tokens.RefreshToken);
                    return true;
                }
            }
        }
        catch
        {
            // Refresh failed
        }
        return false;
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(Func<Task<HttpResponseMessage>> action)
    {
        await EnsureAuthHeaderAsync();
        var response = await action();

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            if (await TryRefreshAsync())
            {
                await EnsureAuthHeaderAsync(); // Update header with new token
                response = await action(); // Retry
            }
            else
            {
                // If refresh fails, we are done.
                await _authService.DeleteTokensAsync();
                throw new UnauthorizedAccessException("Session expired. Please login again.");
            }
        }

        // Final check
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
             await _authService.DeleteTokensAsync();
             throw new UnauthorizedAccessException("Session expired. Please login again.");
        }

        response.EnsureSuccessStatusCode();
        return response;
    }

    public async Task<List<CharacterDto>> GetCharactersAsync()
    {
        var response = await SendWithRetryAsync(() => _httpClient.GetAsync("me/characters"));
        var result = await response.Content.ReadFromJsonAsync<List<CharacterDto>>();
        return result ?? new List<CharacterDto>();
    }

    public async Task<MythicPlusSummaryDto> GetMythicPlusSummaryAsync(string characterKey)
    {
        var response = await SendWithRetryAsync(() => _httpClient.GetAsync($"characters/{characterKey}/mythicplus"));
        var result = await response.Content.ReadFromJsonAsync<MythicPlusSummaryDto>();
        return result ?? new MythicPlusSummaryDto();
    }

    public string GetAuthUrl()
    {
        var baseUrl = _httpClient.BaseAddress?.ToString() ?? _appConfig.ApiBaseUrl;
        if (!baseUrl.EndsWith("/")) baseUrl += "/";
        return $"{baseUrl}auth/blizzard/start?region=eu";
    }
}
