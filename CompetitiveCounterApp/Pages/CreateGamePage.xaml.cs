namespace CompetitiveCounterApp.Pages;

public partial class CreateGamePage : ContentPage
{
	bool isSelectingImage = true;
	public CreateGamePage(CreateGamePageModel createGamePageModel)
	{
		InitializeComponent();
		BindingContext = createGamePageModel;

		selectorImagen.IsVisible = isSelectingImage;
    }

    private void SfSegmentedControl_SelectionChanged(object sender, Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
    {
		//Do something
    }
}
