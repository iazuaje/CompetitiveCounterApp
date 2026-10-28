using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CompetitiveCounterApp.Helpers;
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

        public bool HasPlayers => Leaderboard.Count > 0;

        partial void OnLeaderboardChanged(ObservableCollection<SessionPlayer> value) =>
            OnPropertyChanged(nameof(HasPlayers));

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

        [ObservableProperty]
        private LeaderboardReorderRequest? _leaderboardReorderRequest;

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

            HapticFeedbackHelper.Click();

            try
            {
                IsBusy = true;
                var available = await _playerRepository.ListAvailableForSessionAsync(Session.ID);

                var options = available
                    .Select(p => p.Name)
                    .Append(CreateNewPlayerOption)
                    .ToArray();

                var choice = await Shell.Current.DisplayActionSheetAsync(
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

        /// <summary>Tap sobre el jugador: sus estadísticas en el juego de esta sesión.</summary>
        [RelayCommand]
        private async Task NavigateToPlayer(SessionPlayer? sessionPlayer)
        {
            if (Session is null || sessionPlayer is null || sessionPlayer.PlayerID == 0 || IsBusy)
                return;

            HapticFeedbackHelper.Click();
            await Shell.Current.GoToAsync($"playergamestats?playerId={sessionPlayer.PlayerID}&gameId={Session.GameID}");
        }

        /// <summary>Mantener presionado el jugador: quitarlo de la sesión (solo si está activa).</summary>
        [RelayCommand]
        private async Task RemovePlayer(SessionPlayer? sessionPlayer)
        {
            if (Session is null || sessionPlayer is null || IsBusy)
                return;

            if (!IsActive)
            {
                await AppShell.DisplayToastAsync("No se puede modificar una sesión cerrada");
                return;
            }

            // Sin confirmación: se aplica al instante y se puede deshacer (con sus victorias).
            var sessionId = Session.ID;
            var playerId = sessionPlayer.PlayerID;
            var wins = sessionPlayer.Wins;

            try
            {
                IsBusy = true;
                await _sessionRepository.RemovePlayerAsync(sessionId, playerId);
                Leaderboard.Remove(sessionPlayer);
                OnPropertyChanged(nameof(HasPlayers));

                await UndoSnackbar.ShowAsync(
                    $"{sessionPlayer.Player?.Name} quitado de la sesión",
                    async () =>
                    {
                        await _sessionRepository.AddPlayerAsync(sessionId, playerId, wins);
                        if (Session?.ID == sessionId)
                            await LoadData(sessionId);
                    },
                    _errorHandler);
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
            if (Session is null || !IsActive || sessionPlayer?.Player is null || IsBusy)
                return;

            HapticFeedbackHelper.Click();

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
                var previous = sessionPlayer.Wins;
                var newWins = await _sessionRepository.SetWinsAsync(Session.ID, sessionPlayer.PlayerID, wins);
                await ApplyWinsAndReorderAsync(sessionPlayer, newWins);
                ShowWinsUndo(sessionPlayer, previous);
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
            if (Session is null || !IsActive || sessionPlayer is null || IsBusy)
                return;

            try
            {
                HapticFeedbackHelper.Click();
                IsBusy = true;
                var previous = sessionPlayer.Wins;
                var newWins = await _sessionRepository.AdjustWinsAsync(Session.ID, sessionPlayer.PlayerID, delta);
                await ApplyWinsAndReorderAsync(sessionPlayer, newWins);
                ShowWinsUndo(sessionPlayer, previous);
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

        /// <summary>Aviso corto con el nuevo valor; «Deshacer» vuelve al anterior (un toque de más es el error típico).</summary>
        private void ShowWinsUndo(SessionPlayer sessionPlayer, int previous)
        {
            if (Session is null || sessionPlayer.Wins == previous)
                return;

            var sessionId = Session.ID;
            var wins = sessionPlayer.Wins;
            UndoSnackbar.ShowAsync(
                $"{sessionPlayer.Player?.Name}: {wins} {(wins == 1 ? "victoria" : "victorias")}",
                async () =>
                {
                    var restored = await _sessionRepository.SetWinsAsync(sessionId, sessionPlayer.PlayerID, previous);
                    if (Session?.ID == sessionId)
                        await ApplyWinsAndReorderAsync(sessionPlayer, restored);
                },
                _errorHandler,
                seconds: 3).FireAndForgetSafeAsync(_errorHandler);
        }

        private async Task ApplyWinsAndReorderAsync(SessionPlayer sessionPlayer, int newWins)
        {
            sessionPlayer.Wins = newWins;

            var targetOrder = Leaderboard
                .OrderByDescending(sp => sp.Wins)
                .ThenBy(sp => sp.PlayerID)
                .ToList();

            if (Leaderboard.Select(sp => sp.PlayerID).SequenceEqual(targetOrder.Select(sp => sp.PlayerID)))
                return;

            var request = new LeaderboardReorderRequest(Leaderboard, targetOrder);
            LeaderboardReorderRequest = request;
            await request.Completion;
        }

        [RelayCommand]
        private async Task CloseSession()
        {
            if (Session is null || !IsActive)
                return;

            HapticFeedbackHelper.Click();

            // Sin victorias la sesión no aporta datos: el repositorio la descarta en vez de guardarla.
            var hasWins = Leaderboard.Any(sp => sp.Wins > 0);

            bool confirm = hasWins
                ? await Shell.Current.DisplayAlertAsync(
                    "Cerrar sesión",
                    "¿Cerrar la sesión activa? Podrás crear una nueva desde el detalle del juego.",
                    "Cerrar",
                    "Cancelar")
                : await Shell.Current.DisplayAlertAsync(
                    "Descartar sesión",
                    "La sesión no tiene victorias, así que no se guardará. ¿Descartarla?",
                    "Descartar",
                    "Cancelar");

            if (!confirm)
                return;

            try
            {
                IsBusy = true;
                var kept = await _sessionRepository.CloseAsync(Session.ID);
                await AppShell.DisplayToastAsync(kept ? "Sesión cerrada" : "Sesión descartada");
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

        /// <summary>Sesión nueva del mismo juego con estos jugadores (en 0); solo desde una sesión cerrada.</summary>
        [RelayCommand]
        private async Task Rematch()
        {
            if (Session is null || IsActive || IsBusy)
                return;

            HapticFeedbackHelper.Click();

            try
            {
                IsBusy = true;

                var active = await _sessionRepository.GetActiveByGameIdAsync(Session.GameID);
                if (active is not null && !await NewSessionPrompt.ConfirmReplaceActiveAsync(active.HasBestPlayer, "Revancha"))
                    return;

                var rematch = await _sessionRepository.CreateRematchAsync(Session.ID);
                // Reemplaza esta pantalla: volver lleva al detalle del juego, no a la sesión vieja.
                await Shell.Current.GoToAsync($"../sessiondetail?id={rematch.ID}");
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

        /// <summary>Imagen del resultado para mandar al grupo (solo a pedido, desde la barra).</summary>
        [RelayCommand]
        private async Task ShareResult()
        {
            if (Session is null || IsBusy)
                return;

            HapticFeedbackHelper.Click();

            if (!Leaderboard.Any(sp => sp.Wins > 0))
            {
                await AppShell.DisplayToastAsync("Todavía no hay victorias para compartir");
                return;
            }

            try
            {
                IsBusy = true;
                var path = await SessionShareImage.CreateAsync(Session, Leaderboard.ToList());
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Resultado de la sesión",
                    File = new ShareFile(path, "image/png")
                });
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
        private async Task DeleteSession()
        {
            if (Session is null || IsBusy)
                return;

            HapticFeedbackHelper.Click();

            // Se elimina al instante; «Deshacer» la vuelve a crear con sus jugadores y victorias.
            var deleted = Session;
            var players = Leaderboard.ToList();

            try
            {
                IsBusy = true;
                await _sessionRepository.DeleteAsync(deleted.ID);
                await Shell.Current.GoToAsync("..");
                await UndoSnackbar.ShowAsync(
                    "Sesión eliminada",
                    async () =>
                    {
                        await _sessionRepository.RestoreAsync(deleted, players);
                        WeakReferenceMessenger.Default.Send(new SessionsChangedMessage(deleted.GameID));
                    },
                    _errorHandler,
                    seconds: 6);
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
