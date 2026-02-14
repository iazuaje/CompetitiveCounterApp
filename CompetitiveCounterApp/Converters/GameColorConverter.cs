using System.Globalization;

namespace CompetitiveCounterApp.Converters
{
    public class GameColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return color;
            }

            if (value is string colorHex)
            {
                try
                {
                    return Color.FromArgb(colorHex);
                }
                catch
                {
                    return Color.FromArgb("#E63946"); // Color por defecto
                }
            }

            return Color.FromArgb("#E63946"); // Color por defecto
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
