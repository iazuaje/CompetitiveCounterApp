using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CompetitiveCounterApp.Helpers;
using CompetitiveCounterApp.Messages;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels
{
    public partial class PlayersPageModel : ObservableObject
    {
        private readonly PlayerRepository _playerRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private List<Player> _players = [];

        public bool HasPlayers => Players.Count > 0;

        partial void OnPlayersChanged(List<Player> value) =>
            OnPropertyChanged(nameof(HasPlayers));

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isRefreshing;

        public PlayersPageModel(PlayerRepository playerRepository, ModalErrorHandler errorHandler)
        {
            _playerRepository = playerRepository;
            _errorHandler = errorHandler;

            WeakReferenceMessenger.Default.Register<AppThemeChangedMessage>(this, static (r, _) =>
            {
                foreach (var player in ((PlayersPageModel)r).Players)
                    player.NotifyThemeChanged();
            });
        }

        [RelayCommand]
        private async Task Appearing()
        {
            await LoadPlayers();
        }

        [RelayCommand]
        private async Task Refresh()
        {
            try
            {
                IsRefreshing = true;
                await LoadPlayers();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task LoadPlayers()
        {
            try
            {
                IsBusy = true;
                Players = await _playerRepository.ListAsync();
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
        private async Task AddPlayer()
        {
            HapticFeedbackHelper.Click();
            await Shell.Current.GoToAsync("createplayer");
        }

        [RelayCommand]
        private async Task NavigateToPlayer(Player player)
        {
            HapticFeedbackHelper.Click();
            await Shell.Current.GoToAsync($"editplayer?id={player.ID}");
        }
    }
}
