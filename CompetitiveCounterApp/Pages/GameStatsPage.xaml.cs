namespace CompetitiveCounterApp.Pages;

public partial class GameStatsPage : ContentPage
{
    public GameStatsPage(GameStatsPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
