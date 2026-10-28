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

        /// <summary>Archivados: ocultos de la lista principal, en una sección plegable al final.</summary>
        [ObservableProperty]
        private List<Player> _archivedPlayers = [];

        [ObservableProperty]
        private bool _showArchived;

        [ObservableProperty]
        private string _archivedToggleText = string.Empty;

        public bool HasPlayers => Players.Count > 0 || ArchivedPlayers.Count > 0;

        public bool HasArchived => ArchivedPlayers.Count > 0;

        partial void OnPlayersChanged(List<Player> value) =>
            OnPropertyChanged(nameof(HasPlayers));

        partial void OnArchivedPlayersChanged(List<Player> value)
        {
            OnPropertyChanged(nameof(HasPlayers));
            OnPropertyChanged(nameof(HasArchived));
            UpdateArchivedToggleText();
        }

        partial void OnShowArchivedChanged(bool value) => UpdateArchivedToggleText();

        private void UpdateArchivedToggleText() =>
            ArchivedToggleText = $"{(ShowArchived ? "Ocultar" : "Ver")} archivados ({ArchivedPlayers.Count})";

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
                var vm = (PlayersPageModel)r;
                foreach (var player in vm.Players.Concat(vm.ArchivedPlayers))
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
                var players = await _playerRepository.ListAsync();
                Players = players.Where(p => !p.IsArchived).ToList();
                ArchivedPlayers = players.Where(p => p.IsArchived).ToList();
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
        private void ToggleArchived()
        {
            HapticFeedbackHelper.Click();
            ShowArchived = !ShowArchived;
        }

        [RelayCommand]
        private async Task AddPlayer()
        {
            HapticFeedbackHelper.Click();
            await Shell.Current.GoToAsync("createplayer");
        }

        [RelayCommand]
        private async Task OpenStats()
        {
            HapticFeedbackHelper.Click();
            // Sección del flyout, no página apilada: evita dos instancias de la misma pantalla.
            await Shell.Current.GoToAsync("//stats");
        }

        [RelayCommand]
        private async Task NavigateToPlayer(Player player)
        {
            HapticFeedbackHelper.Click();
            await Shell.Current.GoToAsync($"playerstats?id={player.ID}");
        }
    }
}
