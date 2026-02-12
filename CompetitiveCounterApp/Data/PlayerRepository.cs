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

        public async Task<List<Player>> ListAsync(int gameId)
        {
            var players = new List<Player>();

            await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

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
            });

            return players;
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
                        INSERT INTO Players (GameID, Name, WinCount, ColorHex)
                        VALUES (@gameId, @name, @winCount, @colorHex);
                        SELECT last_insert_rowid();";
                    command.Parameters.AddWithValue("@gameId", player.GameID);
                    command.Parameters.AddWithValue("@name", player.Name);
                    command.Parameters.AddWithValue("@winCount", player.WinCount);
                    command.Parameters.AddWithValue("@colorHex", player.ColorHex);

                    player.ID = Convert.ToInt32(command.ExecuteScalar());
                }
                else
                {
                    command.CommandText = @"
                        UPDATE Players 
                        SET Name = @name, WinCount = @winCount, ColorHex = @colorHex
                        WHERE ID = @id";
                    command.Parameters.AddWithValue("@id", player.ID);
                    command.Parameters.AddWithValue("@name", player.Name);
                    command.Parameters.AddWithValue("@winCount", player.WinCount);
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