using Microsoft.EntityFrameworkCore;

namespace CompetitiveCounterApp.Data
{
    public class DatabaseInitializer
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public DatabaseInitializer(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public void Initialize()
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Database.Migrate();
        }
    }
}
