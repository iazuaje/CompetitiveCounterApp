using CommunityToolkit.Mvvm.ComponentModel;

namespace CompetitiveCounterApp.Models
{
    public partial class Player : ObservableObject
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string ColorLight { get; set; } = "#C62828";
        public string ColorDark { get; set; } = "#EF9A9A";

        public ThemeColorPair ThemeColors => new(ColorLight, ColorDark);
        public Color CurrentColor => ThemeColors.CurrentColor;

        public void NotifyThemeChanged() => OnPropertyChanged(nameof(CurrentColor));

        public override string ToString() => Name;
    }
}
