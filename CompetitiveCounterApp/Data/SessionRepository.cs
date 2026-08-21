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

            return await query
                .OrderByDescending(s => s.SessionDate)
                .ToListAsync();
        }

        public async Task<Session?> GetAsync(int id)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.Sessions
                .AsNoTracking()
                .Include(s => s.Game)
                .Include(s => s.SessionPlayers)
                    .ThenInclude(sp => sp.Player)
                .FirstOrDefaultAsync(s => s.ID == id);
        }

        public async Task SaveItemAsync(Session session)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            if (session.ID == 0)
            {
                foreach (var sessionPlayer in session.SessionPlayers)
                {
                    sessionPlayer.Session = null;
                    sessionPlayer.Player = null;
                }

                session.Game = null;
                db.Sessions.Add(session);
            }
            else
            {
                var existing = await db.Sessions
                    .Include(s => s.SessionPlayers)
                    .FirstOrDefaultAsync(s => s.ID == session.ID);

                if (existing is null)
                    return;

                existing.GameID = session.GameID;
                existing.SessionDate = session.SessionDate;
                existing.Notes = session.Notes;

                db.SessionPlayers.RemoveRange(existing.SessionPlayers);

                foreach (var sessionPlayer in session.SessionPlayers)
                {
                    existing.SessionPlayers.Add(new SessionPlayer
                    {
                        SessionID = existing.ID,
                        PlayerID = sessionPlayer.PlayerID,
                        Wins = sessionPlayer.Wins
                    });
                }
            }

            await db.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(Session session)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            db.Sessions.Remove(session);
            await db.SaveChangesAsync();
        }
    }
}
