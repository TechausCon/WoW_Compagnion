using System.Collections.Generic;
using System.Threading.Tasks;
using WoWInsight.Mobile.DTOs;

namespace WoWInsight.Mobile.Services;

public interface IBackendApiClient
{
    Task<List<CharacterDto>> GetCharactersAsync();
    Task<MythicPlusSummaryDto> GetMythicPlusSummaryAsync(string characterKey);
    string GetAuthUrl();
}
