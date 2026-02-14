namespace CompetitiveCounterApp.Pages;

public partial class CreateGamePage : ContentPage
{
	public CreateGamePage(CreateGamePageModel createGamePageModel)
	{
		InitializeComponent();
		BindingContext = createGamePageModel;
	}
}
