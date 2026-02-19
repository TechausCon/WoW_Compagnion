using System.Collections.Generic;
using System.Threading.Tasks;
using WoWInsight.Application.DTOs;

namespace WoWInsight.Application.Services;

public interface ICharacterService
{
    Task<List<CharacterDto>> GetCharactersAsync(Guid userId);
    Task<MythicPlusSummaryDto> GetMythicPlusSummaryAsync(Guid userId, string characterKey);
}
