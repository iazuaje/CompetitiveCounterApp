namespace CompetitiveCounterApp.Pages;

public partial class GameDetailPage : ContentPage
{
	public GameDetailPage(GameDetailPageModel gameDetailPageModel)
	{
		InitializeComponent();
		BindingContext = gameDetailPageModel;
	}
}