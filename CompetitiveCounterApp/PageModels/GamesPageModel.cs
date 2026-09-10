using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CompetitiveCounterApp.Helpers;
using CompetitiveCounterApp.Messages;
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

        public bool HasGames => Games.Count > 0;

        partial void OnGamesChanged(List<Game> value) =>
            OnPropertyChanged(nameof(HasGames));

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isRefreshing;

        public GamesPageModel(GameRepository gameRepository, SessionRepository sessionRepository, ModalErrorHandler errorHandler)
        {
            _gameRepository = gameRepository;
            _sessionRepository = sessionRepository;
            _errorHandler = errorHandler;

            WeakReferenceMessenger.Default.Register<AppThemeChangedMessage>(this, static (r, _) =>
            {
                foreach (var game in ((GamesPageModel)r).Games)
                    game.NotifyThemeChanged();
            });
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

                foreach (var game in Games)
                    game.SessionCount = await _sessionRepository.CountByGameIdAsync(game.ID);
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
            HapticFeedbackHelper.Click();
            await Shell.Current.GoToAsync("creategame");
        }

        [RelayCommand]
        private async Task NavigateToGame(Game game)
        {
            HapticFeedbackHelper.Click();
            await Shell.Current.GoToAsync($"gamedetail?id={game.ID}");
        }

        [RelayCommand]
        private async Task OpenOptions()
        {
            HapticFeedbackHelper.Click();
            // Por implementar
            await AppShell.DisplayToastAsync("Opciones - Por implementar");
        }
    }
}
