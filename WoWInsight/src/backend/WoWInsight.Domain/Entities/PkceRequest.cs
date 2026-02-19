using System;

namespace WoWInsight.Domain.Entities;

public class PkceRequest
{
    public Guid Id { get; set; }

    // The state parameter sent to Blizzard
    public string State { get; set; } = string.Empty;

    // The hashed code verifier stored securely (or even the plain verifier if the DB is secure enough, but prompt suggests "optional gehashed" or "verifier only server side")
    // If I hash it here, I can't reconstruct the plain verifier to send to the token endpoint.
    // Wait, PKCE flow requires sending the PLAIN verifier to the token endpoint.
    // So I must store the PLAIN verifier on the server side to exchange the code.
    // "speichert PkceRequest in DB (verifier nur serverseitig, optional gehashed)"
    // If it's hashed in DB, I can't use it to exchange code.
    // Ah, usually the client sends the verifier. But here, the Backend handles the exchange.
    // So the Backend needs the verifier.
    // So I must store the plain verifier (encrypted preferably) or just plain if trusted DB.
    // The prompt says "verifier nur serverseitig, optional gehashed". This is confusing.
    // If I hash it, I can't send it to Blizzard. Blizzard expects `code_verifier`.
    // Maybe the prompt means "store it securely"?
    // I will store it encrypted or plain. I'll store it plain for simplicity as it's a short lived ephemeral secret.
    // Wait, maybe the prompt implies the client sends the verifier?
    // No, "2. GET /auth/blizzard/callback?code=...&state=... -> validiert state, liest verifier, tauscht code gegen tokens".
    // So backend reads verifier from DB. So it MUST be recoverable.
    // I will store it as `CodeVerifier`.

    public string CodeVerifier { get; set; } = string.Empty;

    public string Region { get; set; } = "eu"; // to know which region to call back

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }
}
