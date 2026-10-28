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

        /// <summary>Posición en el orden manual de la lista de juegos (menor = primero).</summary>
        public int SortOrder { get; set; }

        /// <summary>Etiquetas del juego (muchos a muchos vía GameCategories).</summary>
        public List<Category> Categories { get; set; } = [];

        public bool HasCategories => Categories.Count > 0;

        /// <summary>Nombres de las categorías para UI.</summary>
        public string CategoriesText => string.Join(" · ", Categories.Select(c => c.Name));

        /// <summary>Hay una sesión en curso, para UI y filtros (no persistido).</summary>
        [ObservableProperty]
        private bool _hasActiveSession;

        /// <summary>Tiene al menos una sesión con victorias (no persistido); false = "sin jugar".</summary>
        [ObservableProperty]
        private bool _hasWins;

        /// <summary>Fecha de la última sesión, para ordenar por "jugado recientemente" (no persistido).</summary>
        [ObservableProperty]
        private DateTime? _lastPlayedDate;

        /// <summary>Conteo de sesiones para UI (no persistido).</summary>
        [ObservableProperty]
        private int _sessionCount;

        /// <summary>Primero del ranking del juego (sesiones ganadas, luego victorias) para UI (no persistido).</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasLeader))]
        private string _leaderName = string.Empty;

        [ObservableProperty]
        private int _leaderSessionsWon;

        public bool HasLeader => !string.IsNullOrEmpty(LeaderName);

        public ThemeColorPair ThemeColors => new(ColorLight, ColorDark);

        public Color GameColorLight => ThemeColors.LightThemeColor;
        public Color GameColorDark => ThemeColors.DarkThemeColor;
        public Color CurrentGameColor => ThemeColors.CurrentColor;
        public Color OnGameColor => ThemeColors.OnColor;
        public Color ToolbarColor => ThemeColors.ToolbarColor;
        public Color OnToolbarColor => ThemeColors.OnToolbarColor;
        public Color ComplementaryColor => ThemeColors.ComplementaryColor;

        public Color SurfaceColor => ThemeColors.SurfaceColor;
        public Color OnSurfaceColor => ThemeColors.OnSurfaceColor;

        /// <summary>
        /// Para ocultar la imagen cuando no hay: al reutilizar vistas (BindableLayout) un Source nulo
        /// puede dejar visible la imagen del juego anterior.
        /// </summary>
        public bool HasImage => !string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath);

        public ImageSource GameImage => !string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath)
            ? ImageSource.FromFile(ImagePath)
            : null!;

        public void NotifyThemeChanged()
        {
            OnPropertyChanged(nameof(CurrentGameColor));
            OnPropertyChanged(nameof(OnGameColor));
            OnPropertyChanged(nameof(ToolbarColor));
            OnPropertyChanged(nameof(OnToolbarColor));
            OnPropertyChanged(nameof(ComplementaryColor));
            OnPropertyChanged(nameof(SurfaceColor));
            OnPropertyChanged(nameof(OnSurfaceColor));
        }

        public override string ToString() => Name;
    }
}
