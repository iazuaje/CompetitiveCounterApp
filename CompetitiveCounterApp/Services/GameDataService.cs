using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.Services;

public class GameDataService
{
    private static List<IconData>? _icons;
    private static List<GameColor>? _gameColors;

    public static List<IconData> GetIcons()
    {
        return _icons ??= new List<IconData>
        {
            new IconData { Icon = FluentUI.games_24_regular, Description = "Games Icon" },
            new IconData { Icon = FluentUI.trophy_24_regular, Description = "Trophy Icon" },
            new IconData { Icon = FluentUI.target_24_regular, Description = "Target Icon" },
            new IconData { Icon = FluentUI.sport_24_regular, Description = "Sport Icon" },
            new IconData { Icon = FluentUI.xbox_controller_28_regular, Description = "Controller Icon" },
            new IconData { Icon = FluentUI.puzzle_piece_24_regular, Description = "Puzzle Icon" }
        };
    }

    public static List<GameColor> GetGameColors()
    {
        return _gameColors ??= new List<GameColor>
        {
            new GameColor { Name = "Rojo", ColorLight = "#E63946", ColorDark = "#FF5964" },
            new GameColor { Name = "Azul", ColorLight = "#457B9D", ColorDark = "#6DA5D0" },
            new GameColor { Name = "Verde", ColorLight = "#2A9D8F", ColorDark = "#4ECDC4" },
            new GameColor { Name = "Naranja", ColorLight = "#F77F00", ColorDark = "#FFB04C" },
            new GameColor { Name = "Morado", ColorLight = "#9B59B6", ColorDark = "#B57EDC" },
            new GameColor { Name = "Rosa", ColorLight = "#E91E63", ColorDark = "#FF4081" },
            new GameColor { Name = "Turquesa", ColorLight = "#00BCD4", ColorDark = "#4DD0E1" },
            new GameColor { Name = "Ambar", ColorLight = "#FFC107", ColorDark = "#FFD54F" },
            new GameColor { Name = "Indigo", ColorLight = "#3F51B5", ColorDark = "#7986CB" },
            new GameColor { Name = "Lima", ColorLight = "#8BC34A", ColorDark = "#AED581" }
        };
    }

    public static IconData GetDefaultIcon() => GetIcons()[0];
    
    public static GameColor GetDefaultColor() => GetGameColors()[0];
}