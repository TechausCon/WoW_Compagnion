using System;
using System.Threading.Tasks;

namespace WoWInsight.Mobile.Services;

public interface IAuthService
{
    Task<string?> GetTokenAsync();
    Task<string?> GetRefreshTokenAsync();
    Task SaveTokensAsync(string token, string refreshToken);
    Task DeleteTokensAsync();
    bool IsAuthenticated { get; }
}

public class AuthService : IAuthService
{
    private const string TokenKey = "auth_token";
    private const string RefreshTokenKey = "refresh_token";
    private readonly ISecureStorageService _secureStorage;
    private string? _cachedToken;
    private string? _cachedRefreshToken;

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

    public async Task<string?> GetRefreshTokenAsync()
    {
        if (_cachedRefreshToken == null)
        {
            _cachedRefreshToken = await _secureStorage.GetAsync(RefreshTokenKey);
        }
        return _cachedRefreshToken;
    }

    public async Task SaveTokensAsync(string token, string refreshToken)
    {
        await _secureStorage.SetAsync(TokenKey, token);
        await _secureStorage.SetAsync(RefreshTokenKey, refreshToken);
        _cachedToken = token;
        _cachedRefreshToken = refreshToken;
    }

    public async Task DeleteTokensAsync()
    {
        _secureStorage.Remove(TokenKey);
        _secureStorage.Remove(RefreshTokenKey);
        _cachedToken = null;
        _cachedRefreshToken = null;
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(_cachedToken);
}
