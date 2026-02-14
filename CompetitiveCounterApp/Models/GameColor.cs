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

        public Color LightColor => Color.FromArgb(ColorLight);
        public Color DarkColor => Color.FromArgb(ColorDark);

        public override string ToString() => Name;

        public Color CurrentGameColor
        {
            get
            {
                var theme = Application.Current?.RequestedTheme ?? AppTheme.Light;
                return theme == AppTheme.Dark ? DarkColor : LightColor;
            }
        }
    }
}
