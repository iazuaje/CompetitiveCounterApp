namespace CompetitiveCounterApp.Helpers;

/// <summary>
/// Tema elegido en el flyout, recordado entre sesiones de la app. Sin elección guardada se sigue el del sistema.
/// </summary>
public static class ThemePreference
{
    const string Key = "app.theme";

    public static AppTheme Current
    {
        get => (AppTheme)Preferences.Default.Get(Key, (int)AppTheme.Unspecified);
        set => Preferences.Default.Set(Key, (int)value);
    }
}
