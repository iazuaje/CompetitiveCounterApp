using CompetitiveCounterApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CompetitiveCounterApp.Data
{
    public class GameRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public GameRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<Game>> ListAsync()
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.Games
                .AsNoTracking()
                .OrderByDescending(g => g.CreatedDate)
                .ToListAsync();
        }

        public async Task<Game?> GetAsync(int id)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.Games
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.ID == id);
        }

        public async Task SaveItemAsync(Game game)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            if (game.ID == 0)
                db.Games.Add(game);
            else
                db.Games.Update(game);

            await db.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(Game game)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            db.Games.Remove(game);
            await db.SaveChangesAsync();
        }
    }
}
