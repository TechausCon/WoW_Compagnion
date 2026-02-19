using System.Threading.Tasks;
using WoWInsight.Domain.Entities;

namespace WoWInsight.Application.Services;

public interface IAuthService
{
    Task<string> StartLoginAsync(string region);
    Task<string> HandleCallbackAsync(string code, string state);
    Task<UserAccount?> GetUserAsync(Guid userId);
}
