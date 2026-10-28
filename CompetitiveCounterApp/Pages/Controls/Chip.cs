using CommunityToolkit.Mvvm.Messaging;
using CompetitiveCounterApp.Messages;
using Microsoft.Maui.Controls.Shapes;

namespace CompetitiveCounterApp.Pages.Controls;

public enum ChipVariant
{
    /// <summary>Sobre fondos de la app (formularios): superficie secundaria; seleccionado = color de acento.</summary>
    Surface,

    /// <summary>Sobre la banda de color Primary: translúcido; seleccionado = superficie clara con texto Primary.</summary>
    OnPrimary
}

/// <summary>
/// Chip seleccionable (filtros y categorías). Seleccionado: fondo <see cref="AccentColor"/> y texto
/// <see cref="OnAccentColor"/>; sin seleccionar: superficie secundaria del tema con borde.
/// El tap y el long press se agregan desde afuera con <c>behaviors:AnimatedTap</c>.
/// </summary>
public class Chip : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(Chip), string.Empty, propertyChanged: OnVisualChanged);

    public static readonly BindableProperty GlyphProperty =
        BindableProperty.Create(nameof(Glyph), typeof(string), typeof(Chip), string.Empty, propertyChanged: OnVisualChanged);

    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(Chip), false, propertyChanged: OnVisualChanged);

    public static readonly BindableProperty AccentColorProperty =
        BindableProperty.Create(nameof(AccentColor), typeof(Color), typeof(Chip), null, propertyChanged: OnVisualChanged);

    public static readonly BindableProperty OnAccentColorProperty =
        BindableProperty.Create(nameof(OnAccentColor), typeof(Color), typeof(Chip), null, propertyChanged: OnVisualChanged);

    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(ChipVariant), typeof(Chip), ChipVariant.Surface, propertyChanged: OnVisualChanged);

    public ChipVariant Variant { get => (ChipVariant)GetValue(VariantProperty); set => SetValue(VariantProperty, value); }

    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public string Glyph { get => (string)GetValue(GlyphProperty); set => SetValue(GlyphProperty, value); }
    public bool IsSelected { get => (bool)GetValue(IsSelectedProperty); set => SetValue(IsSelectedProperty, value); }
    public Color? AccentColor { get => (Color?)GetValue(AccentColorProperty); set => SetValue(AccentColorProperty, value); }
    public Color? OnAccentColor { get => (Color?)GetValue(OnAccentColorProperty); set => SetValue(OnAccentColorProperty, value); }

    private readonly Border _border;
    private readonly Label _label;

    public Chip()
    {
        _label = new Label { FontSize = 14, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
        _border = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            StrokeThickness = 1,
            Padding = new Thickness(12, 6),
            InputTransparent = true,
            Content = _label
        };
        Content = _border;

        WeakReferenceMessenger.Default.Register<AppThemeChangedMessage>(this, static (r, _) => ((Chip)r).UpdateVisual());
        UpdateVisual();
    }

    static void OnVisualChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((Chip)bindable).UpdateVisual();

    private void UpdateVisual()
    {
        Color foreground;

        if (Variant == ChipVariant.OnPrimary)
        {
            // Contenido de la banda (blanco en claro, oscuro en oscuro); el chip elegido se "recorta" con la superficie.
            var onBand = ThemeColor("White", "PrimaryDarkText");
            foreground = IsSelected ? ThemeColor("Primary", "PrimaryDarkText") : onBand;
            _border.Background = IsSelected ? ThemeColor("LightBackground", "LightBackground") : onBand.WithAlpha(0.16f);
            _border.Stroke = IsSelected ? Colors.Transparent : onBand.WithAlpha(0.35f);
        }
        else
        {
            var accent = AccentColor ?? ThemeColor("Primary", "PrimaryDark");
            var onAccent = OnAccentColor ?? ThemeColor("White", "PrimaryDarkText");
            foreground = IsSelected ? onAccent : ThemeColor("DarkOnLightBackground", "LightOnDarkBackground");
            _border.Background = IsSelected ? accent : ThemeColor("LightSecondaryBackground", "DarkSecondaryBackground");
            _border.Stroke = IsSelected ? accent : ThemeColor("Gray300", "Gray600");
        }

        var text = new FormattedString();
        if (!string.IsNullOrEmpty(Glyph))
        {
            text.Spans.Add(new Span { Text = Glyph, FontFamily = FluentUI.FontFamily, FontSize = 15, TextColor = foreground });
            text.Spans.Add(new Span { Text = " " });
        }
        text.Spans.Add(new Span
        {
            Text = Text,
            TextColor = foreground,
            FontAttributes = IsSelected ? FontAttributes.Bold : FontAttributes.None
        });
        _label.FormattedText = text;

        SemanticProperties.SetDescription(this, IsSelected ? $"{Text}, seleccionado" : Text);
    }

    private static Color ThemeColor(string lightKey, string darkKey)
    {
        var app = Application.Current;
        var key = app?.RequestedTheme == AppTheme.Dark ? darkKey : lightKey;
        return app?.Resources.TryGetValue(key, out var value) == true && value is Color color ? color : Colors.Gray;
    }
}
