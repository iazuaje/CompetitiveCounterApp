namespace CompetitiveCounterApp.Models;

public readonly record struct ThemeColorPair(string LightThemeHex, string DarkThemeHex)
{
    public Color LightThemeColor => Color.FromArgb(LightThemeHex);
    public Color DarkThemeColor => Color.FromArgb(DarkThemeHex);

    public Color CurrentColor =>
        (Application.Current?.RequestedTheme ?? AppTheme.Light) == AppTheme.Dark
            ? DarkThemeColor
            : LightThemeColor;
}
