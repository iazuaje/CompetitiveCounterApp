using Fonts;
using System.Globalization;

namespace CompetitiveCounterApp.Converters;

public class SegmentItemGlyphConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        (value as string) switch
        {
            "Imagen" => FluentUI.image_add_24_regular,
            "Icono" => FluentUI.sticker_add_24_regular,
            _ => FluentUI.image_add_24_regular
        };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
