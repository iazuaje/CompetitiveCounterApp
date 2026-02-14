using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels
{
    public partial class GamesPageModel : ObservableObject
    {
        private readonly GameRepository _gameRepository;
        private readonly SessionRepository _sessionRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private List<Game> _games = [];

        [ObservableProperty]
        private Dictionary<int, int> _sessionCounts = new();

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isRefreshing;

        public GamesPageModel(GameRepository gameRepository, SessionRepository sessionRepository, ModalErrorHandler errorHandler)
        {
            _gameRepository = gameRepository;
            _sessionRepository = sessionRepository;
            _errorHandler = errorHandler;
        }

        [RelayCommand]
        private async Task Appearing()
        {
            await LoadGames();
        }

        [RelayCommand]
        private async Task Refresh()
        {
            try
            {
                IsRefreshing = true;
                await LoadGames();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task LoadGames()
        {
            try
            {
                IsBusy = true;
                Games = await _gameRepository.ListAsync();

                // Cargar el conteo de sesiones para cada juego
                var sessionCountsDict = new Dictionary<int, int>();
                foreach (var game in Games)
                {
                    var sessions = await _sessionRepository.ListAsync(game.ID);
                    sessionCountsDict[game.ID] = sessions.Count;
                }
                SessionCounts = sessionCountsDict;
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
        private async Task AddGame()
        {
            await Shell.Current.GoToAsync("creategame");
        }

        [RelayCommand]
        private async Task NavigateToGame(Game game)
        {
            await Shell.Current.GoToAsync($"gamedetail?id={game.ID}");
        }

        [RelayCommand]
        private async Task OpenOptions()
        {
            // Por implementar
            await AppShell.DisplayToastAsync("Opciones - Por implementar");
        }

        public int GetSessionCount(int gameId)
        {
            return SessionCounts.TryGetValue(gameId, out var count) ? count : 0;
        }
    }
}