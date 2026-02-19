using Microsoft.EntityFrameworkCore;
using WoWInsight.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace WoWInsight.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<UserAccount> UserAccounts { get; }
    DbSet<OAuthToken> OAuthTokens { get; }
    DbSet<PkceRequest> PkceRequests { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
