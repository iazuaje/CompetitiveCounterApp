using CompetitiveCounterApp.Behaviors;
using CompetitiveCounterApp.Helpers;

namespace CompetitiveCounterApp.Pages.Controls;

/// <summary>
/// Selector de período de las estadísticas: una fila de chips (Todo / Este año / Este mes).
/// <see cref="Period"/> es TwoWay por defecto.
/// </summary>
public class PeriodSelector : ContentView
{
    public static readonly BindableProperty PeriodProperty =
        BindableProperty.Create(nameof(Period), typeof(StatsPeriod), typeof(PeriodSelector), StatsPeriod.All,
            BindingMode.TwoWay, propertyChanged: (b, _, _) => ((PeriodSelector)b).UpdateSelection());

    public StatsPeriod Period { get => (StatsPeriod)GetValue(PeriodProperty); set => SetValue(PeriodProperty, value); }

    private readonly List<(StatsPeriod Period, Chip Chip)> _chips = [];

    public PeriodSelector()
    {
        var row = new HorizontalStackLayout { Spacing = 8 };

        foreach (var period in new[] { StatsPeriod.All, StatsPeriod.Year, StatsPeriod.Month })
        {
            var chip = new Chip { Text = StatsPeriodFilter.Label(period) };
            AnimatedTap.SetCommand(chip, new Command(() =>
            {
                if (Period == period)
                    return;

                HapticFeedbackHelper.Click();
                Period = period;
            }));
            _chips.Add((period, chip));
            row.Children.Add(chip);
        }

        Content = row;
        UpdateSelection();
    }

    private void UpdateSelection()
    {
        foreach (var (period, chip) in _chips)
            chip.IsSelected = period == Period;
    }
}
