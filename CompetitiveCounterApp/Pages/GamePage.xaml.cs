namespace CompetitiveCounterApp.Pages;

public partial class GamesPage : ContentPage
{
	public GamesPage(GamesPageModel gamesPageModel)
	{
		InitializeComponent();
		BindingContext = gamesPageModel;
	}
}