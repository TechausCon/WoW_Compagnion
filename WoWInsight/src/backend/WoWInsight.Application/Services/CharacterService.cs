using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WoWInsight.Application.DTOs;
using WoWInsight.Application.Interfaces;
using WoWInsight.Domain.Entities;

namespace WoWInsight.Application.Services;

public class CharacterService : ICharacterService
{
    private readonly ITokenStore _tokenStore;
    private readonly IBlizzardService _blizzardService;
    private readonly IRaiderIoService _raiderIoService;
    private readonly IUserAccountRepository _userAccountRepository;

    public CharacterService(ITokenStore tokenStore, IBlizzardService blizzardService, IRaiderIoService raiderIoService, IUserAccountRepository userAccountRepository)
    {
        _tokenStore = tokenStore;
        _blizzardService = blizzardService;
        _raiderIoService = raiderIoService;
        _userAccountRepository = userAccountRepository;
    }

    public async Task<List<CharacterDto>> GetCharactersAsync(Guid userId)
    {
        var user = await _userAccountRepository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found.");

        var (accessToken, refreshToken, expiresAt) = await _tokenStore.GetTokenAsync(userId);
        if (string.IsNullOrEmpty(accessToken)) throw new Exception("No access token found.");

        // Check Expiry (with 5 min buffer)
        if (DateTimeOffset.UtcNow.AddMinutes(5) >= expiresAt)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                // Can't refresh, hope access token still works or let it fail
            }
            else
            {
                try
                {
                    var (newAccess, newRefresh, expiresIn, scope) = await _blizzardService.RefreshTokenAsync(user.Region, refreshToken);

                    var newExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn);

                    await _tokenStore.StoreTokenAsync(userId, newAccess, newRefresh, newExpiresAt, null);
                    accessToken = newAccess;
                }
                catch
                {
                    // Refresh failed. Token might be revoked.
                    // We can proceed to try existing token or throw.
                    // For now, proceed and let it fail if invalid.
                }
            }
        }

        try
        {
            return await _blizzardService.GetCharactersAsync(user.Region, accessToken);
        }
        catch (System.Net.Http.HttpRequestException ex) when (ex.Message.Contains("Unauthorized") || ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // If we got 401 and haven't tried refreshing yet (or refresh failed above), we could try one more time?
            // But if we already refreshed above, then token is really bad.
            // If we didn't refresh above (because expiry was far), maybe it was revoked?
            // Implementing a second retry here is complex without tracking "didRefresh".
            // Since we check expiry proactively, this should be rare.
            throw new Exception("Blizzard Session Expired. Please login again.");
        }
    }

    public async Task<MythicPlusSummaryDto> GetMythicPlusSummaryAsync(Guid userId, string characterKey)
    {
        // key: {region}:{realm}:{name}
        var parts = characterKey.Split(':');
        if (parts.Length != 3) throw new ArgumentException("Invalid character key format.");

        var region = parts[0];
        var realm = parts[1];
        var name = parts[2];

        // Raider.IO is public API, doesn't need user token.
        return await _raiderIoService.GetCharacterMythicPlusSummaryAsync(region, realm, name);
    }
}
