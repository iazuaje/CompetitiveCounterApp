using CompetitiveCounterApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CompetitiveCounterApp.Data
{
    /// <summary>
    /// Categorías (etiquetas) de juegos. El nombre es único sin distinguir mayúsculas ni tildes:
    /// crear una que ya existe devuelve la existente.
    /// </summary>
    public class CategoryRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public CategoryRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<Category>> ListAsync()
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var categories = await db.Categories.AsNoTracking().ToListAsync();
            return categories
                .OrderBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        public async Task<Category> GetOrCreateAsync(string name)
        {
            var clean = Clean(name);

            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var existing = (await db.Categories.ToListAsync()).FirstOrDefault(c => SameName(c.Name, clean));
            if (existing is not null)
                return existing;

            var category = new Category { Name = clean };
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            return category;
        }

        public async Task RenameAsync(int categoryId, string name)
        {
            var clean = Clean(name);

            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var all = await db.Categories.ToListAsync();
            var category = all.FirstOrDefault(c => c.ID == categoryId)
                ?? throw new InvalidOperationException("La categoría no existe.");

            if (all.Any(c => c.ID != categoryId && SameName(c.Name, clean)))
                throw new InvalidOperationException("Ya existe una categoría con ese nombre.");

            category.Name = clean;
            await db.SaveChangesAsync();
        }

        /// <summary>Elimina la categoría; los juegos solo pierden la etiqueta.</summary>
        public async Task DeleteAsync(int categoryId)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var category = await db.Categories.FirstOrDefaultAsync(c => c.ID == categoryId);
            if (category is null)
                return;

            db.Categories.Remove(category);
            await db.SaveChangesAsync();
        }

        static string Clean(string name)
        {
            var clean = name?.Trim() ?? string.Empty;
            if (clean.Length == 0)
                throw new InvalidOperationException("El nombre de la categoría es requerido.");
            return clean;
        }

        static bool SameName(string a, string b) =>
            string.Compare(a, b, System.Globalization.CultureInfo.CurrentCulture,
                System.Globalization.CompareOptions.IgnoreCase | System.Globalization.CompareOptions.IgnoreNonSpace) == 0;
    }
}
