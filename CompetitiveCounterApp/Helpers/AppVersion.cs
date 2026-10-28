namespace CompetitiveCounterApp.Helpers;

/// <summary>Versión de la app para mostrar al pie del flyout y en Respaldo (ayuda a saber de qué versión es un respaldo o un reporte).</summary>
public static class AppVersion
{
    public static string Text => $"Versión {AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})";
}
