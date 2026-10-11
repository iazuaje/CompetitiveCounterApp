namespace CompetitiveCounterApp.Pages;

public partial class PlayerGameStatsPage : ContentPage
{
    public PlayerGameStatsPage(PlayerGameStatsPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
