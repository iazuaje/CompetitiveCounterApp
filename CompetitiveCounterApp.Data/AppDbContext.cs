using CompetitiveCounterApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CompetitiveCounterApp.Data
{
    public class AppDbContext : DbContext
    {
        private static readonly LocalDateTimeConverter LocalDateTimeConverter = new();
        private static readonly NullableLocalDateTimeConverter NullableLocalDateTimeConverter = new();

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Game> Games => Set<Game>();
        public DbSet<Player> Players => Set<Player>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<SessionPlayer> SessionPlayers => Set<SessionPlayer>();
        public DbSet<Category> Categories => Set<Category>();

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
                entity.Property(e => e.SortOrder);
                entity.Property(e => e.IsArchived);

                // Etiquetas: muchos a muchos; borrar un juego o una categoría borra solo la relación.
                entity.HasMany(e => e.Categories)
                    .WithMany(c => c.Games)
                    .UsingEntity<Dictionary<string, object>>(
                        "GameCategories",
                        right => right.HasOne<Category>().WithMany().HasForeignKey("CategoryID").OnDelete(DeleteBehavior.Cascade),
                        left => left.HasOne<Game>().WithMany().HasForeignKey("GameID").OnDelete(DeleteBehavior.Cascade),
                        join =>
                        {
                            join.ToTable("GameCategories");
                            join.HasKey("GameID", "CategoryID");
                        });

                entity.Ignore(e => e.HasCategories);
                entity.Ignore(e => e.CategoriesText);
                entity.Ignore(e => e.HasActiveSession);
                entity.Ignore(e => e.HasWins);
                entity.Ignore(e => e.LastPlayedDate);
                entity.Ignore(e => e.ThemeColors);
                entity.Ignore(e => e.GameColorLight);
                entity.Ignore(e => e.GameColorDark);
                entity.Ignore(e => e.CurrentGameColor);
                entity.Ignore(e => e.OnGameColor);
                entity.Ignore(e => e.ToolbarColor);
                entity.Ignore(e => e.OnToolbarColor);
                entity.Ignore(e => e.ComplementaryColor);
                entity.Ignore(e => e.SurfaceColor);
                entity.Ignore(e => e.OnSurfaceColor);
                entity.Ignore(e => e.GameImage);
                entity.Ignore(e => e.HasImage);
                entity.Ignore(e => e.SessionCount);
                entity.Ignore(e => e.LeaderName);
                entity.Ignore(e => e.LeaderSessionsWon);
                entity.Ignore(e => e.HasLeader);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired();
                entity.HasIndex(e => e.Name)
                    .IsUnique()
                    .HasDatabaseName("IX_Categories_Name");
            });

            modelBuilder.Entity<Player>(entity =>
            {
                entity.ToTable("Players");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Icon).HasDefaultValue(string.Empty);
                entity.Property(e => e.ColorLight).HasDefaultValue("#C62828");
                entity.Property(e => e.ColorDark).HasDefaultValue("#EF9A9A");
                entity.Property(e => e.IsArchived);
                entity.Ignore(e => e.ThemeColors);
                entity.Ignore(e => e.CurrentColor);
                entity.Ignore(e => e.OnColor);
                entity.Ignore(e => e.ToolbarColor);
                entity.Ignore(e => e.OnToolbarColor);
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.ToTable("Sessions");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.SessionDate)
                    .IsRequired()
                    .HasConversion(LocalDateTimeConverter);
                entity.Property(e => e.Notes).HasDefaultValue(string.Empty);
                entity.Property(e => e.ClosedAt)
                    .HasConversion(NullableLocalDateTimeConverter);
                entity.Ignore(e => e.IsActive);
                entity.Ignore(e => e.HasBestPlayer);
                entity.Ignore(e => e.TotalWins);
                entity.Ignore(e => e.PlayerCount);
                entity.Ignore(e => e.SessionDateLocal);
                entity.Ignore(e => e.ClosedAtLocal);
                entity.Ignore(e => e.ListCardBackground);
                entity.Ignore(e => e.ListCardStroke);
                entity.Ignore(e => e.ListCardStrokeThickness);
                entity.Ignore(e => e.ListCardIconColor);
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
