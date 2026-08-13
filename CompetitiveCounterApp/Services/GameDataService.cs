using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.Services;

public class GameDataService
{
    private static List<IconData>? _icons;
    private static List<GameColor>? _gameColors;

    public static List<IconData> GetIcons()
    {
        List<IconData> icons = _icons ??= new List<IconData>
        {
            new IconData { Icon = FluentUI.games_24_regular, Description = "Games Icon" },
            new IconData { Icon = FluentUI.trophy_24_regular, Description = "Trophy Icon" },
            new IconData { Icon = FluentUI.target_24_regular, Description = "Target Icon" },
            new IconData { Icon = FluentUI.sport_24_regular, Description = "Sport Icon" },
            new IconData { Icon = FluentUI.xbox_controller_28_regular, Description = "Controller Icon" },
            new IconData { Icon = FluentUI.puzzle_piece_24_regular, Description = "Puzzle Icon" }
        };

        foreach (var item in icons)
        {
            item.IsSelected = false;
        }

        return icons;
    }

    public static List<GameColor> GetGameColors()
    {
        var colors = _gameColors ??= new List<GameColor>
        {
            new GameColor { Name = "Rojo", ColorLight = "#C62828", ColorDark = "#EF9A9A" },
            new GameColor { Name = "Azul", ColorLight = "#1565C0", ColorDark = "#90CAF9" },
            new GameColor { Name = "Verde", ColorLight = "#2E7D32", ColorDark = "#A5D6A7" },
            new GameColor { Name = "Naranja", ColorLight = "#EF6C00", ColorDark = "#FFCC80" },
            new GameColor { Name = "Morado", ColorLight = "#6A1B9A", ColorDark = "#CE93D8" },
            new GameColor { Name = "Rosa", ColorLight = "#AD1457", ColorDark = "#F48FB1" },
            new GameColor { Name = "Turquesa", ColorLight = "#00838F", ColorDark = "#80DEEA" },
            new GameColor { Name = "Ámbar", ColorLight = "#FF8F00", ColorDark = "#FFE082" },
            new GameColor { Name = "Índigo", ColorLight = "#283593", ColorDark = "#9FA8DA" },
            new GameColor { Name = "Lima", ColorLight = "#558B2F", ColorDark = "#C5E1A5" }
        };

        return colors.Select(color => new GameColor
        {
            Name = color.Name,
            ColorLight = color.ColorLight,
            ColorDark = color.ColorDark
        }).ToList();
    }

    public static IconData GetDefaultIcon() => GetIcons()[0];
    
    public static GameColor GetDefaultColor() => GetGameColors()[0];
}