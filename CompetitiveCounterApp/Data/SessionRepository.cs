using CompetitiveCounterApp.Models;
using Microsoft.Data.Sqlite;

namespace CompetitiveCounterApp.Data
{
    public class SessionRepository
    {
        private readonly string _connectionString;

        public SessionRepository()
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
                );";
            command.ExecuteNonQuery();
        }

        public async Task<List<Session>> ListAsync(int? gameId = null)
        {
            var sessions = new List<Session>();

            await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                
                if (gameId.HasValue)
                {
                    command.CommandText = @"
                        SELECT s.ID, s.GameID, s.SessionDate, s.Notes,
                               g.Name, g.Icon, g.Description, g.CreatedDate
                        FROM Sessions s
                        INNER JOIN Games g ON s.GameID = g.ID
                        WHERE s.GameID = @gameId
                        ORDER BY s.SessionDate DESC";
                    command.Parameters.AddWithValue("@gameId", gameId.Value);
                }
                else
                {
                    command.CommandText = @"
                        SELECT s.ID, s.GameID, s.SessionDate, s.Notes,
                               g.Name, g.Icon, g.Description, g.CreatedDate
                        FROM Sessions s
                        INNER JOIN Games g ON s.GameID = g.ID
                        ORDER BY s.SessionDate DESC";
                }

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var session = new Session
                    {
                        ID = reader.GetInt32(0),
                        GameID = reader.GetInt32(1),
                        SessionDate = DateTime.Parse(reader.GetString(2)),
                        Notes = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        Game = new Game
                        {
                            ID = reader.GetInt32(1),
                            Name = reader.GetString(4),
                            Icon = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                            Description = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                            CreatedDate = DateTime.Parse(reader.GetString(7))
                        }
                    };

                    session.SessionPlayers = GetSessionPlayersBySessionId(session.ID, connection);
                    sessions.Add(session);
                }
            });

            return sessions;
        }

        public async Task<Session?> GetAsync(int id)
        {
            return await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT s.ID, s.GameID, s.SessionDate, s.Notes,
                           g.Name, g.Icon, g.Description, g.CreatedDate
                    FROM Sessions s
                    INNER JOIN Games g ON s.GameID = g.ID
                    WHERE s.ID = @id";
                command.Parameters.AddWithValue("@id", id);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var session = new Session
                    {
                        ID = reader.GetInt32(0),
                        GameID = reader.GetInt32(1),
                        SessionDate = DateTime.Parse(reader.GetString(2)),
                        Notes = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        Game = new Game
                        {
                            ID = reader.GetInt32(1),
                            Name = reader.GetString(4),
                            Icon = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                            Description = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                            CreatedDate = DateTime.Parse(reader.GetString(7))
                        }
                    };

                    session.SessionPlayers = GetSessionPlayersBySessionId(session.ID, connection);
                    return session;
                }

                return null;
            });
        }

        public async Task SaveItemAsync(Session session)
        {
            await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                using var transaction = connection.BeginTransaction();

                try
                {
                    var command = connection.CreateCommand();
                    command.Transaction = transaction;

                    if (session.ID == 0)
                    {
                        command.CommandText = @"
                            INSERT INTO Sessions (GameID, SessionDate, Notes)
                            VALUES (@gameId, @sessionDate, @notes);
                            SELECT last_insert_rowid();";
                        command.Parameters.AddWithValue("@gameId", session.GameID);
                        command.Parameters.AddWithValue("@sessionDate", session.SessionDate.ToString("o"));
                        command.Parameters.AddWithValue("@notes", session.Notes);

                        session.ID = Convert.ToInt32(command.ExecuteScalar());
                    }
                    else
                    {
                        command.CommandText = @"
                            UPDATE Sessions 
                            SET GameID = @gameId, SessionDate = @sessionDate, Notes = @notes
                            WHERE ID = @id";
                        command.Parameters.AddWithValue("@id", session.ID);
                        command.Parameters.AddWithValue("@gameId", session.GameID);
                        command.Parameters.AddWithValue("@sessionDate", session.SessionDate.ToString("o"));
                        command.Parameters.AddWithValue("@notes", session.Notes);

                        command.ExecuteNonQuery();

                        // Eliminar los SessionPlayers existentes antes de insertar los nuevos
                        command = connection.CreateCommand();
                        command.Transaction = transaction;
                        command.CommandText = "DELETE FROM SessionPlayers WHERE SessionID = @sessionId";
                        command.Parameters.AddWithValue("@sessionId", session.ID);
                        command.ExecuteNonQuery();
                    }

                    // Insertar SessionPlayers
                    foreach (var sessionPlayer in session.SessionPlayers)
                    {
                        command = connection.CreateCommand();
                        command.Transaction = transaction;
                        command.CommandText = @"
                            INSERT INTO SessionPlayers (SessionID, PlayerID, Wins)
                            VALUES (@sessionId, @playerId, @wins);";
                        command.Parameters.AddWithValue("@sessionId", session.ID);
                        command.Parameters.AddWithValue("@playerId", sessionPlayer.PlayerID);
                        command.Parameters.AddWithValue("@wins", sessionPlayer.Wins);
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            });
        }

        public async Task DeleteItemAsync(Session session)
        {
            await Task.Run(() =>
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Sessions WHERE ID = @id";
                command.Parameters.AddWithValue("@id", session.ID);
                command.ExecuteNonQuery();
            });
        }

        private List<SessionPlayer> GetSessionPlayersBySessionId(int sessionId, SqliteConnection connection)
        {
            var sessionPlayers = new List<SessionPlayer>();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT sp.ID, sp.SessionID, sp.PlayerID, sp.Wins,
                       p.Name, p.ColorHex
                FROM SessionPlayers sp
                INNER JOIN Players p ON sp.PlayerID = p.ID
                WHERE sp.SessionID = @sessionId";
            command.Parameters.AddWithValue("@sessionId", sessionId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                sessionPlayers.Add(new SessionPlayer
                {
                    ID = reader.GetInt32(0),
                    SessionID = reader.GetInt32(1),
                    PlayerID = reader.GetInt32(2),
                    Wins = reader.GetInt32(3),
                    Player = new Player
                    {
                        ID = reader.GetInt32(2),
                        Name = reader.GetString(4),
                        ColorHex = reader.GetString(5)
                    }
                });
            }

            return sessionPlayers;
        }
    }
}
