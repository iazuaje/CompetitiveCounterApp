using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels
{
    public partial class GameDetailPageModel : ObservableObject, IQueryAttributable
    {
        private readonly GameRepository _gameRepository;
        private readonly SessionRepository _sessionRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private Game? _game;

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
        private List<Session> _sessions = new();

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isLoadingGame = true;

        public GameDetailPageModel(GameRepository gameRepository, SessionRepository sessionRepository, ModalErrorHandler errorHandler)
        {
            _gameRepository = gameRepository;
            _sessionRepository = sessionRepository;
            _errorHandler = errorHandler;
            _selectedIcon = _icons[0];
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
                IsLoadingGame = true;

                Game = await _gameRepository.GetAsync(id);

                if (Game.IsNullOrNew())
                {
                    _errorHandler.HandleError(new Exception($"Game with id {id} could not be found."));
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                Name = Game.Name;
                Description = Game.Description;
                SelectedIcon = Icons.FirstOrDefault(i => i.Icon == Game.Icon) ?? Icons[0];
                
                Sessions = await _sessionRepository.ListAsync(Game.ID);
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
            finally
            {
                IsBusy = false;
                IsLoadingGame = false;
            }
        }

        [RelayCommand]
        private async Task Edit()
        {
            if (Game.IsNullOrNew())
            {
                await AppShell.DisplayToastAsync("Error: No se pudo identificar el juego");
                return;
            }

            await Shell.Current.GoToAsync($"editgame?id={Game.ID}");
        }

        [RelayCommand]
        private async Task AddSession()
        {
            if (Game.IsNullOrNew())
            {
                await AppShell.DisplayToastAsync("Error: No se pudo identificar el juego");
                return;
            }

            await Shell.Current.GoToAsync($"sessiondetail?gameId={Game.ID}");
        }

        [RelayCommand]
        private async Task NavigateToSession(Session session)
        {
            await Shell.Current.GoToAsync($"sessiondetail?id={session.ID}");
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (Game.IsNullOrNew())
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert(
                "Eliminar Juego",
                $"¿Estás seguro de eliminar '{Game.Name}'? Esto eliminará todas las sesiones asociadas.",
                "Sí",
                "No");

            if (!confirm) return;

            try
            {
                IsBusy = true;
                await _gameRepository.DeleteItemAsync(Game);
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