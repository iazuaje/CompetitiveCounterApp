using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels
{
    public partial class GameDetailPageModel : ObservableObject, IQueryAttributable
    {
        private Game? _game;
        private readonly GameRepository _gameRepository;
        private readonly PlayerRepository _playerRepository;
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
        private List<Player> _players = [];

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _newPlayerName = string.Empty;

        [ObservableProperty]
        private string _newPlayerColor = "#FF6B6B";

        private readonly string[] _playerColors = new[]
        {
            "#FF6B6B", "#4ECDC4", "#45B7D1", "#FFA07A",
            "#98D8C8", "#F7DC6F", "#BB8FCE", "#85C1E2"
        };

        public GameDetailPageModel(GameRepository gameRepository, PlayerRepository playerRepository, ModalErrorHandler errorHandler)
        {
            _gameRepository = gameRepository;
            _playerRepository = playerRepository;
            _errorHandler = errorHandler;
            _selectedIcon = new();
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
                _game = new Game();
                _game.Players = [];
                Players = _game.Players;
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
                    return;
                }

                Name = _game.Name;
                Description = _game.Description;
                Players = _game.Players;
                SelectedIcon = Icons.FirstOrDefault(i => i.Icon == _game.Icon);
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

            _game.Name = Name;
            _game.Description = Description;
            _game.Icon = SelectedIcon.Icon ?? FluentUI.games_24_regular;
            
            await _gameRepository.SaveItemAsync(_game);

            // Guardar jugadores nuevos
            foreach (var player in _game.Players)
            {
                if (player.ID == 0)
                {
                    player.GameID = _game.ID;
                    await _playerRepository.SaveItemAsync(player);
                }
            }

            await Shell.Current.GoToAsync("..");
            await AppShell.DisplayToastAsync("Juego guardado");
        }

        [RelayCommand]
        private async Task AddPlayer()
        {
            if (string.IsNullOrWhiteSpace(NewPlayerName))
            {
                await AppShell.DisplayToastAsync("El nombre del jugador es requerido");
                return;
            }

            if (_game is null)
            {
                _errorHandler.HandleError(new Exception("Game is null. Cannot add player."));
                return;
            }

            var player = new Player
            {
                Name = NewPlayerName,
                ColorHex = GetNextPlayerColor(),
                GameID = _game.ID,
                WinCount = 0
            };

            _game.Players.Add(player);
            Players = new List<Player>(_game.Players);
            
            NewPlayerName = string.Empty;
            
            await AppShell.DisplayToastAsync($"Jugador {player.Name} agregado");
        }

        [RelayCommand]
        private async Task DeletePlayer(Player player)
        {
            if (_game is null) return;

            _game.Players.Remove(player);
            Players = [.. _game.Players];

            if (player.ID > 0)
            {
                await _playerRepository.DeleteItemAsync(player);
            }

            await AppShell.DisplayToastAsync("Jugador eliminado");
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
                $"¿Estás seguro de eliminar '{_game.Name}'?",
                "Sí",
                "No");

            if (confirm)
            {
                await _gameRepository.DeleteItemAsync(_game);
                await Shell.Current.GoToAsync("..");
                await AppShell.DisplayToastAsync("Juego eliminado");
            }
        }

        private string GetNextPlayerColor()
        {
            var usedColors = _game?.Players.Select(p => p.ColorHex).ToHashSet() ?? new HashSet<string>();
            var availableColor = _playerColors.FirstOrDefault(c => !usedColors.Contains(c));
            return availableColor ?? _playerColors[Random.Shared.Next(_playerColors.Length)];
        }
    }
}