using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CompetitiveCounterApp.Helpers;
using CompetitiveCounterApp.Messages;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels;

/// <summary>Estadísticas de un jugador dentro de un juego.</summary>
public partial class PlayerGameStatsPageModel : ObservableObject, IQueryAttributable
{
    /// <summary>Rivales que entran en el gráfico de barras; el resto queda solo en la lista.</summary>
    private const int ChartedRivals = 6;

    private readonly SessionRepository _sessionRepository;
    private readonly GameRepository _gameRepository;
    private readonly PlayerRepository _playerRepository;
    private readonly ModalErrorHandler _errorHandler;
    private int _playerId;
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
    private Player? _player;

    [ObservableProperty]
    private Game? _game;

    [ObservableProperty]
    private PlayerGameStats? _stats;

    [ObservableProperty]
    private bool _hasSessions;

    [ObservableProperty]
    private bool _hasClosedSessions;

    [ObservableProperty]
    private bool _hasRivals;

    /// <summary>Una curva necesita al menos dos sesiones.</summary>
    [ObservableProperty]
    private bool _hasEvolution;

    [ObservableProperty]
    private string _rankText = string.Empty;

    [ObservableProperty]
    private string _averageText = string.Empty;

    [ObservableProperty]
    private string _sessionWinRateText = string.Empty;

    [ObservableProperty]
    private string _winShareText = string.Empty;

    [ObservableProperty]
    private string _sessionsWonText = string.Empty;

    [ObservableProperty]
    private string _bestSessionText = string.Empty;

    /// <summary>Contexto de los duelos ganados: total disputado y porcentaje.</summary>
    [ObservableProperty]
    private string _duelsText = string.Empty;

    [ObservableProperty]
    private string _activityText = string.Empty;

    [ObservableProperty]
    private List<ChartItem> _radialItems = [];

    [ObservableProperty]
    private List<SessionPoint> _evolution = [];

    [ObservableProperty]
    private List<ChartItem> _headToHead = [];

    [ObservableProperty]
    private List<RivalRecord> _chartRivals = [];

    [ObservableProperty]
    private List<RivalRecord> _rivals = [];

    [ObservableProperty]
    private double _rivalsChartHeight;

    /// <summary>Con un solo rival las barras repiten la lista: el gráfico se muestra desde dos.</summary>
    [ObservableProperty]
    private bool _hasRivalsChart;

    [ObservableProperty]
    private RivalRecord? _nemesis;

    [ObservableProperty]
    private RivalRecord? _victim;

    // Separación entre Némesis y Víctima solo si se muestran ambas; sola, la tarjeta va a ras como el resto.
    [ObservableProperty]
    private Thickness _nemesisMargin;

    [ObservableProperty]
    private Thickness _victimMargin;

    [ObservableProperty]
    private string _nemesisText = string.Empty;

    [ObservableProperty]
    private string _victimText = string.Empty;

    // Pinceles de los gráficos: dependen del color del juego y del tema.
    [ObservableProperty]
    private List<Brush> _radialBrushes = [];

    [ObservableProperty]
    private List<Brush> _resultBrushes = [];

    [ObservableProperty]
    private Brush _gameBrush = Brush.Transparent;

    [ObservableProperty]
    private Brush _evolutionFill = Brush.Transparent;

    [ObservableProperty]
    private Color _shareColor = Colors.Transparent;

    [ObservableProperty]
    private Brush _winBrush = Brush.Transparent;

    [ObservableProperty]
    private Brush _drawBrush = Brush.Transparent;

    [ObservableProperty]
    private Brush _lossBrush = Brush.Transparent;

    public PlayerGameStatsPageModel(
        SessionRepository sessionRepository,
        GameRepository gameRepository,
        PlayerRepository playerRepository,
        ModalErrorHandler errorHandler)
    {
        _sessionRepository = sessionRepository;
        _gameRepository = gameRepository;
        _playerRepository = playerRepository;
        _errorHandler = errorHandler;

        WeakReferenceMessenger.Default.Register<AppThemeChangedMessage>(this, static (r, _) =>
        {
            var vm = (PlayerGameStatsPageModel)r;
            vm.Game?.NotifyThemeChanged();
            vm.Player?.NotifyThemeChanged();
            foreach (var rival in vm.Rivals)
                rival.Player.NotifyThemeChanged();
            vm.BuildBrushes();
        });
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("playerId", out var playerId) && query.TryGetValue("gameId", out var gameId))
        {
            _playerId = Convert.ToInt32(playerId);
            _gameId = Convert.ToInt32(gameId);
            LoadData().FireAndForgetSafeAsync(_errorHandler);
        }
        else
        {
            Shell.Current.GoToAsync("..").FireAndForgetSafeAsync(_errorHandler);
        }
    }

    // Recarga al volver (p. ej. tras editar el jugador o jugar más sesiones).
    [RelayCommand]
    private async Task Appearing()
    {
        if (_playerId > 0 && _gameId > 0)
            await LoadData();
    }

    private async Task LoadData()
    {
        try
        {
            Player = await _playerRepository.GetAsync(_playerId);
            Game = await _gameRepository.GetAsync(_gameId);

            if (Player is null || Game.IsNullOrNew())
            {
                _errorHandler.HandleError(new Exception("No se encontró el jugador o el juego."));
                await Shell.Current.GoToAsync("..");
                return;
            }

            Title = Player.Name;
            Period = StatsPeriodPreference.Current;
            EmptyText = StatsPeriodFilter.EmptyText(Period, "Todavía no jugó sesiones con victorias en este juego.");

            var sessions = StatsPeriodFilter.Apply(await _sessionRepository.ListAsync(_gameId), Period, DateTime.Now);
            ApplyStats(PlayerGameStatsCalculator.Compute(sessions, _playerId));
            BuildBrushes();
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }
    }

    private void ApplyStats(PlayerGameStats stats)
    {
        Stats = stats;
        HasSessions = stats.SessionsPlayed > 0;
        HasClosedSessions = stats.ClosedSessionsPlayed > 0;
        HasRivals = stats.Rivals.Count > 0;

        RankText = stats.Rank > 0 ? $"{stats.Rank}º de {stats.RankedPlayers}" : "Sin puesto";
        AverageText = stats.AverageWins.ToString("0.#");
        SessionWinRateText = $"{stats.SessionWinRate:P0}";
        WinShareText = $"{stats.WinShare:P0}";
        SessionsWonText = $"{stats.SessionsWon} de {stats.ClosedSessionsPlayed}";
        BestSessionText = stats.BestSessionDate is DateTime best
            ? $"{stats.BestSessionWins} victorias · {best:dd/MM/yyyy}"
            : "—";
        var duels = stats.HeadToHeadWins + stats.HeadToHeadDraws + stats.HeadToHeadLosses;
        DuelsText = duels == 0
            ? string.Empty
            : $"de {duels} {(duels == 1 ? "duelo" : "duelos")} contra otros jugadores · {(double)stats.HeadToHeadWins / duels:P0}";
        ActivityText = stats.FirstSessionDate is DateTime first && stats.LastSessionDate is DateTime last
            ? $"Primera sesión {first:dd/MM/yyyy} · última {last:dd/MM/yyyy}"
            : string.Empty;

        RadialItems =
        [
            new ChartItem("Sesiones ganadas", Math.Round(stats.SessionWinRate * 100)),
            new ChartItem("Victorias del grupo", Math.Round(stats.WinShare * 100))
        ];

        Evolution = stats.Evolution.ToList();
        HasEvolution = Evolution.Count >= 2;

        HeadToHead =
        [
            new ChartItem("Ganadas", stats.HeadToHeadWins),
            new ChartItem("Empatadas", stats.HeadToHeadDraws),
            new ChartItem("Perdidas", stats.HeadToHeadLosses)
        ];

        Rivals = stats.Rivals.ToList();
        // Transpuesto, la primera categoría queda abajo: se invierte para leer de arriba hacia abajo.
        ChartRivals = stats.Rivals.Take(ChartedRivals).Reverse().ToList();
        RivalsChartHeight = 40 + 44 * ChartRivals.Count;
        HasRivalsChart = ChartRivals.Count >= 2;
        Nemesis = stats.Nemesis;
        Victim = stats.Victim;
        var both = Nemesis is not null && Victim is not null;
        NemesisMargin = both ? new Thickness(0, 0, 4, 0) : Thickness.Zero;
        VictimMargin = both ? new Thickness(4, 0, 0, 0) : Thickness.Zero;
        NemesisText = Nemesis is null ? string.Empty : Duels(Nemesis.Losses, "perdido", "perdidos");
        VictimText = Victim is null ? string.Empty : Duels(Victim.Wins, "ganado", "ganados");
    }

    private static string Duels(int count, string singular, string plural) =>
        count == 1 ? $"1 duelo {singular}" : $"{count} duelos {plural}";

    private void BuildBrushes()
    {
        var gameColor = Game?.CurrentGameColor ?? Colors.Gray;
        var shareColor = ThemeResource("PodiumGold", "PodiumGoldDark");
        var win = ThemeResource("ResultWin", "ResultWinDark");
        var draw = ThemeResource("ResultDraw", "ResultDrawDark");
        var loss = ThemeResource("ResultLoss", "ResultLossDark");

        GameBrush = new SolidColorBrush(gameColor);
        ShareColor = shareColor;
        RadialBrushes = [new SolidColorBrush(gameColor), new SolidColorBrush(shareColor)];
        EvolutionFill = new LinearGradientBrush(
            [
                new GradientStop(gameColor.WithAlpha(0.45f), 0),
                new GradientStop(gameColor.WithAlpha(0.02f), 1)
            ],
            new Point(0, 0),
            new Point(0, 1));

        WinBrush = new SolidColorBrush(win);
        DrawBrush = new SolidColorBrush(draw);
        LossBrush = new SolidColorBrush(loss);
        ResultBrushes = [WinBrush, DrawBrush, LossBrush];
    }

    private static Color ThemeResource(string lightKey, string darkKey)
    {
        var app = Application.Current;
        var key = app?.RequestedTheme == AppTheme.Dark ? darkKey : lightKey;
        return app?.Resources.TryGetValue(key, out var value) == true && value is Color color
            ? color
            : Colors.Gray;
    }

    [RelayCommand]
    private async Task OpenGeneralStats()
    {
        if (_playerId <= 0)
            return;

        HapticFeedbackHelper.Click();
        await Shell.Current.GoToAsync($"playerstats?id={_playerId}");
    }

    [RelayCommand]
    private async Task OpenRival(RivalRecord? rival)
    {
        if (rival is null)
            return;

        HapticFeedbackHelper.Click();
        await Shell.Current.GoToAsync($"playergamestats?playerId={rival.Player.ID}&gameId={_gameId}");
    }
}
