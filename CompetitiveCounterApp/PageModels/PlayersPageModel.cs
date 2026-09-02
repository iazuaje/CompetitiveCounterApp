using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels
{
    public partial class PlayersPageModel : ObservableObject
    {
        private readonly PlayerRepository _playerRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private List<Player> _players = [];

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isRefreshing;

        public PlayersPageModel(PlayerRepository playerRepository, ModalErrorHandler errorHandler)
        {
            _playerRepository = playerRepository;
            _errorHandler = errorHandler;
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
            await Shell.Current.GoToAsync("createplayer");
        }

        [RelayCommand]
        private async Task NavigateToPlayer(Player player)
        {
            await Shell.Current.GoToAsync($"editplayer?id={player.ID}");
        }
    }
}
