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
        }

        public async Task<List<Game>> ListAsync()
        {
            var games = new List<Game>();

            await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT ID, Name, Icon, Description, ColorLight, ColorDark, CreatedDate FROM Games ORDER BY CreatedDate DESC";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var game = new Game
                    {
                        ID = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Icon = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        Description = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        ColorLight = reader.IsDBNull(4) ? "#E63946" : reader.GetString(4),
                        ColorDark = reader.IsDBNull(5) ? "#FF5964" : reader.GetString(5),
                        CreatedDate = reader.IsDBNull(6) ? DateTime.Now : DateTime.Parse(reader.GetString(6))
                    };

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
                command.CommandText = "SELECT ID, Name, Icon, Description, ColorLight, ColorDark, CreatedDate FROM Games WHERE ID = @id";
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
                        ColorLight = reader.IsDBNull(4) ? "#E63946" : reader.GetString(4),
                        ColorDark = reader.IsDBNull(5) ? "#FF5964" : reader.GetString(5),
                        CreatedDate = reader.IsDBNull(6) ? DateTime.Now : DateTime.Parse(reader.GetString(6))
                    };

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
                        INSERT INTO Games (Name, Icon, Description, ColorLight, ColorDark, CreatedDate)
                        VALUES (@name, @icon, @description, @colorLight, @colorDark, @createdDate);
                        SELECT last_insert_rowid();";
                    command.Parameters.AddWithValue("@name", game.Name);
                    command.Parameters.AddWithValue("@icon", game.Icon);
                    command.Parameters.AddWithValue("@description", game.Description);
                    command.Parameters.AddWithValue("@colorLight", game.ColorLight);
                    command.Parameters.AddWithValue("@colorDark", game.ColorDark);
                    command.Parameters.AddWithValue("@createdDate", game.CreatedDate.ToString("o"));

                    game.ID = Convert.ToInt32(command.ExecuteScalar());
                }
                else
                {
                    command.CommandText = @"
                        UPDATE Games 
                        SET Name = @name, Icon = @icon, Description = @description, 
                            ColorLight = @colorLight, ColorDark = @colorDark, CreatedDate = @createdDate
                        WHERE ID = @id";
                    command.Parameters.AddWithValue("@id", game.ID);
                    command.Parameters.AddWithValue("@name", game.Name);
                    command.Parameters.AddWithValue("@icon", game.Icon);
                    command.Parameters.AddWithValue("@description", game.Description);
                    command.Parameters.AddWithValue("@colorLight", game.ColorLight);
                    command.Parameters.AddWithValue("@colorDark", game.ColorDark);
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
    }
}