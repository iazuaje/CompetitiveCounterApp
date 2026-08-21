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
            db.Players.Remove(player);
            await db.SaveChangesAsync();
        }
    }
}
