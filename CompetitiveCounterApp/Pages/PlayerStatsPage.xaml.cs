namespace CompetitiveCounterApp.Pages;

public partial class PlayerStatsPage : ContentPage
{
    public PlayerStatsPage(PlayerStatsPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
