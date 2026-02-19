using System;
using System.Threading.Tasks;
using WoWInsight.Domain.Entities;
using WoWInsight.Application.DTOs;

namespace WoWInsight.Application.Interfaces;

public interface ITokenStore
{
    Task StoreTokenAsync(Guid userAccountId, string accessToken, string refreshToken, DateTimeOffset expiresAt, DateTimeOffset? refreshExpiresAt);
    Task<(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt)> GetTokenAsync(Guid userAccountId);
    Task DeleteTokenAsync(Guid userAccountId);

    // For PKCE
    Task StorePkceRequestAsync(string state, string codeVerifier, string region);
    Task<PkceRequest?> GetPkceRequestAsync(string state);
    Task DeletePkceRequestAsync(string state);
}
