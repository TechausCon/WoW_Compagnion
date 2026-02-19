using System;
using System.Threading.Tasks;

namespace WoWInsight.Mobile.Services;

public interface IAuthService
{
    Task<string?> GetTokenAsync();
    Task SaveTokenAsync(string token);
    Task DeleteTokenAsync();
    bool IsAuthenticated { get; }
}

public class AuthService : IAuthService
{
    private const string TokenKey = "auth_token";
    private readonly ISecureStorageService _secureStorage;
    private string? _cachedToken;

    public AuthService(ISecureStorageService secureStorage)
    {
        _secureStorage = secureStorage;
    }

    public async Task<string?> GetTokenAsync()
    {
        if (_cachedToken == null)
        {
            _cachedToken = await _secureStorage.GetAsync(TokenKey);
        }
        return _cachedToken;
    }

    public async Task SaveTokenAsync(string token)
    {
        await _secureStorage.SetAsync(TokenKey, token);
        _cachedToken = token;
    }

    public async Task DeleteTokenAsync()
    {
        _secureStorage.Remove(TokenKey);
        _cachedToken = null;
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(_cachedToken);
}
