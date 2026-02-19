using System;

namespace WoWInsight.Domain.Entities;

public class UserAccount
{
    public Guid Id { get; set; }
    public string BattleTag { get; set; } = string.Empty;
    public string Sub { get; set; } = string.Empty; // Blizzard Account ID
    public string Region { get; set; } = "eu";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Backend JWT Refresh Token
    public string BackendRefreshToken { get; set; } = string.Empty;
    public DateTimeOffset? BackendRefreshTokenExpiry { get; set; }

    // Navigation property
    public OAuthToken? Token { get; set; }
}
