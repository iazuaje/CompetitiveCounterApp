namespace CompetitiveCounterApp.Pages;

public partial class PlayersPage : ContentPage
{
    public PlayersPage(PlayersPageModel playersPageModel)
    {
        InitializeComponent();
        BindingContext = playersPageModel;
    }
}
