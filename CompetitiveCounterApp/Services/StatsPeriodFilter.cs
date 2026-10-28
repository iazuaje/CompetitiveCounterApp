using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.Services;

public enum StatsPeriod
{
    All,
    Year,
    Month
}

/// <summary>
/// Recorte de sesiones por período para las pantallas de estadísticas. Se aplica antes de los calculadores:
/// rankings, rachas y récords se recalculan solo con las sesiones del período (por <c>SessionDate</c>, hora local).
/// </summary>
public static class StatsPeriodFilter
{
    public static List<Session> Apply(IEnumerable<Session> sessions, StatsPeriod period, DateTime now) => period switch
    {
        StatsPeriod.Month => sessions.Where(s => s.SessionDate.Year == now.Year && s.SessionDate.Month == now.Month).ToList(),
        StatsPeriod.Year => sessions.Where(s => s.SessionDate.Year == now.Year).ToList(),
        _ => sessions.ToList()
    };

    public static string Label(StatsPeriod period) => period switch
    {
        StatsPeriod.Month => "Este mes",
        StatsPeriod.Year => "Este año",
        _ => "Todo"
    };

    /// <summary>Texto del estado vacío: con un período elegido, aclara que no hubo sesiones en él.</summary>
    public static string EmptyText(StatsPeriod period, string allTimeText) => period switch
    {
        StatsPeriod.Month => "No hay sesiones con victorias este mes.",
        StatsPeriod.Year => "No hay sesiones con victorias este año.",
        _ => allTimeText
    };
}
