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
        // Generate PKCE Challenge
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        var state = GenerateState();

        // Store PKCE Request
        await _tokenStore.StorePkceRequestAsync(state, codeVerifier, region);

        // Get Authorization URL
        return _blizzardService.GetAuthorizationUrl(region, state, codeChallenge);
    }

    public async Task<string> HandleCallbackAsync(string code, string state)
    {
        // Validate State
        var pkceRequest = await _tokenStore.GetPkceRequestAsync(state);
        if (pkceRequest == null || pkceRequest.ExpiresAt < DateTimeOffset.UtcNow)
        {
            throw new Exception("Invalid or expired state.");
        }

        // Exchange Code
        var (accessToken, refreshToken, expiresIn, scope) = await _blizzardService.ExchangeCodeForTokenAsync(pkceRequest.Region, code, pkceRequest.CodeVerifier);

        // Get User Profile
        var userProfile = await _blizzardService.GetUserProfileAsync(pkceRequest.Region, accessToken);

        // Upsert User
        var existingUser = await _userAccountRepository.GetBySubAsync(userProfile.Sub);
        if (existingUser != null)
        {
            existingUser.BattleTag = userProfile.BattleTag;
            existingUser.Region = pkceRequest.Region;
            await _userAccountRepository.UpdateAsync(existingUser);
            userProfile = existingUser;
        }
        else
        {
            userProfile.Region = pkceRequest.Region;
            await _userAccountRepository.AddAsync(userProfile);
        }

        // Store Token Encrypted (via ITokenStore)
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn);
        // Refresh token expiry not strictly provided by Blizzard OAuth usually, but we can estimate or assume long lived (30 days often).
        // Actually Blizzard refresh tokens are valid for 30 days if unused, or longer.
        var refreshExpiresAt = DateTimeOffset.UtcNow.AddDays(30);

        await _tokenStore.StoreTokenAsync(userProfile.Id, accessToken, refreshToken, expiresAt, refreshExpiresAt);

        // Cleanup PKCE Request
        await _tokenStore.DeletePkceRequestAsync(state);

        // Generate Backend JWT
        return _jwtService.GenerateToken(userProfile);
    }

    public async Task<UserAccount?> GetUserAsync(Guid userId)
    {
        return await _userAccountRepository.GetByIdAsync(userId);
    }

    // Helper Methods
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
        output = output.Split('=')[0]; // Remove any trailing '='s
        output = output.Replace('+', '-'); // 62nd char of encoding
        output = output.Replace('/', '_'); // 63rd char of encoding
        return output;
    }
}
