using System;
using System.Threading.Tasks;
using WoWInsight.Domain.Entities;

namespace WoWInsight.Application.Interfaces;

public interface IUserAccountRepository
{
    Task<UserAccount?> GetByBattleTagAsync(string battleTag);
    Task<UserAccount?> GetByIdAsync(Guid id);
    Task AddAsync(UserAccount userAccount);
    Task UpdateAsync(UserAccount userAccount);
    Task<UserAccount?> GetBySubAsync(string sub);
}
