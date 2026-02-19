using System.Threading.Tasks;

namespace WoWInsight.Mobile.Services;

public interface ISyncService
{
    Task<bool> SyncCharactersAsync();
    Task SyncMythicPlusAsync(string characterKey);
}
