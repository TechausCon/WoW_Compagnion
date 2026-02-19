using System.Collections.Generic;
using System.Threading.Tasks;
using WoWInsight.Application.DTOs;
using WoWInsight.Domain.Entities;

namespace WoWInsight.Application.Interfaces;

public interface IBlizzardService
{
    // Authorization
    string GetAuthorizationUrl(string region, string state, string codeChallenge);
    Task<(string AccessToken, string RefreshToken, int ExpiresIn, string Scope)> ExchangeCodeForTokenAsync(string region, string code, string codeVerifier);
    Task<UserAccount> GetUserProfileAsync(string region, string accessToken);

    // Characters
    Task<List<CharacterDto>> GetCharactersAsync(string region, string accessToken);
}
