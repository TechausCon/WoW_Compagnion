using System;

namespace WoWInsight.Domain.Entities;

public class OAuthToken
{
    public Guid Id { get; set; }
    public Guid UserAccountId { get; set; }
    public UserAccount? UserAccount { get; set; }

    // Encrypted Blobs
    public string EncryptedAccessToken { get; set; } = string.Empty;
    public string EncryptedRefreshToken { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RefreshExpiresAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
