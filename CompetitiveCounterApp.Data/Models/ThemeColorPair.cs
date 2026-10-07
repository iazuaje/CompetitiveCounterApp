namespace CompetitiveCounterApp.Models;

public readonly record struct ThemeColorPair(string LightThemeHex, string DarkThemeHex)
{
    /// <summary>Factor de luminosidad del toolbar respecto al color del juego (~28% más oscuro).</summary>
    private const float ToolbarLuminosityFactor = 0.72f;

    /// <summary>Texto oscuro sobre colores claros; mismo valor que PrimaryDarkText en Colors.xaml.</summary>
    private static readonly Color DarkText = Color.FromArgb("#2F221C");

    /// <summary>
    /// Luminancia relativa a partir de la cual el texto oscuro contrasta más que el blanco
    /// (punto de igual contraste WCAG entre blanco y <see cref="DarkText"/>).
    /// </summary>
    private const double DarkTextLuminanceThreshold = 0.21;

    public Color LightThemeColor => Color.FromArgb(LightThemeHex);
    public Color DarkThemeColor => Color.FromArgb(DarkThemeHex);

    public Color CurrentColor => IsDarkTheme ? DarkThemeColor : LightThemeColor;

    /// <summary>Texto e iconos sobre <see cref="CurrentColor"/>.</summary>
    public Color OnColor => ContrastingTextColor(CurrentColor);

    /// <summary>Color del juego oscurecido para barra Shell / toolbar.</summary>
    public Color ToolbarColor => Darken(CurrentColor, ToolbarLuminosityFactor);

    /// <summary>Título e iconos sobre <see cref="ToolbarColor"/>.</summary>
    public Color OnToolbarColor => ContrastingTextColor(ToolbarColor);

    /// <summary>Complementario del color actual (matiz +180°) para resaltar bordes.</summary>
    public Color ComplementaryColor
    {
        get
        {
            var color = CurrentColor;
            var hue = (color.GetHue() + 0.5f) % 1f;
            return Color.FromHsla(hue, color.GetSaturation(), color.GetLuminosity(), color.Alpha);
        }
    }

    /// <summary>Fondo neutro del tema (círculos de icono, cabeceras) que lleva el color encima.</summary>
    public Color SurfaceColor => IsDarkTheme
        ? GetResourceColor("DarkBackground", Colors.Black)
        : GetResourceColor("LightBackground", Colors.White);

    /// <summary>Color del juego sobre <see cref="SurfaceColor"/>: claro en oscuro y viceversa.</summary>
    public Color OnSurfaceColor => CurrentColor;

    /// <summary>Blanco u oscuro, el que contraste más con <paramref name="background"/>.</summary>
    public static Color ContrastingTextColor(Color background) =>
        RelativeLuminance(background) > DarkTextLuminanceThreshold ? DarkText : Colors.White;

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

    private static double RelativeLuminance(Color color)
    {
        static double Channel(float c) => c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        return 0.2126 * Channel(color.Red) + 0.7152 * Channel(color.Green) + 0.0722 * Channel(color.Blue);
    }
}
