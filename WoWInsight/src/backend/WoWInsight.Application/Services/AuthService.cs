using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WoWInsight.Application.Interfaces;
using WoWInsight.Domain.Entities;

namespace WoWInsight.Application.Services;

public class AuthService : IAuthService
{
    private readonly ITokenStore _tokenStore;
    private readonly IBlizzardService _blizzardService;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IJwtService _jwtService;

    public AuthService(ITokenStore tokenStore, IBlizzardService blizzardService, IUserAccountRepository userAccountRepository, IJwtService jwtService)
    {
        _tokenStore = tokenStore;
        _blizzardService = blizzardService;
        _userAccountRepository = userAccountRepository;
        _jwtService = jwtService;
    }

    public async Task<string> StartLoginAsync(string region)
    {
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        var state = GenerateState();

        await _tokenStore.StorePkceRequestAsync(state, codeVerifier, region);

        return _blizzardService.GetAuthorizationUrl(region, state, codeChallenge);
    }

    public async Task<(string AccessToken, string RefreshToken)> HandleCallbackAsync(string code, string state)
    {
        var pkceRequest = await _tokenStore.GetPkceRequestAsync(state);
        if (pkceRequest == null || pkceRequest.ExpiresAt < DateTimeOffset.UtcNow)
        {
            throw new Exception("Invalid or expired state.");
        }

        var (accessToken, refreshToken, expiresIn, scope) = await _blizzardService.ExchangeCodeForTokenAsync(pkceRequest.Region, code, pkceRequest.CodeVerifier);
        var userProfile = await _blizzardService.GetUserProfileAsync(pkceRequest.Region, accessToken);

        var existingUser = await _userAccountRepository.GetBySubAsync(userProfile.Sub);
        string backendRefreshToken;

        if (existingUser != null)
        {
            existingUser.BattleTag = userProfile.BattleTag;
            existingUser.Region = pkceRequest.Region;

            backendRefreshToken = _jwtService.GenerateRefreshToken();
            existingUser.BackendRefreshToken = backendRefreshToken;
            existingUser.BackendRefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(7);

            await _userAccountRepository.UpdateAsync(existingUser);
            userProfile = existingUser;
        }
        else
        {
            userProfile.Region = pkceRequest.Region;
            backendRefreshToken = _jwtService.GenerateRefreshToken();
            userProfile.BackendRefreshToken = backendRefreshToken;
            userProfile.BackendRefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(7);

            await _userAccountRepository.AddAsync(userProfile);
        }

        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn);
        var refreshExpiresAt = DateTimeOffset.UtcNow.AddDays(30);

        await _tokenStore.StoreTokenAsync(userProfile.Id, accessToken, refreshToken, expiresAt, refreshExpiresAt);
        await _tokenStore.DeletePkceRequestAsync(state);

        var backendAccessToken = _jwtService.GenerateToken(userProfile);
        return (backendAccessToken, backendRefreshToken);
    }

    public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken)
    {
        var user = await _userAccountRepository.GetByBackendRefreshTokenAsync(refreshToken);
        if (user == null || user.BackendRefreshTokenExpiry < DateTimeOffset.UtcNow)
        {
            throw new Exception("Invalid or expired refresh token.");
        }

        var newAccessToken = _jwtService.GenerateToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.BackendRefreshToken = newRefreshToken;
        user.BackendRefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(7);
        await _userAccountRepository.UpdateAsync(user);

        return (newAccessToken, newRefreshToken);
    }

    public async Task<UserAccount?> GetUserAsync(Guid userId)
    {
        return await _userAccountRepository.GetByIdAsync(userId);
    }

    private string GenerateCodeVerifier()
    {
        var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Base64UrlEncode(bytes);
    }

    private string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        return Base64UrlEncode(challengeBytes);
    }

    private string GenerateState()
    {
        return Guid.NewGuid().ToString("N");
    }

    private string Base64UrlEncode(byte[] input)
    {
        var output = Convert.ToBase64String(input);
        output = output.Split('=')[0];
        output = output.Replace('+', '-');
        output = output.Replace('/', '_');
        return output;
    }
}
