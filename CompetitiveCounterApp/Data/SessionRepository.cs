using CompetitiveCounterApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CompetitiveCounterApp.Data
{
    public class SessionRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public SessionRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<Session>> ListAsync(int? gameId = null)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            var query = db.Sessions
                .AsNoTracking()
                .Include(s => s.Game)
                .Include(s => s.SessionPlayers)
                    .ThenInclude(sp => sp.Player)
                .AsQueryable();

            if (gameId.HasValue)
                query = query.Where(s => s.GameID == gameId.Value);

            // Activa primero; luego historial por fecha de creación descendente.
            return await query
                .OrderBy(s => s.ClosedAt == null ? 0 : 1)
                .ThenByDescending(s => s.SessionDate)
                .ToListAsync();
        }

        public async Task<Session?> GetAsync(int id)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.Sessions
                .AsNoTracking()
                .Include(s => s.Game)
                .Include(s => s.SessionPlayers.OrderByDescending(sp => sp.Wins))
                    .ThenInclude(sp => sp.Player)
                .FirstOrDefaultAsync(s => s.ID == id);
        }

        public async Task<Session?> GetActiveByGameIdAsync(int gameId)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.Sessions
                .AsNoTracking()
                .Include(s => s.Game)
                .Include(s => s.SessionPlayers.OrderByDescending(sp => sp.Wins))
                    .ThenInclude(sp => sp.Player)
                .FirstOrDefaultAsync(s => s.GameID == gameId && s.ClosedAt == null);
        }

        /// <summary>
        /// Cierra la sesión activa del juego (si existe) y crea una nueva.
        /// </summary>
        public async Task<Session> CreateAsync(int gameId, string? notes = null)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            await using var transaction = await db.Database.BeginTransactionAsync();

            var active = await db.Sessions
                .FirstOrDefaultAsync(s => s.GameID == gameId && s.ClosedAt == null);

            if (active is not null)
                active.ClosedAt = DateTime.Now;

            var session = new Session
            {
                GameID = gameId,
                SessionDate = DateTime.Now,
                Notes = notes?.Trim() ?? string.Empty,
                ClosedAt = null
            };

            db.Sessions.Add(session);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return (await GetAsync(session.ID))!;
        }

        public async Task CloseAsync(int sessionId)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            var session = await db.Sessions.FirstOrDefaultAsync(s => s.ID == sessionId)
                ?? throw new InvalidOperationException("La sesión no existe.");

            if (session.ClosedAt is not null)
                throw new InvalidOperationException("La sesión ya está cerrada.");

            session.ClosedAt = DateTime.Now;
            await db.SaveChangesAsync();
        }

        public async Task<SessionPlayer> AddPlayerAsync(int sessionId, int playerId)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            var session = await db.Sessions.FirstOrDefaultAsync(s => s.ID == sessionId)
                ?? throw new InvalidOperationException("La sesión no existe.");

            if (session.ClosedAt is not null)
                throw new InvalidOperationException("No se pueden agregar jugadores a una sesión cerrada.");

            var playerExists = await db.Players.AnyAsync(p => p.ID == playerId);
            if (!playerExists)
                throw new InvalidOperationException("El jugador no existe.");

            var alreadyInSession = await db.SessionPlayers
                .AnyAsync(sp => sp.SessionID == sessionId && sp.PlayerID == playerId);

            if (alreadyInSession)
                throw new InvalidOperationException("El jugador ya está en esta sesión.");

            var sessionPlayer = new SessionPlayer
            {
                SessionID = sessionId,
                PlayerID = playerId,
                Wins = 0
            };

            db.SessionPlayers.Add(sessionPlayer);
            await db.SaveChangesAsync();

            return (await db.SessionPlayers
                .AsNoTracking()
                .Include(sp => sp.Player)
                .FirstAsync(sp => sp.ID == sessionPlayer.ID));
        }

        /// <summary>
        /// Suma <paramref name="delta"/> a las victorias (no baja de 0).
        /// </summary>
        public async Task<int> AdjustWinsAsync(int sessionId, int playerId, int delta)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var sessionPlayer = await GetEditableSessionPlayerAsync(db, sessionId, playerId);

            sessionPlayer.Wins = Math.Max(0, sessionPlayer.Wins + delta);
            await db.SaveChangesAsync();
            return sessionPlayer.Wins;
        }

        /// <summary>
        /// Fija el valor absoluto de victorias (mínimo 0).
        /// </summary>
        public async Task<int> SetWinsAsync(int sessionId, int playerId, int wins)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var sessionPlayer = await GetEditableSessionPlayerAsync(db, sessionId, playerId);

            sessionPlayer.Wins = Math.Max(0, wins);
            await db.SaveChangesAsync();
            return sessionPlayer.Wins;
        }

        /// <summary>
        /// Actualiza solo datos de la sesión (notas, fechas). No toca SessionPlayers.
        /// </summary>
        public async Task SaveItemAsync(Session session)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            if (session.ID == 0)
            {
                session.Game = null;
                session.SessionPlayers = [];
                db.Sessions.Add(session);
            }
            else
            {
                var existing = await db.Sessions.FirstOrDefaultAsync(s => s.ID == session.ID);
                if (existing is null)
                    return;

                existing.GameID = session.GameID;
                existing.SessionDate = session.SessionDate;
                existing.Notes = session.Notes;
                existing.ClosedAt = session.ClosedAt;
            }

            await db.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(Session session)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            db.Sessions.Remove(session);
            await db.SaveChangesAsync();
        }

        private static async Task<SessionPlayer> GetEditableSessionPlayerAsync(
            AppDbContext db,
            int sessionId,
            int playerId)
        {
            var session = await db.Sessions.FirstOrDefaultAsync(s => s.ID == sessionId)
                ?? throw new InvalidOperationException("La sesión no existe.");

            if (session.ClosedAt is not null)
                throw new InvalidOperationException("No se pueden modificar victorias de una sesión cerrada.");

            return await db.SessionPlayers
                .FirstOrDefaultAsync(sp => sp.SessionID == sessionId && sp.PlayerID == playerId)
                ?? throw new InvalidOperationException("El jugador no está en esta sesión.");
        }
    }
}
