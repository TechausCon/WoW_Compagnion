using Microsoft.EntityFrameworkCore;
using WoWInsight.Application.Interfaces;
using WoWInsight.Domain.Entities;

namespace WoWInsight.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<OAuthToken> OAuthTokens => Set<OAuthToken>();
    public DbSet<PkceRequest> PkceRequests => Set<PkceRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Sub).IsUnique(); // Sub is unique per region, but assume global unique ID from Blizzard or pair with region?
            // Blizzard 'sub' (Account ID) is unique globally? Usually yes.
            // But battle tag is not unique globally?
            // I'll assume Sub is unique.
        });

        modelBuilder.Entity<OAuthToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.UserAccount)
                  .WithOne(e => e.Token)
                  .HasForeignKey<OAuthToken>(e => e.UserAccountId);
        });

        modelBuilder.Entity<PkceRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.State).IsUnique();
        });
    }
}
