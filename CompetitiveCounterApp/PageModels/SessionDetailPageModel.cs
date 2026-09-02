using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CompetitiveCounterApp.Messages;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels
{
    public partial class SessionDetailPageModel : ObservableObject, IQueryAttributable
    {
        private const string CreateNewPlayerOption = "Crear nuevo jugador…";

        private readonly SessionRepository _sessionRepository;
        private readonly PlayerRepository _playerRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private Session? _session;

        [ObservableProperty]
        private Game? _game;

        [ObservableProperty]
        private ObservableCollection<SessionPlayer> _leaderboard = [];

        [ObservableProperty]
        private string _title = "Sesión";

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private bool _hasNotes;

        [ObservableProperty]
        private string _sessionDateText = string.Empty;

        [ObservableProperty]
        private bool _isActive;

        [ObservableProperty]
        private bool _isBusy;

        public SessionDetailPageModel(
            SessionRepository sessionRepository,
            PlayerRepository playerRepository,
            ModalErrorHandler errorHandler)
        {
            _sessionRepository = sessionRepository;
            _playerRepository = playerRepository;
            _errorHandler = errorHandler;

            WeakReferenceMessenger.Default.Register<AppThemeChangedMessage>(this, static (r, _) =>
            {
                var vm = (SessionDetailPageModel)r;
                vm.Game?.NotifyThemeChanged();
                foreach (var row in vm.Leaderboard)
                    row.Player?.NotifyThemeChanged();
            });
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

        [RelayCommand]
        private async Task Appearing()
        {
            if (Session is { ID: > 0 })
                await LoadData(Session.ID);
        }

        private async Task LoadData(int id)
        {
            try
            {
                IsBusy = true;
                Session = await _sessionRepository.GetAsync(id);

                if (Session is null || Session.ID == 0)
                {
                    _errorHandler.HandleError(new Exception("No se encontró la sesión."));
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                ApplySession(Session);
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

        private void ApplySession(Session session)
        {
            IsActive = session.IsActive;
            Notes = session.Notes?.Trim() ?? string.Empty;
            HasNotes = !string.IsNullOrWhiteSpace(Notes);
            SessionDateText = session.SessionDateLocal.ToString("dd/MM/yyyy HH:mm");
            Title = IsActive ? "Sesión activa" : "Sesión cerrada";
            Game = session.Game;

            Leaderboard = new ObservableCollection<SessionPlayer>(
                session.SessionPlayers.OrderByDescending(sp => sp.Wins));
        }

        [RelayCommand]
        private async Task AddPlayer()
        {
            if (Session is null || !IsActive)
                return;

            try
            {
                IsBusy = true;
                var available = await _playerRepository.ListAvailableForSessionAsync(Session.ID);

                var options = available
                    .Select(p => p.Name)
                    .Append(CreateNewPlayerOption)
                    .ToArray();

                var choice = await Shell.Current.DisplayActionSheet(
                    "Agregar jugador",
                    "Cancelar",
                    null,
                    options);

                if (string.IsNullOrEmpty(choice) || choice == "Cancelar")
                    return;

                if (choice == CreateNewPlayerOption)
                {
                    await CreatePlayerAndAddAsync();
                    return;
                }

                var player = available.FirstOrDefault(p => p.Name == choice);
                if (player is null)
                    return;

                await _sessionRepository.AddPlayerAsync(Session.ID, player.ID);
                await LoadData(Session.ID);
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

        private async Task CreatePlayerAndAddAsync()
        {
            if (Session is null)
                return;

            var name = await Shell.Current.DisplayPromptAsync(
                "Nuevo jugador",
                "Nombre del jugador",
                "Agregar",
                "Cancelar",
                maxLength: 40,
                keyboard: Keyboard.Text);

            if (string.IsNullOrWhiteSpace(name))
                return;

            var defaultIcon = GameDataService.GetDefaultPlayerIcon();
            var player = new Player
            {
                Name = name.Trim(),
                Icon = defaultIcon.Icon,
                ColorLight = GameDataService.GetDefaultColor().ColorLight,
                ColorDark = GameDataService.GetDefaultColor().ColorDark
            };

            await _playerRepository.SaveItemAsync(player);
            await _sessionRepository.AddPlayerAsync(Session.ID, player.ID);
            await LoadData(Session.ID);
            await AppShell.DisplayToastAsync("Jugador agregado");
        }

        [RelayCommand]
        private async Task IncrementWins(SessionPlayer sessionPlayer)
        {
            await ChangeWinsAsync(sessionPlayer, delta: 1);
        }

        [RelayCommand]
        private async Task DecrementWins(SessionPlayer sessionPlayer)
        {
            await ChangeWinsAsync(sessionPlayer, delta: -1);
        }

        [RelayCommand]
        private async Task EditWins(SessionPlayer? sessionPlayer)
        {
            if (Session is null || !IsActive || sessionPlayer?.Player is null)
                return;

            var input = await Shell.Current.DisplayPromptAsync(
                "Victorias",
                $"Valor para {sessionPlayer.Player.Name}",
                "Guardar",
                "Cancelar",
                initialValue: sessionPlayer.Wins.ToString(),
                keyboard: Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(input) || !int.TryParse(input, out var wins))
                return;

            try
            {
                IsBusy = true;
                await _sessionRepository.SetWinsAsync(Session.ID, sessionPlayer.PlayerID, wins);
                await LoadData(Session.ID);
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

        private async Task ChangeWinsAsync(SessionPlayer? sessionPlayer, int delta)
        {
            if (Session is null || !IsActive || sessionPlayer is null)
                return;

            try
            {
                IsBusy = true;
                await _sessionRepository.AdjustWinsAsync(Session.ID, sessionPlayer.PlayerID, delta);
                await LoadData(Session.ID);
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
        private async Task CloseSession()
        {
            if (Session is null || !IsActive)
                return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Cerrar sesión",
                "¿Cerrar la sesión activa? Podrás crear una nueva desde el detalle del juego.",
                "Cerrar",
                "Cancelar");

            if (!confirm)
                return;

            try
            {
                IsBusy = true;
                await _sessionRepository.CloseAsync(Session.ID);
                await AppShell.DisplayToastAsync("Sesión cerrada");
                await Shell.Current.GoToAsync("..");
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
