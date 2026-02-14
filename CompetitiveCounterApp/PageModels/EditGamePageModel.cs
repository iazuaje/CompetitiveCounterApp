using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels
{
    public partial class EditGamePageModel : ObservableObject, IQueryAttributable
    {
        private Game? _game;
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

        public EditGamePageModel(GameRepository gameRepository, ModalErrorHandler errorHandler)
        {
            _gameRepository = gameRepository;
            _errorHandler = errorHandler;
            _selectedIcon = _icons[0];
            _selectedColor = _gameColors[0];
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("id"))
            {
                int id = Convert.ToInt32(query["id"]);
                LoadData(id).FireAndForgetSafeAsync(_errorHandler);
            }
            else
            {
                Shell.Current.GoToAsync("..").FireAndForgetSafeAsync(_errorHandler);
            }
        }

        private async Task LoadData(int id)
        {
            try
            {
                IsBusy = true;

                _game = await _gameRepository.GetAsync(id);

                if (_game.IsNullOrNew())
                {
                    _errorHandler.HandleError(new Exception($"Game with id {id} could not be found."));
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                Name = _game.Name;
                Description = _game.Description;
                SelectedIcon = Icons.FirstOrDefault(i => i.Icon == _game.Icon) ?? Icons[0];
                
                var matchingColor = GameColors.FirstOrDefault(c => 
                    c.ColorLight == _game.ColorLight && c.ColorDark == _game.ColorDark);
                
                if (matchingColor != null)
                {
                    matchingColor.IsSelected = true;
                    SelectedColor = matchingColor;
                }
                else
                {
                    SelectedColor = GameColors[0];
                    SelectedColor.IsSelected = true;
                }
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
            if (_game is null)
            {
                _errorHandler.HandleError(new Exception("Game is null. Cannot Save."));
                return;
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                await AppShell.DisplayToastAsync("El nombre del juego es requerido");
                return;
            }

            try
            {
                IsBusy = true;

                _game.Name = Name;
                _game.Description = Description;
                _game.Icon = SelectedIcon?.Icon ?? FluentUI.games_24_regular;
                _game.ColorLight = SelectedColor?.ColorLight ?? "#E63946";
                _game.ColorDark = SelectedColor?.ColorDark ?? "#FF5964";
                
                await _gameRepository.SaveItemAsync(_game);

                await Shell.Current.GoToAsync("..");
                await AppShell.DisplayToastAsync("Juego actualizado exitosamente");
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

        [RelayCommand]
        private async Task Delete()
        {
            if (_game.IsNullOrNew())
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert(
                "Eliminar Juego",
                $"¿Estás seguro de eliminar '{_game.Name}'? Esto eliminará todas las sesiones asociadas.",
                "Sí",
                "No");

            if (!confirm) return;

            try
            {
                IsBusy = true;
                await _gameRepository.DeleteItemAsync(_game);
                await Shell.Current.GoToAsync("..");
                await AppShell.DisplayToastAsync("Juego eliminado exitosamente");
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
