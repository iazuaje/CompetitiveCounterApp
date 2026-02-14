using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels
{
    public partial class CreateGamePageModel : ObservableObject
    {
        private readonly GameRepository _gameRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private IconData _selectedIcon;

        [ObservableProperty]
        private List<IconData> _icons = new List<IconData>
        {
            new IconData { Icon = FluentUI.games_24_regular, Description = "Games Icon" },
            new IconData { Icon = FluentUI.trophy_24_regular, Description = "Trophy Icon" },
            new IconData { Icon = FluentUI.target_24_regular, Description = "Target Icon" },
            new IconData { Icon = FluentUI.sport_24_regular, Description = "Sport Icon" },
            new IconData { Icon = FluentUI.xbox_controller_28_regular, Description = "Controller Icon" },
            new IconData { Icon = FluentUI.puzzle_piece_24_regular, Description = "Puzzle Icon" }
        };

        [ObservableProperty]
        private GameColor _selectedColor;

        [ObservableProperty]
        private List<GameColor> _gameColors = new List<GameColor>
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

        [ObservableProperty]
        private bool _isBusy;

        public CreateGamePageModel(GameRepository gameRepository, ModalErrorHandler errorHandler)
        {
            _gameRepository = gameRepository;
            _errorHandler = errorHandler;
            _selectedIcon = _icons[0];
            _selectedColor = _gameColors[0];
            _selectedColor.IsSelected = true;
        }

        [RelayCommand]
        private void SelectIcon(IconData selectedIcon)
        {
            SelectedIcon.IsSelected = false;
            SelectedIcon = selectedIcon;
            SelectedIcon.IsSelected = true;
        }

        [RelayCommand]
        private void SelectColor(GameColor color)
        {
            foreach (var c in GameColors)
            {
                c.IsSelected = false;
            }
            color.IsSelected = true;
            SelectedColor = color;
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await AppShell.DisplayToastAsync("El nombre del juego es requerido");
                return;
            }

            try
            {
                IsBusy = true;

                var game = new Game
                {
                    Name = Name,
                    Description = Description,
                    Icon = SelectedIcon?.Icon ?? FluentUI.games_24_regular,
                    ColorLight = SelectedColor?.ColorLight ?? "#E63946",
                    ColorDark = SelectedColor?.ColorDark ?? "#FF5964",
                    CreatedDate = DateTime.Now
                };

                await _gameRepository.SaveItemAsync(game);

                await Shell.Current.GoToAsync("..");
                await AppShell.DisplayToastAsync("Juego creado exitosamente");
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
