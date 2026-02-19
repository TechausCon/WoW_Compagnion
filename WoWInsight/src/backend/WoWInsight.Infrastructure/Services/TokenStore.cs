using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using WoWInsight.Application.Interfaces;
using WoWInsight.Domain.Entities;
using WoWInsight.Infrastructure.Persistence;

namespace WoWInsight.Infrastructure.Services;

public class TokenStore : ITokenStore
{
    private readonly AppDbContext _context;
    private readonly IDataProtector _protector;

    public TokenStore(AppDbContext context, IDataProtectionProvider provider)
    {
        _context = context;
        _protector = provider.CreateProtector("WoWInsight.Tokens");
    }

    public async Task StoreTokenAsync(Guid userAccountId, string accessToken, string refreshToken, DateTimeOffset expiresAt, DateTimeOffset? refreshExpiresAt)
    {
        var token = await _context.OAuthTokens.FirstOrDefaultAsync(t => t.UserAccountId == userAccountId);
        if (token == null)
        {
            token = new OAuthToken
            {
                UserAccountId = userAccountId
            };
            await _context.OAuthTokens.AddAsync(token);
        }

        token.EncryptedAccessToken = _protector.Protect(accessToken);
        token.EncryptedRefreshToken = _protector.Protect(refreshToken);
        token.ExpiresAt = expiresAt;
        token.RefreshExpiresAt = refreshExpiresAt;
        token.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt)> GetTokenAsync(Guid userAccountId)
    {
        var token = await _context.OAuthTokens.FirstOrDefaultAsync(t => t.UserAccountId == userAccountId);
        if (token == null) return (string.Empty, string.Empty, DateTimeOffset.MinValue);

        try
        {
            var accessToken = _protector.Unprotect(token.EncryptedAccessToken);
            var refreshToken = _protector.Unprotect(token.EncryptedRefreshToken);
            return (accessToken, refreshToken, token.ExpiresAt);
        }
        catch
        {
            // Decryption failed (maybe key rotated/lost)
            return (string.Empty, string.Empty, DateTimeOffset.MinValue);
        }
    }

    public async Task DeleteTokenAsync(Guid userAccountId)
    {
        var token = await _context.OAuthTokens.FirstOrDefaultAsync(t => t.UserAccountId == userAccountId);
        if (token != null)
        {
            _context.OAuthTokens.Remove(token);
            await _context.SaveChangesAsync();
        }
    }

    public async Task StorePkceRequestAsync(string state, string codeVerifier, string region)
    {
        var request = new PkceRequest
        {
            State = state,
            CodeVerifier = codeVerifier,
            Region = region,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10) // 10 min expiry for login
        };

        await _context.PkceRequests.AddAsync(request);
        await _context.SaveChangesAsync();
    }

    public async Task<PkceRequest?> GetPkceRequestAsync(string state)
    {
        return await _context.PkceRequests.FirstOrDefaultAsync(r => r.State == state);
    }

    public async Task DeletePkceRequestAsync(string state)
    {
        var request = await _context.PkceRequests.FirstOrDefaultAsync(r => r.State == state);
        if (request != null)
        {
            _context.PkceRequests.Remove(request);
            await _context.SaveChangesAsync();
        }
    }
}
