namespace CompetitiveCounterApp.Models;

public readonly record struct ThemeColorPair(string LightThemeHex, string DarkThemeHex)
{
    /// <summary>Factor de luminosidad del toolbar respecto al color del juego (~28% más oscuro).</summary>
    private const float ToolbarLuminosityFactor = 0.72f;

    public Color LightThemeColor => Color.FromArgb(LightThemeHex);
    public Color DarkThemeColor => Color.FromArgb(DarkThemeHex);

    public Color CurrentColor => IsDarkTheme ? DarkThemeColor : LightThemeColor;

    /// <summary>Color del juego oscurecido para barra Shell / toolbar.</summary>
    public Color ToolbarColor => Darken(CurrentColor, ToolbarLuminosityFactor);

    public Color SurfaceColor => IsDarkTheme
        ? CurrentColor
        : GetResourceColor("LightBackground", Colors.White);

    public Color OnSurfaceColor => IsDarkTheme ? Colors.White : CurrentColor;

    private static bool IsDarkTheme => (Application.Current?.RequestedTheme ?? AppTheme.Light) == AppTheme.Dark;

    private static Color GetResourceColor(string key, Color fallback) =>
        Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color
            ? color
            : fallback;

    private static Color Darken(Color color, float luminosityFactor)
    {
        var luminosity = Math.Clamp(color.GetLuminosity() * luminosityFactor, 0f, 1f);
        return color.WithLuminosity(luminosity);
    }
}
