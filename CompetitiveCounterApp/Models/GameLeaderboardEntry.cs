using CommunityToolkit.Mvvm.ComponentModel;

namespace CompetitiveCounterApp.Models;

public partial class GameLeaderboardEntry : ObservableObject
{
    public int Rank { get; init; }
    public int PlayerId { get; init; }
    public string PlayerName { get; init; } = string.Empty;
    public string PlayerIcon { get; init; } = string.Empty;
    public int SessionsWon { get; init; }
    public int TotalWins { get; init; }
    public string TotalWinsText => TotalWins == 1 ? "1 victoria" : $"{TotalWins} victorias";
    public ThemeColorPair PlayerColors { get; init; }
    public ThemeColorPair MedalColors { get; init; }

    public bool IsFirst => Rank == 1;

    public Color PlayerColor => PlayerColors.CurrentColor;
    public Color OnPlayerColor => PlayerColors.OnColor;
    public Color MedalColor => MedalColors.CurrentColor;
    public Color MedalTextColor => MedalColors.OnColor;

    public void NotifyThemeChanged()
    {
        OnPropertyChanged(nameof(PlayerColor));
        OnPropertyChanged(nameof(OnPlayerColor));
        OnPropertyChanged(nameof(MedalColor));
        OnPropertyChanged(nameof(MedalTextColor));
    }
}
