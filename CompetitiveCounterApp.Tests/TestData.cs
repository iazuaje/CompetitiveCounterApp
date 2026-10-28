using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.Tests;

/// <summary>Armado breve de jugadores y sesiones (con SessionPlayers y Player cargados, como los repositorios).</summary>
internal static class TestData
{
    static int _nextSessionId = 1;

    public static readonly Player Ana = new() { ID = 1, Name = "Ana" };
    public static readonly Player Beto = new() { ID = 2, Name = "Beto" };
    public static readonly Player Carla = new() { ID = 3, Name = "Carla" };

    /// <summary>Sesión cerrada (o activa con <paramref name="active"/>) con las victorias de cada jugador.</summary>
    public static Session Session(DateTime date, params (Player Player, int Wins)[] results) =>
        Session(date, active: false, results);

    public static Session Session(DateTime date, bool active, params (Player Player, int Wins)[] results)
    {
        var id = _nextSessionId++;
        return new Session
        {
            ID = id,
            GameID = 1,
            SessionDate = date,
            ClosedAt = active ? null : date.AddHours(1),
            SessionPlayers = results
                .Select(r => new SessionPlayer { SessionID = id, PlayerID = r.Player.ID, Player = r.Player, Wins = r.Wins })
                .ToList()
        };
    }

    public static DateTime Day(int month, int day, int year = 2026) => new(year, month, day, 20, 0, 0);
}
