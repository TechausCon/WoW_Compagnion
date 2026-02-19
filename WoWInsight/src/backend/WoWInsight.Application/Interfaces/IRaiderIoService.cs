using System.Threading.Tasks;
using WoWInsight.Application.DTOs;

namespace WoWInsight.Application.Interfaces;

public interface IRaiderIoService
{
    Task<MythicPlusSummaryDto> GetCharacterMythicPlusSummaryAsync(string region, string realm, string name);
}
