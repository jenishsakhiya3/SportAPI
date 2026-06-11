using Microsoft.EntityFrameworkCore;

namespace SportAPI.Data;

public class SportDbContext : DbContext
{
    public SportDbContext(DbContextOptions<SportDbContext> options) : base(options) { }

    public DbSet<Sport> Sports => Set<Sport>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchStat> MatchStats => Set<MatchStat>();
    public DbSet<League> Leagues => Set<League>();
    public DbSet<Standing> Standings => Set<Standing>();
    public DbSet<Coach> Coaches => Set<Coach>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Injury> Injuries => Set<Injury>();
    public DbSet<Transfer> Transfers => Set<Transfer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Composite Key for Standing
        modelBuilder.Entity<Standing>()
            .HasKey(s => new { s.LeagueId, s.TeamId });

        // Configure One-to-Many Relationships and restrict cascade deletes to avoid database cycles
        modelBuilder.Entity<Match>()
            .HasOne(m => m.HomeTeam)
            .WithMany()
            .HasForeignKey(m => m.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Match>()
            .HasOne(m => m.AwayTeam)
            .WithMany()
            .HasForeignKey(m => m.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transfer>()
            .HasOne(t => t.FromTeam)
            .WithMany()
            .HasForeignKey(t => t.FromTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transfer>()
            .HasOne(t => t.ToTeam)
            .WithMany()
            .HasForeignKey(t => t.ToTeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
