using Microsoft.Data.Sqlite;

namespace CompetitiveCounterApp.Data
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer()
        {
            _connectionString = Constants.DatabasePath;
        }

        public void Initialize()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = ON;";
                pragma.ExecuteNonQuery();
            }

            CreateTables(connection);
            ApplyMigrations(connection);
        }

        private static void CreateTables(SqliteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Games (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Icon TEXT,
                    Description TEXT,
                    ColorLight TEXT DEFAULT '#E63946',
                    ColorDark TEXT DEFAULT '#FF5964',
                    CreatedDate TEXT
                );

                CREATE TABLE IF NOT EXISTS Players (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    ColorHex TEXT DEFAULT '#FF6B6B'
                );

                CREATE TABLE IF NOT EXISTS Sessions (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    GameID INTEGER NOT NULL,
                    SessionDate TEXT NOT NULL,
                    Notes TEXT,
                    FOREIGN KEY (GameID) REFERENCES Games(ID) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS SessionPlayers (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    SessionID INTEGER NOT NULL,
                    PlayerID INTEGER NOT NULL,
                    Wins INTEGER DEFAULT 0,
                    FOREIGN KEY (SessionID) REFERENCES Sessions(ID) ON DELETE CASCADE,
                    FOREIGN KEY (PlayerID) REFERENCES Players(ID) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS Project (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    Icon TEXT NOT NULL,
                    CategoryID INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Task (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    IsCompleted INTEGER NOT NULL,
                    ProjectID INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Category (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Color TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Tag (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Color TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS ProjectsTags (
                    ProjectID INTEGER NOT NULL,
                    TagID INTEGER NOT NULL,
                    PRIMARY KEY(ProjectID, TagID)
                );
            ";
            command.ExecuteNonQuery();
        }

        private static void ApplyMigrations(SqliteConnection connection)
        {
            // Centraliza aquí cualquier ALTER/índices/etc.
            EnsureColumnExists(connection, "Games", "ColorLight", "TEXT DEFAULT '#E63946'");
            EnsureColumnExists(connection, "Games", "ColorDark", "TEXT DEFAULT '#FF5964'");
            EnsureColumnExists(connection, "Players", "ColorHex", "TEXT DEFAULT '#FF6B6B'");
        }

        private static void EnsureColumnExists(SqliteConnection connection, string tableName, string columnName, string columnDefinition)
        {
            using var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = $"PRAGMA table_info({tableName});";

            using var reader = checkCommand.ExecuteReader();
            while (reader.Read())
            {
                var existingColumnName = reader.GetString(1);
                if (string.Equals(existingColumnName, columnName, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            using var alterCommand = connection.CreateCommand();
            alterCommand.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition};";
            alterCommand.ExecuteNonQuery();
        }
    }
}
