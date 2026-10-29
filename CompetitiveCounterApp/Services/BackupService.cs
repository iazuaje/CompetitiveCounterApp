using System.IO.Compression;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CompetitiveCounterApp.Services;

/// <summary>
/// Respaldo completo de los datos en un .zip: la base SQLite (copiada con la API de backup de SQLite, consistente
/// aunque haya escrituras pendientes) y las imágenes de los juegos. Restaurar reemplaza todos los datos actuales.
/// </summary>
public class BackupService
{
    const string DatabaseEntry = "database.db3";
    const string ImagesPrefix = "GameImages/";
    const string ManifestEntry = "backup.json";
    const int FormatVersion = 1;

    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public BackupService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>Crea el respaldo en la caché y devuelve su ruta (se comparte o se guarda desde ahí).</summary>
    public async Task<string> CreateAsync()
    {
        var zipPath = Path.Combine(FileSystem.CacheDirectory, $"CompetitiveCounter-respaldo-{DateTime.Now:yyyyMMdd-HHmm}.zip");
        var databaseCopy = Path.Combine(FileSystem.CacheDirectory, "backup-database.db3");

        await Task.Run(() =>
        {
            DeleteIfExists(zipPath);
            DeleteIfExists(databaseCopy);

            using (var source = new SqliteConnection(Constants.DatabasePath))
            using (var destination = new SqliteConnection($"Data Source={databaseCopy};Pooling=False"))
            {
                source.Open();
                destination.Open();
                source.BackupDatabase(destination);
            }

            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(databaseCopy, DatabaseEntry);

                if (Directory.Exists(Constants.GameImagesDirectory))
                {
                    foreach (var image in Directory.EnumerateFiles(Constants.GameImagesDirectory))
                        zip.CreateEntryFromFile(image, ImagesPrefix + Path.GetFileName(image));
                }

                using var manifest = new StreamWriter(zip.CreateEntry(ManifestEntry).Open());
                manifest.Write(JsonSerializer.Serialize(new { version = FormatVersion, createdAt = DateTime.Now }));
            }

            DeleteIfExists(databaseCopy);
        });

        return zipPath;
    }

    /// <summary>
    /// Reemplaza la base y las imágenes por las del respaldo. Valida el archivo antes de tocar nada; después
    /// aplica migraciones (respaldos de versiones anteriores) y corrige las rutas de imagen al directorio actual.
    /// </summary>
    public async Task RestoreAsync(Stream backup)
    {
        var workDirectory = Path.Combine(FileSystem.CacheDirectory, "restore");
        var databaseCopy = Path.Combine(workDirectory, DatabaseEntry);
        var imagesCopy = Path.Combine(workDirectory, "GameImages");

        await Task.Run(() =>
        {
            if (Directory.Exists(workDirectory))
                Directory.Delete(workDirectory, recursive: true);
            Directory.CreateDirectory(imagesCopy);

            using (var zip = new ZipArchive(backup, ZipArchiveMode.Read))
            {
                var databaseEntry = zip.GetEntry(DatabaseEntry)
                    ?? throw new InvalidOperationException("El archivo elegido no es un respaldo de la app.");
                databaseEntry.ExtractToFile(databaseCopy);

                foreach (var entry in zip.Entries.Where(e => e.FullName.StartsWith(ImagesPrefix) && e.Name.Length > 0))
                    entry.ExtractToFile(Path.Combine(imagesCopy, entry.Name));
            }

            ValidateDatabase(databaseCopy);

            // Sin conexiones abiertas a la base actual antes de reemplazarla (ni su WAL, que ya no corresponde).
            SqliteConnection.ClearAllPools();
            DeleteIfExists(Constants.DatabaseFilePath + "-wal");
            DeleteIfExists(Constants.DatabaseFilePath + "-shm");
            File.Copy(databaseCopy, Constants.DatabaseFilePath, overwrite: true);

            if (Directory.Exists(Constants.GameImagesDirectory))
                Directory.Delete(Constants.GameImagesDirectory, recursive: true);
            Directory.Move(imagesCopy, Constants.GameImagesDirectory);

            Directory.Delete(workDirectory, recursive: true);
        });

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        await db.Database.MigrateAsync();

        // Las rutas son absolutas: se reubican en el directorio de imágenes de este dispositivo.
        foreach (var game in await db.Games.Where(g => g.ImagePath != string.Empty).ToListAsync())
        {
            var image = Path.Combine(Constants.GameImagesDirectory, Path.GetFileName(game.ImagePath));
            game.ImagePath = File.Exists(image) ? image : string.Empty;
        }
        await db.SaveChangesAsync();
    }

    /// <summary>Borra la base y las imágenes de los juegos, y deja la app con una base vacía y migrada.</summary>
    public async Task DeleteAllAsync()
    {
        await Task.Run(() =>
        {
            SqliteConnection.ClearAllPools();
            DeleteIfExists(Constants.DatabaseFilePath);
            DeleteIfExists(Constants.DatabaseFilePath + "-wal");
            DeleteIfExists(Constants.DatabaseFilePath + "-shm");

            if (Directory.Exists(Constants.GameImagesDirectory))
                Directory.Delete(Constants.GameImagesDirectory, recursive: true);
        });

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        await db.Database.MigrateAsync();
    }

    static void ValidateDatabase(string path)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={path};Mode=ReadOnly;Pooling=False");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name IN ('Games', 'Players', 'Sessions')";
            if (Convert.ToInt32(command.ExecuteScalar()) != 3)
                throw new InvalidOperationException("El respaldo no contiene los datos de la app.");
        }
        catch (SqliteException)
        {
            throw new InvalidOperationException("El respaldo está dañado y no se puede leer.");
        }
    }

    static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
}
