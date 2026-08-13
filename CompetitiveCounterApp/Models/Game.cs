using CommunityToolkit.Mvvm.ComponentModel;

namespace CompetitiveCounterApp.Models
{
    public partial class Game : ObservableObject
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ColorLight { get; set; } = "#E63946";
        public string ColorDark { get; set; } = "#FF5964";
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public ThemeColorPair ThemeColors => new(ColorLight, ColorDark);

        public Color GameColorLight => ThemeColors.LightThemeColor;
        public Color GameColorDark => ThemeColors.DarkThemeColor;
        public Color CurrentGameColor => ThemeColors.CurrentColor;

        public Color IconSurfaceColor => IsDarkTheme
            ? CurrentGameColor
            : GetResourceColor("LightBackground", Colors.White);

        public Color IconGlyphColor => IsDarkTheme ? Colors.White : CurrentGameColor;

        private static bool IsDarkTheme => (Application.Current?.RequestedTheme ?? AppTheme.Light) == AppTheme.Dark;

        private static Color GetResourceColor(string key, Color fallback) =>
            Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color
                ? color
                : fallback;

        public ImageSource GameImage => !string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath) 
            ? ImageSource.FromFile(ImagePath) 
            : null;

        public void NotifyThemeChanged()
        {
            OnPropertyChanged(nameof(CurrentGameColor));
            OnPropertyChanged(nameof(IconSurfaceColor));
            OnPropertyChanged(nameof(IconGlyphColor));
        }

        public override string ToString() => Name;
    }
}