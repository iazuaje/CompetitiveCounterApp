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

        /// <summary>Conteo de sesiones para UI (no persistido).</summary>
        [ObservableProperty]
        private int _sessionCount;

        public ThemeColorPair ThemeColors => new(ColorLight, ColorDark);

        public Color GameColorLight => ThemeColors.LightThemeColor;
        public Color GameColorDark => ThemeColors.DarkThemeColor;
        public Color CurrentGameColor => ThemeColors.CurrentColor;
        public Color ToolbarColor => ThemeColors.ToolbarColor;

        public Color SurfaceColor => ThemeColors.SurfaceColor;
        public Color OnSurfaceColor => ThemeColors.OnSurfaceColor;

        public ImageSource GameImage => !string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath)
            ? ImageSource.FromFile(ImagePath)
            : null!;

        public void NotifyThemeChanged()
        {
            OnPropertyChanged(nameof(CurrentGameColor));
            OnPropertyChanged(nameof(ToolbarColor));
            OnPropertyChanged(nameof(SurfaceColor));
            OnPropertyChanged(nameof(OnSurfaceColor));
        }

        public override string ToString() => Name;
    }
}
