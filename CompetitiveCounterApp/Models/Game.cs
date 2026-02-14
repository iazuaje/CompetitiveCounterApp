namespace CompetitiveCounterApp.Models
{
    public class Game
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ColorLight { get; set; } = "#E63946";
        public string ColorDark { get; set; } = "#FF5964";
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public Color GameColorLight => Color.FromArgb(ColorLight);
        public Color GameColorDark => Color.FromArgb(ColorDark);
        
        public Color CurrentGameColor
        {
            get
            {
                var theme = Application.Current?.RequestedTheme ?? AppTheme.Light;
                return theme == AppTheme.Dark ? GameColorDark : GameColorLight;
            }
        }

        public override string ToString() => Name;
    }
}