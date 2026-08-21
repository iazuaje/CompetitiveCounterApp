using System.Globalization;

namespace CompetitiveCounterApp.Converters;

public class SegmentForegroundConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[1] is not string text)
            return Colors.Black;

        var selectedIndex = values[0] is int index ? index : -1;
        var segmentIndex = text switch
        {
            "Imagen" => 0,
            "Icono" => 1,
            _ => -1
        };

        if (selectedIndex == segmentIndex)
            return Colors.White;

        var app = Application.Current;
        if (app is null)
            return Colors.Black;

        var key = app.RequestedTheme == AppTheme.Dark ? "LightOnDarkBackground" : "DarkOnLightBackground";
        return app.Resources.TryGetValue(key, out var resource) && resource is Color color
            ? color
            : Colors.Black;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
