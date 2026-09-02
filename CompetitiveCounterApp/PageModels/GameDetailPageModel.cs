using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CompetitiveCounterApp.Messages;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels
{
    public partial class GameDetailPageModel : ObservableObject, IQueryAttributable
    {
        private readonly GameRepository _gameRepository;
        private readonly SessionRepository _sessionRepository;
        private readonly ModalErrorHandler _errorHandler;
        private readonly GameOperationsService _gameOperations;
        private int _gameId;

        [ObservableProperty]
        private Game? _game;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private IconData _selectedIcon;

        [ObservableProperty]
        private List<IconData> _icons;

        [ObservableProperty]
        private List<Session> _sessions = new();

        public bool HasSessions => Sessions.Count > 0;

        partial void OnSessionsChanged(List<Session> value) =>
            OnPropertyChanged(nameof(HasSessions));

        [ObservableProperty]
        private Session? _activeSession;

        [ObservableProperty]
        private bool _hasActiveSession;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isLoadingGame = true;

        public GameDetailPageModel(
            GameRepository gameRepository,
            SessionRepository sessionRepository,
            ModalErrorHandler errorHandler,
            GameOperationsService gameOperations)
        {
            _gameRepository = gameRepository;
            _sessionRepository = sessionRepository;
            _errorHandler = errorHandler;
            _gameOperations = gameOperations;
            _icons = GameDataService.GetIcons();
            _selectedIcon = GameDataService.GetDefaultIcon();

            WeakReferenceMessenger.Default.Register<AppThemeChangedMessage>(this, static (r, _) =>
                ((GameDetailPageModel)r).Game?.NotifyThemeChanged());
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("id"))
            {
                _gameId = Convert.ToInt32(query["id"]);
                LoadData(_gameId).FireAndForgetSafeAsync(_errorHandler);
            }
            else
            {
                Shell.Current.GoToAsync("..").FireAndForgetSafeAsync(_errorHandler);
            }
        }

        [RelayCommand]
        private async Task Appearing()
        {
            if (_gameId > 0)
                await LoadData(_gameId);
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
                SelectedIcon = Icons.FirstOrDefault(i => i.Icon == Game.Icon) ?? GameDataService.GetDefaultIcon();

                Sessions = await _sessionRepository.ListAsync(Game.ID);
                ActiveSession = Sessions.FirstOrDefault(s => s.IsActive);
                HasActiveSession = ActiveSession is not null;
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

            if (HasActiveSession)
            {
                bool confirm = await Shell.Current.DisplayAlert(
                    "Nueva sesión",
                    "Hay una sesión activa. Al crear una nueva se cerrará la actual. ¿Continuar?",
                    "Crear",
                    "Cancelar");

                if (!confirm)
                    return;
            }

            var notes = await Shell.Current.DisplayPromptAsync(
                "Nueva sesión",
                "Descripción opcional",
                "Crear",
                "Cancelar",
                maxLength: 120,
                keyboard: Keyboard.Text);

            // Cancelar devuelve null; Enter vacío es string vacío (permitido).
            if (notes is null)
                return;

            try
            {
                IsBusy = true;
                var session = await _sessionRepository.CreateAsync(Game.ID, notes);
                await Shell.Current.GoToAsync($"sessiondetail?id={session.ID}");
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
        private async Task NavigateToSession(Session? session)
        {
            if (session is null || session.ID == 0)
                return;

            await Shell.Current.GoToAsync($"sessiondetail?id={session.ID}");
        }

        [RelayCommand]
        private async Task Delete()
        {
            try
            {
                IsBusy = true;
                await _gameOperations.DeleteGameAsync(Game, _errorHandler);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
