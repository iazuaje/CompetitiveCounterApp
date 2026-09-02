using CompetitiveCounterApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CompetitiveCounterApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Game> Games => Set<Game>();
        public DbSet<Player> Players => Set<Player>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<SessionPlayer> SessionPlayers => Set<SessionPlayer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>(entity =>
            {
                entity.ToTable("Games");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Icon).HasDefaultValue(string.Empty);
                entity.Property(e => e.Description).HasDefaultValue(string.Empty);
                entity.Property(e => e.ColorLight).HasDefaultValue("#E63946");
                entity.Property(e => e.ColorDark).HasDefaultValue("#FF5964");
                entity.Property(e => e.ImagePath).HasDefaultValue(string.Empty);
                entity.Property(e => e.CreatedDate);

                entity.Ignore(e => e.ThemeColors);
                entity.Ignore(e => e.GameColorLight);
                entity.Ignore(e => e.GameColorDark);
                entity.Ignore(e => e.CurrentGameColor);
                entity.Ignore(e => e.SurfaceColor);
                entity.Ignore(e => e.OnSurfaceColor);
                entity.Ignore(e => e.GameImage);
            });

            modelBuilder.Entity<Player>(entity =>
            {
                entity.ToTable("Players");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.ColorLight).HasDefaultValue("#C62828");
                entity.Property(e => e.ColorDark).HasDefaultValue("#EF9A9A");
                entity.Ignore(e => e.ThemeColors);
                entity.Ignore(e => e.CurrentColor);
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.ToTable("Sessions");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.SessionDate).IsRequired();
                entity.Property(e => e.Notes).HasDefaultValue(string.Empty);
                entity.Property(e => e.ClosedAt);
                entity.Ignore(e => e.IsActive);
                entity.Ignore(e => e.SessionDateLocal);
                entity.Ignore(e => e.ClosedAtLocal);

                // Una sola sesión activa (ClosedAt IS NULL) por juego.
                entity.HasIndex(e => e.GameID)
                    .IsUnique()
                    .HasFilter("\"ClosedAt\" IS NULL")
                    .HasDatabaseName("IX_Sessions_GameID_Active");

                entity.HasOne(e => e.Game)
                    .WithMany()
                    .HasForeignKey(e => e.GameID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.SessionPlayers)
                    .WithOne(e => e.Session)
                    .HasForeignKey(e => e.SessionID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SessionPlayer>(entity =>
            {
                entity.ToTable("SessionPlayers");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Wins).HasDefaultValue(0);

                entity.HasIndex(e => new { e.SessionID, e.PlayerID })
                    .IsUnique()
                    .HasDatabaseName("IX_SessionPlayers_SessionID_PlayerID");

                entity.HasOne(e => e.Player)
                    .WithMany()
                    .HasForeignKey(e => e.PlayerID)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
