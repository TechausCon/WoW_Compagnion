using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WoWInsight.Application.Interfaces;
using WoWInsight.Domain.Entities;

namespace WoWInsight.Infrastructure.Persistence;

public class UserAccountRepository : IUserAccountRepository
{
    private readonly AppDbContext _context;

    public UserAccountRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserAccount?> GetByBattleTagAsync(string battleTag)
    {
        return await _context.UserAccounts.FirstOrDefaultAsync(u => u.BattleTag == battleTag);
    }

    public async Task<UserAccount?> GetByIdAsync(Guid id)
    {
        return await _context.UserAccounts.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task AddAsync(UserAccount userAccount)
    {
        await _context.UserAccounts.AddAsync(userAccount);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(UserAccount userAccount)
    {
        _context.UserAccounts.Update(userAccount);
        await _context.SaveChangesAsync();
    }

    public async Task<UserAccount?> GetBySubAsync(string sub)
    {
        return await _context.UserAccounts.FirstOrDefaultAsync(u => u.Sub == sub);
    }

    public async Task<UserAccount?> GetByBackendRefreshTokenAsync(string token)
    {
        return await _context.UserAccounts.FirstOrDefaultAsync(u => u.BackendRefreshToken == token);
    }
}
