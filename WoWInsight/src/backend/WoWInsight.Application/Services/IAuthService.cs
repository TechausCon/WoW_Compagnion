using System;
using System.Threading.Tasks;
using WoWInsight.Domain.Entities;

namespace WoWInsight.Application.Services;

public interface IAuthService
{
    Task<string> StartLoginAsync(string region);
    Task<(string AccessToken, string RefreshToken)> HandleCallbackAsync(string code, string state);
    Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken);
    Task<UserAccount?> GetUserAsync(Guid userId);
}
