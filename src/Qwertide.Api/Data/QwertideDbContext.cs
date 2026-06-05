using Microsoft.EntityFrameworkCore;
using Qwertide.Api.Models;

namespace Qwertide.Api.Data;

/// <summary>
/// EF Core context for the leaderboard. One <see cref="DbSet{Score}"/>, created
/// and evolved through migrations (PDD §7).
/// </summary>
public sealed class QwertideDbContext : DbContext
{
    public QwertideDbContext(DbContextOptions<QwertideDbContext> options) : base(options) { }

    public DbSet<Score> Scores => Set<Score>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Score>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.PlayerName).IsRequired().HasMaxLength(30);
            // Leaderboard reads are ordered by Wpm desc; index keeps that cheap.
            entity.HasIndex(s => s.Wpm);
        });
    }
}
