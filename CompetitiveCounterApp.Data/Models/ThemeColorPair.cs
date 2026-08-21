namespace CompetitiveCounterApp.Models;

public readonly record struct ThemeColorPair(string LightThemeHex, string DarkThemeHex)
{
    public Color LightThemeColor => Color.FromArgb(LightThemeHex);
    public Color DarkThemeColor => Color.FromArgb(DarkThemeHex);

    public Color CurrentColor => IsDarkTheme ? DarkThemeColor : LightThemeColor;

    public Color SurfaceColor => IsDarkTheme
        ? CurrentColor
        : GetResourceColor("LightBackground", Colors.White);

    public Color OnSurfaceColor => IsDarkTheme ? Colors.White : CurrentColor;

    private static bool IsDarkTheme => (Application.Current?.RequestedTheme ?? AppTheme.Light) == AppTheme.Dark;

    private static Color GetResourceColor(string key, Color fallback) =>
        Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color
            ? color
            : fallback;
}
