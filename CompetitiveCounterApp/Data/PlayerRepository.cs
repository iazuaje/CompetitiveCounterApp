using CompetitiveCounterApp.Models;
using Microsoft.Data.Sqlite;

namespace CompetitiveCounterApp.Data
{
    public class PlayerRepository
    {
        private readonly string _connectionString;

        public PlayerRepository()
        {
            _connectionString = Constants.DatabasePath;
        }

        public async Task<List<Player>> ListAsync()
        {
            var players = new List<Player>();

            await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT ID, Name, ColorHex FROM Players ORDER BY Name";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    players.Add(new Player
                    {
                        ID = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        ColorHex = reader.GetString(2)
                    });
                }
            });

            return players;
        }

        public async Task<Player?> GetAsync(int id)
        {
            return await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT ID, Name, ColorHex FROM Players WHERE ID = @id";
                command.Parameters.AddWithValue("@id", id);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new Player
                    {
                        ID = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        ColorHex = reader.GetString(2)
                    };
                }

                return null;
            });
        }

        public async Task SaveItemAsync(Player player)
        {
            await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();

                if (player.ID == 0)
                {
                    command.CommandText = @"
                        INSERT INTO Players (Name, ColorHex)
                        VALUES (@name, @colorHex);
                        SELECT last_insert_rowid();";
                    command.Parameters.AddWithValue("@name", player.Name);
                    command.Parameters.AddWithValue("@colorHex", player.ColorHex);

                    player.ID = Convert.ToInt32(command.ExecuteScalar());
                }
                else
                {
                    command.CommandText = @"
                        UPDATE Players 
                        SET Name = @name, ColorHex = @colorHex
                        WHERE ID = @id";
                    command.Parameters.AddWithValue("@id", player.ID);
                    command.Parameters.AddWithValue("@name", player.Name);
                    command.Parameters.AddWithValue("@colorHex", player.ColorHex);

                    command.ExecuteNonQuery();
                }
            });
        }

        public async Task DeleteItemAsync(Player player)
        {
            await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Players WHERE ID = @id";
                command.Parameters.AddWithValue("@id", player.ID);
                command.ExecuteNonQuery();
            });
        }
    }
}