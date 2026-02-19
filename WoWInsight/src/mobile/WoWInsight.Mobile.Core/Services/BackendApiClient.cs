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

    private async Task HandleResponseErrors(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await _authService.DeleteTokenAsync();
            throw new UnauthorizedAccessException("Session expired. Please login again.");
        }
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<CharacterDto>> GetCharactersAsync()
    {
        await EnsureAuthHeaderAsync();
        var response = await _httpClient.GetAsync("/me/characters");
        await HandleResponseErrors(response);

        return await response.Content.ReadFromJsonAsync<List<CharacterDto>>();
    }

    public async Task<MythicPlusSummaryDto> GetMythicPlusSummaryAsync(string characterKey)
    {
        await EnsureAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"/characters/{characterKey}/mythicplus");
        await HandleResponseErrors(response);

        return await response.Content.ReadFromJsonAsync<MythicPlusSummaryDto>();
    }

    public string GetAuthUrl()
    {
        var baseUrl = _httpClient.BaseAddress?.ToString() ?? _appConfig.ApiBaseUrl;
        if (!baseUrl.EndsWith("/")) baseUrl += "/";
        return $"{baseUrl}auth/blizzard/start?region=eu";
    }
}
