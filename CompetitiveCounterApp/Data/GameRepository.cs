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

        /// <summary>Juegos con sus categorías, en el orden manual (empate: el más nuevo primero).</summary>
        public async Task<List<Game>> ListAsync()
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.Games
                .AsNoTracking()
                .Include(g => g.Categories)
                .OrderBy(g => g.SortOrder)
                .ThenByDescending(g => g.CreatedDate)
                .ToListAsync();
        }

        public async Task<Game?> GetAsync(int id)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.Games
                .AsNoTracking()
                .Include(g => g.Categories)
                .FirstOrDefaultAsync(g => g.ID == id);
        }

        /// <summary>
        /// Guarda los datos del juego. No toca sus categorías (ver <see cref="SetCategoriesAsync"/>).
        /// Un juego nuevo queda primero en el orden manual.
        /// </summary>
        public async Task SaveItemAsync(Game game)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            // Las categorías vienen de otro contexto: sin desacoplarlas, Update intentaría insertar la relación.
            var categories = game.Categories;
            game.Categories = [];

            try
            {
                if (game.ID == 0)
                {
                    game.SortOrder = await db.Games.AnyAsync()
                        ? await db.Games.MinAsync(g => g.SortOrder) - 1
                        : 0;
                    db.Games.Add(game);
                }
                else
                {
                    db.Games.Update(game);
                }

                await db.SaveChangesAsync();
            }
            finally
            {
                game.Categories = categories;
            }
        }

        /// <summary>Reemplaza las categorías del juego por las indicadas.</summary>
        public async Task SetCategoriesAsync(int gameId, IEnumerable<int> categoryIds)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            var game = await db.Games
                .Include(g => g.Categories)
                .FirstOrDefaultAsync(g => g.ID == gameId)
                ?? throw new InvalidOperationException("El juego no existe.");

            var ids = categoryIds.Distinct().ToList();
            var categories = await db.Categories.Where(c => ids.Contains(c.ID)).ToListAsync();

            game.Categories.Clear();
            game.Categories.AddRange(categories);
            await db.SaveChangesAsync();
        }

        /// <summary>Guarda el orden manual: la posición en la lista pasa a ser el SortOrder.</summary>
        public async Task SaveOrderAsync(IReadOnlyList<int> orderedGameIds)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            var games = await db.Games.ToDictionaryAsync(g => g.ID);
            for (var i = 0; i < orderedGameIds.Count; i++)
            {
                if (games.TryGetValue(orderedGameIds[i], out var game))
                    game.SortOrder = i;
            }

            await db.SaveChangesAsync();
        }

        public async Task SetArchivedAsync(int gameId, bool archived)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            await db.Games
                .Where(g => g.ID == gameId)
                .ExecuteUpdateAsync(s => s.SetProperty(g => g.IsArchived, archived));
        }

        public async Task DeleteItemAsync(Game game)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var existing = await db.Games.FirstOrDefaultAsync(g => g.ID == game.ID);
            if (existing is null)
                return;

            db.Games.Remove(existing);
            await db.SaveChangesAsync();
        }
    }
}
