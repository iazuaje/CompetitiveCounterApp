using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CompetitiveCounterApp.Helpers;
using CompetitiveCounterApp.Messages;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels;

/// <summary>Estadísticas generales de un juego (Juego → Stats).</summary>
public partial class GameStatsPageModel : ObservableObject, IQueryAttributable
{
    private readonly SessionRepository _sessionRepository;
    private readonly GameRepository _gameRepository;
    private readonly ModalErrorHandler _errorHandler;
    private int _gameId;

    [ObservableProperty]
    private string _title = "Estadísticas";

    /// <summary>Período de las estadísticas (compartido entre pantallas, ver <see cref="StatsPeriodPreference"/>).</summary>
    [ObservableProperty]
    private StatsPeriod _period = StatsPeriodPreference.Current;

    [ObservableProperty]
    private string _emptyText = string.Empty;

    partial void OnPeriodChanged(StatsPeriod value)
    {
        // Al sincronizar con la preferencia (LoadData) no hay que recargar de nuevo.
        if (value == StatsPeriodPreference.Current)
            return;

        StatsPeriodPreference.Current = value;
        LoadData().FireAndForgetSafeAsync(_errorHandler);
    }

    [ObservableProperty]
    private Game? _game;

    [ObservableProperty]
    private GameStats? _stats;

    [ObservableProperty]
    private bool _hasSessions;

    [ObservableProperty]
    private bool _hasSessionsWonShare;

    [ObservableProperty]
    private bool _hasHistory;

    [ObservableProperty]
    private bool _hasRecords;

    [ObservableProperty]
    private string _averagesText = string.Empty;

    [ObservableProperty]
    private string _activityText = string.Empty;

    [ObservableProperty]
    private string _sessionsLeaderName = string.Empty;

    [ObservableProperty]
    private string _sessionsLeaderText = string.Empty;

    [ObservableProperty]
    private string _winsLeaderName = string.Empty;

    [ObservableProperty]
    private string _winsLeaderText = string.Empty;

    [ObservableProperty]
    private List<GameRankingRow> _ranking = [];

    [ObservableProperty]
    private List<ShareSlice> _sessionsWonShare = [];

    [ObservableProperty]
    private List<ShareSlice> _winsShare = [];

    /// <summary>La vista arma una serie apilada por jugador al cambiar esta lista.</summary>
    [ObservableProperty]
    private List<HistorySeries> _history = [];

    [ObservableProperty]
    private List<GameRecord> _records = [];

    // Cada porción del anillo lleva el color de su jugador (depende del tema).
    [ObservableProperty]
    private List<Brush> _sessionsWonBrushes = [];

    [ObservableProperty]
    private List<Brush> _winsBrushes = [];

    public GameStatsPageModel(
        SessionRepository sessionRepository,
        GameRepository gameRepository,
        ModalErrorHandler errorHandler)
    {
        _sessionRepository = sessionRepository;
        _gameRepository = gameRepository;
        _errorHandler = errorHandler;

        WeakReferenceMessenger.Default.Register<AppThemeChangedMessage>(this, static (r, _) =>
        {
            var vm = (GameStatsPageModel)r;
            vm.Game?.NotifyThemeChanged();
            foreach (var row in vm.Ranking)
                row.Player.NotifyThemeChanged();
            vm.BuildBrushes();
            // Reasignar para que la vista rehaga las series con los colores del tema nuevo.
            vm.History = vm.History.ToList();
        });
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var value))
        {
            _gameId = Convert.ToInt32(value);
            LoadData().FireAndForgetSafeAsync(_errorHandler);
        }
        else
        {
            Shell.Current.GoToAsync("..").FireAndForgetSafeAsync(_errorHandler);
        }
    }

    // Recarga al volver (p. ej. desde las estadísticas de un jugador tras jugar más sesiones).
    [RelayCommand]
    private async Task Appearing()
    {
        if (_gameId > 0)
            await LoadData();
    }

    private async Task LoadData()
    {
        try
        {
            Game = await _gameRepository.GetAsync(_gameId);

            if (Game.IsNullOrNew())
            {
                _errorHandler.HandleError(new Exception("No se encontró el juego."));
                await Shell.Current.GoToAsync("..");
                return;
            }

            Title = Game.Name;
            Period = StatsPeriodPreference.Current;
            EmptyText = StatsPeriodFilter.EmptyText(Period, "Todavía no hay sesiones con victorias en este juego.");
            var sessions = StatsPeriodFilter.Apply(await _sessionRepository.ListAsync(_gameId), Period, DateTime.Now);
            ApplyStats(GameStatsCalculator.Compute(sessions));
            BuildBrushes();
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }
    }

    private void ApplyStats(GameStats stats)
    {
        Stats = stats;
        HasSessions = stats.SessionsPlayed > 0;

        AveragesText = $"Por sesión: {stats.AveragePlayersPerSession:0.#} jugadores · {stats.AverageWinsPerSession:0.#} victorias";
        ActivityText = stats.FirstSessionDate is DateTime first && stats.LastSessionDate is DateTime last
            ? $"Primera sesión {first:dd/MM/yyyy} · última {last:dd/MM/yyyy}"
            : string.Empty;

        Ranking = stats.Ranking.ToList();
        SessionsWonShare = stats.SessionsWonShare.ToList();
        WinsShare = stats.WinsShare.ToList();
        HasSessionsWonShare = SessionsWonShare.Count > 0;

        var sessionsLeader = SessionsWonShare.FirstOrDefault();
        SessionsLeaderName = sessionsLeader?.Label ?? "—";
        SessionsLeaderText = sessionsLeader is null
            ? "sin sesiones cerradas"
            : $"{sessionsLeader.Value} de {stats.ClosedSessions} sesiones";

        var winsLeader = WinsShare.FirstOrDefault();
        WinsLeaderName = winsLeader?.Label ?? "—";
        WinsLeaderText = winsLeader is null
            ? string.Empty
            : $"{winsLeader.Value} de {stats.TotalWins} victorias";

        History = stats.History.ToList();
        HasHistory = History.Count > 0 && History[0].Points.Count >= 2;

        Records = stats.Records.ToList();
        HasRecords = Records.Count > 0;
    }

    private void BuildBrushes()
    {
        SessionsWonBrushes = SessionsWonShare.Select(s => (Brush)new SolidColorBrush(s.Player.CurrentColor)).ToList();
        WinsBrushes = WinsShare.Select(s => (Brush)new SolidColorBrush(s.Player.CurrentColor)).ToList();
    }

    [RelayCommand]
    private async Task OpenPlayer(GameRankingRow? row)
    {
        if (row is null)
            return;

        HapticFeedbackHelper.Click();
        await Shell.Current.GoToAsync($"playergamestats?playerId={row.Player.ID}&gameId={_gameId}");
    }
}
