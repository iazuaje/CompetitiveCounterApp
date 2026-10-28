using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CompetitiveCounterApp.Helpers;
using CompetitiveCounterApp.Messages;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels;

/// <summary>Estadísticas generales de todos los juegos (sección "Estadísticas" del flyout).</summary>
public partial class GeneralStatsPageModel : ObservableObject
{
    private readonly GameRepository _gameRepository;
    private readonly SessionRepository _sessionRepository;
    private readonly ModalErrorHandler _errorHandler;

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
    private GeneralStats? _stats;

    [ObservableProperty]
    private bool _hasSessions;

    [ObservableProperty]
    private bool _hasRecords;

    [ObservableProperty]
    private string _busiestDayText = string.Empty;

    [ObservableProperty]
    private List<MonthPoint> _activity = [];

    [ObservableProperty]
    private List<GlobalRankingRow> _ranking = [];

    [ObservableProperty]
    private List<GameSummaryRow> _games = [];

    [ObservableProperty]
    private List<GameRecord> _records = [];

    public GeneralStatsPageModel(
        GameRepository gameRepository,
        SessionRepository sessionRepository,
        ModalErrorHandler errorHandler)
    {
        _gameRepository = gameRepository;
        _sessionRepository = sessionRepository;
        _errorHandler = errorHandler;

        WeakReferenceMessenger.Default.Register<AppThemeChangedMessage>(this, static (r, _) =>
        {
            var vm = (GeneralStatsPageModel)r;
            foreach (var row in vm.Ranking)
                row.Player.NotifyThemeChanged();
            foreach (var row in vm.Games)
                row.Game.NotifyThemeChanged();
        });
    }

    // Raíz del flyout: se recarga cada vez que se muestra para reflejar sesiones nuevas.
    [RelayCommand]
    private Task Appearing() => LoadData();

    private async Task LoadData()
    {
        try
        {
            Period = StatsPeriodPreference.Current;
            EmptyText = StatsPeriodFilter.EmptyText(Period, "Todavía no hay sesiones con victorias.");

            var games = await _gameRepository.ListAsync();
            var sessions = StatsPeriodFilter.Apply(await _sessionRepository.ListAsync(), Period, DateTime.Now);
            ApplyStats(GeneralStatsCalculator.Compute(games, sessions, DateTime.Now));
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }
    }

    private void ApplyStats(GeneralStats stats)
    {
        Stats = stats;
        HasSessions = stats.SessionsPlayed > 0;
        BusiestDayText = string.IsNullOrEmpty(stats.BusiestDay)
            ? string.Empty
            : $"El día con más sesiones es el {stats.BusiestDay}";
        Activity = stats.Activity.ToList();
        Ranking = stats.Ranking.ToList();
        Games = stats.Games.ToList();
        Records = stats.Records.ToList();
        HasRecords = Records.Count > 0;
    }

    [RelayCommand]
    private async Task OpenPlayer(GlobalRankingRow? row)
    {
        if (row is null)
            return;

        HapticFeedbackHelper.Click();
        await Shell.Current.GoToAsync($"playerstats?id={row.Player.ID}");
    }

    [RelayCommand]
    private async Task OpenGame(GameSummaryRow? row)
    {
        if (row is null)
            return;

        HapticFeedbackHelper.Click();
        await Shell.Current.GoToAsync($"gamestats?id={row.Game.ID}");
    }
}
