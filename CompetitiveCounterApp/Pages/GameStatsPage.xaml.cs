using System.ComponentModel;
using CompetitiveCounterApp.Models;
using Syncfusion.Maui.Toolkit.Charts;

namespace CompetitiveCounterApp.Pages;

public partial class GameStatsPage : ContentPage
{
    private readonly GameStatsPageModel _viewModel;

    public GameStatsPage(GameStatsPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    // La cantidad de jugadores es variable y el toolkit no enlaza series desde una colección:
    // una serie apilada por jugador, con su color, se arma acá cada vez que cambia el historial.
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(GameStatsPageModel.History))
            return;

        HistoryChart.Series.Clear();
        foreach (var series in _viewModel.History)
            HistoryChart.Series.Add(CreateSeries(series));
    }

    private static StackingColumnSeries CreateSeries(HistorySeries data) => new()
    {
        ItemsSource = data.Points,
        XBindingPath = nameof(HistoryPoint.Label),
        YBindingPath = nameof(HistoryPoint.Wins),
        Fill = new SolidColorBrush(data.Player.CurrentColor),
        Label = data.Player.Name,
        Width = 0.6,
        EnableTooltip = true,
        EnableAnimation = true
    };
}
