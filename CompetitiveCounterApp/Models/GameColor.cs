using CommunityToolkit.Mvvm.ComponentModel;

namespace CompetitiveCounterApp.Models
{
    public partial class GameColor : ObservableObject
    {
        public string Name { get; set; } = string.Empty;
        public string ColorLight { get; set; } = string.Empty;
        public string ColorDark { get; set; } = string.Empty;

        [ObservableProperty]
        private bool _isSelected;

        public ThemeColorPair ThemeColors => new(ColorLight, ColorDark);

        public Color LightColor => ThemeColors.LightThemeColor;
        public Color DarkColor => ThemeColors.DarkThemeColor;

        public override string ToString() => Name;

        public Color CurrentGameColor => ThemeColors.CurrentColor;

        public void NotifyThemeChanged() => OnPropertyChanged(nameof(CurrentGameColor));
    }
}
