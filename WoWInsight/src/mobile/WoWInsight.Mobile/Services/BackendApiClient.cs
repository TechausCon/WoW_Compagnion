using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using WoWInsight.Mobile.DTOs;

namespace WoWInsight.Mobile.Services;

public class BackendApiClient
{
    private readonly HttpClient _httpClient;
    private const string TokenKey = "auth_token";

    public BackendApiClient()
    {
        var handler = new HttpClientHandler();
        // Ignore SSL errors for development
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

        _httpClient = new HttpClient(handler);

        if (DeviceInfo.Platform == DevicePlatform.Android)
            _httpClient.BaseAddress = new Uri("https://10.0.2.2:7123");
        else
            _httpClient.BaseAddress = new Uri("https://localhost:7123");

        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(TokenKey);
    }

    public async Task SaveTokenAsync(string token)
    {
        await SecureStorage.Default.SetAsync(TokenKey, token);
    }

    public async Task DeleteTokenAsync()
    {
        SecureStorage.Default.Remove(TokenKey);
    }

    public async Task<List<CharacterDto>> GetCharactersAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token)) throw new Exception("Not logged in");

        var request = new HttpRequestMessage(HttpMethod.Get, "/me/characters");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<CharacterDto>>();
    }

    public async Task<MythicPlusSummaryDto> GetMythicPlusSummaryAsync(string characterKey)
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token)) throw new Exception("Not logged in");

        var request = new HttpRequestMessage(HttpMethod.Get, $"/characters/{characterKey}/mythicplus");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<MythicPlusSummaryDto>();
    }

    // Auth Start URL helper
    public string GetAuthUrl()
    {
        return $"{_httpClient.BaseAddress}auth/blizzard/start?region=eu";
    }
}
