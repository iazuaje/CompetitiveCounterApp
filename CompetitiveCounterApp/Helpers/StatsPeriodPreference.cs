namespace CompetitiveCounterApp.Helpers;

/// <summary>
/// Período elegido en las estadísticas, compartido entre pantallas (al pasar de las generales a un jugador
/// se mantiene) y recordado entre sesiones de la app.
/// </summary>
public static class StatsPeriodPreference
{
    const string Key = "stats.period";

    public static StatsPeriod Current
    {
        get => (StatsPeriod)Preferences.Default.Get(Key, (int)StatsPeriod.All);
        set => Preferences.Default.Set(Key, (int)value);
    }
}
