namespace CompetitiveCounterApp.Pages;

public partial class CreateGamePage : ContentPage
{
	public CreateGamePage(CreateGamePageModel createGamePageModel)
	{
		InitializeComponent();
		BindingContext = createGamePageModel;
	}

    private void SfSegmentedControl_SelectionChanged(object sender, Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
    {
		//Do something
    }
}
