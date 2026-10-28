using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CompetitiveCounterApp.Helpers;
using CompetitiveCounterApp.Messages;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels;

/// <summary>Estadísticas de un jugador en todos los juegos.</summary>
public partial class PlayerStatsPageModel : ObservableObject, IQueryAttributable
{
    private readonly PlayerRepository _playerRepository;
    private readonly GameRepository _gameRepository;
    private readonly SessionRepository _sessionRepository;
    private readonly ModalErrorHandler _errorHandler;
    private int _playerId;

    [ObservableProperty]
    private Player? _player;

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
    private PlayerGeneralStats? _stats;

    [ObservableProperty]
    private bool _hasSessions;

    [ObservableProperty]
    private bool _hasRivals;

    [ObservableProperty]
    private bool _hasSessionsWonByGame;

    [ObservableProperty]
    private bool _hasAchievements;

    [ObservableProperty]
    private string _rankText = string.Empty;

    [ObservableProperty]
    private string _gamesText = string.Empty;

    [ObservableProperty]
    private string _duelsText = string.Empty;

    [ObservableProperty]
    private string _sessionsWonText = string.Empty;

    [ObservableProperty]
    private List<PlayerGameRow> _games = [];

    [ObservableProperty]
    private List<GameShareSlice> _sessionsWonByGame = [];

    [ObservableProperty]
    private List<PlayerMonthPoint> _activity = [];

    [ObservableProperty]
    private List<RivalRecord> _rivals = [];

    [ObservableProperty]
    private RivalRecord? _nemesis;

    [ObservableProperty]
    private RivalRecord? _victim;

    [ObservableProperty]
    private string _nemesisText = string.Empty;

    [ObservableProperty]
    private string _victimText = string.Empty;

    [ObservableProperty]
    private Thickness _nemesisMargin;

    [ObservableProperty]
    private Thickness _victimMargin;

    [ObservableProperty]
    private List<GameRecord> _achievements = [];

    // Pinceles que dependen del color del jugador / juegos y del tema.
    [ObservableProperty]
    private Brush _playerBrush = Brush.Transparent;

    [ObservableProperty]
    private Brush _playedBrush = Brush.Transparent;

    [ObservableProperty]
    private List<Brush> _gameBrushes = [];

    public PlayerStatsPageModel(
        PlayerRepository playerRepository,
        GameRepository gameRepository,
        SessionRepository sessionRepository,
        ModalErrorHandler errorHandler)
    {
        _playerRepository = playerRepository;
        _gameRepository = gameRepository;
        _sessionRepository = sessionRepository;
        _errorHandler = errorHandler;

        WeakReferenceMessenger.Default.Register<AppThemeChangedMessage>(this, static (r, _) =>
        {
            var vm = (PlayerStatsPageModel)r;
            vm.Player?.NotifyThemeChanged();
            foreach (var row in vm.Games)
                row.Game.NotifyThemeChanged();
            foreach (var rival in vm.Rivals)
                rival.Player.NotifyThemeChanged();
            vm.BuildBrushes();
        });
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var value))
        {
            _playerId = Convert.ToInt32(value);
            LoadData().FireAndForgetSafeAsync(_errorHandler);
        }
        else
        {
            Shell.Current.GoToAsync("..").FireAndForgetSafeAsync(_errorHandler);
        }
    }

    // Recarga al volver de editar (nombre y color pueden haber cambiado) o de jugar más sesiones.
    [RelayCommand]
    private async Task Appearing()
    {
        if (_playerId > 0)
            await LoadData();
    }

    private async Task LoadData()
    {
        try
        {
            Player = await _playerRepository.GetAsync(_playerId);

            if (Player is null)
            {
                _errorHandler.HandleError(new Exception("No se encontró el jugador."));
                await Shell.Current.GoToAsync("..");
                return;
            }

            Title = Player.Name;

            Period = StatsPeriodPreference.Current;
            EmptyText = StatsPeriodFilter.EmptyText(Period, "Todavía no jugó sesiones con victorias.");

            var games = await _gameRepository.ListAsync();
            var sessions = StatsPeriodFilter.Apply(await _sessionRepository.ListAsync(), Period, DateTime.Now);
            ApplyStats(PlayerGeneralStatsCalculator.Compute(games, sessions, _playerId, DateTime.Now));
            BuildBrushes();
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }
    }

    private void ApplyStats(PlayerGeneralStats stats)
    {
        var overall = stats.Overall;

        Stats = stats;
        HasSessions = overall.SessionsPlayed > 0;
        RankText = overall.Rank > 0 ? $"{overall.Rank}º global" : "Sin puesto";
        GamesText = stats.GamesPlayed switch
        {
            0 => "Todavía sin sesiones",
            1 => "1 juego",
            _ => $"{stats.GamesPlayed} juegos"
        };

        var duels = overall.HeadToHeadWins + overall.HeadToHeadDraws + overall.HeadToHeadLosses;
        HasRivals = duels > 0;
        DuelsText = duels == 0
            ? string.Empty
            : $"de {duels} {(duels == 1 ? "duelo" : "duelos")} en todos los juegos · {(double)overall.HeadToHeadWins / duels:P0}";
        SessionsWonText = $"{overall.SessionsWon} de {overall.ClosedSessionsPlayed}";

        Games = stats.Games.ToList();
        SessionsWonByGame = stats.SessionsWonByGame.ToList();
        HasSessionsWonByGame = SessionsWonByGame.Count > 0;
        Activity = stats.Activity.ToList();

        Rivals = overall.Rivals.ToList();
        Nemesis = overall.Nemesis;
        Victim = overall.Victim;
        NemesisText = Nemesis is null ? string.Empty : Duels(Nemesis.Losses, "perdido", "perdidos");
        VictimText = Victim is null ? string.Empty : Duels(Victim.Wins, "ganado", "ganados");
        var both = Nemesis is not null && Victim is not null;
        NemesisMargin = both ? new Thickness(0, 0, 4, 0) : Thickness.Zero;
        VictimMargin = both ? new Thickness(4, 0, 0, 0) : Thickness.Zero;

        Achievements = stats.Achievements.ToList();
        HasAchievements = Achievements.Count > 0;
    }

    private static string Duels(int count, string singular, string plural) =>
        count == 1 ? $"1 duelo {singular}" : $"{count} duelos {plural}";

    private void BuildBrushes()
    {
        PlayerBrush = new SolidColorBrush(Player?.CurrentColor ?? Colors.Gray);
        PlayedBrush = new SolidColorBrush(ThemeResource("Gray300", "Gray600"));
        GameBrushes = SessionsWonByGame.Select(s => (Brush)new SolidColorBrush(s.Game.CurrentGameColor)).ToList();
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
    private async Task Edit()
    {
        if (_playerId <= 0)
            return;

        HapticFeedbackHelper.Click();
        await Shell.Current.GoToAsync($"editplayer?id={_playerId}");
    }

    [RelayCommand]
    private async Task OpenGame(PlayerGameRow? row)
    {
        if (row is null)
            return;

        HapticFeedbackHelper.Click();
        await Shell.Current.GoToAsync($"playergamestats?playerId={_playerId}&gameId={row.Game.ID}");
    }

    [RelayCommand]
    private async Task OpenRival(RivalRecord? rival)
    {
        if (rival is null)
            return;

        HapticFeedbackHelper.Click();
        await Shell.Current.GoToAsync($"playerstats?id={rival.Player.ID}");
    }
}
