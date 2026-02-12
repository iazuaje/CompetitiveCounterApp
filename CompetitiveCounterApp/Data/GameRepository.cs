using CompetitiveCounterApp.Models;
using Microsoft.Data.Sqlite;

namespace CompetitiveCounterApp.Data
{
    public class GameRepository
    {
        private readonly string _connectionString;

        public GameRepository()
        {
            _connectionString = Constants.DatabasePath;
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Games (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Icon TEXT,
                    Description TEXT,
                    CreatedDate TEXT
                );

                CREATE TABLE IF NOT EXISTS Players (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    GameID INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    WinCount INTEGER DEFAULT 0,
                    ColorHex TEXT DEFAULT '#FF6B6B',
                    FOREIGN KEY (GameID) REFERENCES Games(ID) ON DELETE CASCADE
                );";
            command.ExecuteNonQuery();
        }

        public async Task<List<Game>> ListAsync()
        {
            var games = new List<Game>();

            await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT ID, Name, Icon, Description, CreatedDate FROM Games ORDER BY CreatedDate DESC";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var game = new Game
                    {
                        ID = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Icon = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        Description = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        CreatedDate = reader.IsDBNull(4) ? DateTime.Now : DateTime.Parse(reader.GetString(4))
                    };

                    game.Players = GetPlayersByGameId(game.ID, connection);
                    games.Add(game);
                }
            });

            return games;
        }

        public async Task<Game?> GetAsync(int id)
        {
            return await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT ID, Name, Icon, Description, CreatedDate FROM Games WHERE ID = @id";
                command.Parameters.AddWithValue("@id", id);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var game = new Game
                    {
                        ID = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Icon = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        Description = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        CreatedDate = reader.IsDBNull(4) ? DateTime.Now : DateTime.Parse(reader.GetString(4))
                    };

                    game.Players = GetPlayersByGameId(game.ID, connection);
                    return game;
                }

                return null;
            });
        }

        public async Task SaveItemAsync(Game game)
        {
            await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();

                if (game.ID == 0)
                {
                    command.CommandText = @"
                        INSERT INTO Games (Name, Icon, Description, CreatedDate)
                        VALUES (@name, @icon, @description, @createdDate);
                        SELECT last_insert_rowid();";
                    command.Parameters.AddWithValue("@name", game.Name);
                    command.Parameters.AddWithValue("@icon", game.Icon);
                    command.Parameters.AddWithValue("@description", game.Description);
                    command.Parameters.AddWithValue("@createdDate", game.CreatedDate.ToString("o"));

                    game.ID = Convert.ToInt32(command.ExecuteScalar());
                }
                else
                {
                    command.CommandText = @"
                        UPDATE Games 
                        SET Name = @name, Icon = @icon, Description = @description, CreatedDate = @createdDate
                        WHERE ID = @id";
                    command.Parameters.AddWithValue("@id", game.ID);
                    command.Parameters.AddWithValue("@name", game.Name);
                    command.Parameters.AddWithValue("@icon", game.Icon);
                    command.Parameters.AddWithValue("@description", game.Description);
                    command.Parameters.AddWithValue("@createdDate", game.CreatedDate.ToString("o"));

                    command.ExecuteNonQuery();
                }
            });
        }

        public async Task DeleteItemAsync(Game game)
        {
            await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Games WHERE ID = @id";
                command.Parameters.AddWithValue("@id", game.ID);
                command.ExecuteNonQuery();
            });
        }

        private List<Player> GetPlayersByGameId(int gameId, SqliteConnection connection)
        {
            var players = new List<Player>();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT ID, GameID, Name, WinCount, ColorHex FROM Players WHERE GameID = @gameId";
            command.Parameters.AddWithValue("@gameId", gameId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                players.Add(new Player
                {
                    ID = reader.GetInt32(0),
                    GameID = reader.GetInt32(1),
                    Name = reader.GetString(2),
                    WinCount = reader.GetInt32(3),
                    ColorHex = reader.GetString(4)
                });
            }

            return players;
        }
    }
}