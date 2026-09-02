using CompetitiveCounterApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CompetitiveCounterApp.Data
{
    public class PlayerRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public PlayerRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<Player>> ListAsync()
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.Players
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Player?> GetAsync(int id)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.Players
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ID == id);
        }

        public async Task<bool> HasParticipationsAsync(int playerId)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.SessionPlayers.AnyAsync(sp => sp.PlayerID == playerId);
        }

        /// <summary>
        /// Jugadores del catálogo que aún no están en la sesión.
        /// </summary>
        public async Task<List<Player>> ListAvailableForSessionAsync(int sessionId)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            var inSession = db.SessionPlayers
                .Where(sp => sp.SessionID == sessionId)
                .Select(sp => sp.PlayerID);

            return await db.Players
                .AsNoTracking()
                .Where(p => !inSession.Contains(p.ID))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task SaveItemAsync(Player player)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            if (player.ID == 0)
                db.Players.Add(player);
            else
                db.Players.Update(player);

            await db.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(Player player)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            if (await db.SessionPlayers.AnyAsync(sp => sp.PlayerID == player.ID))
                throw new InvalidOperationException(
                    "No se puede eliminar el jugador porque participa en sesiones.");

            db.Players.Remove(player);
            await db.SaveChangesAsync();
        }
    }
}
