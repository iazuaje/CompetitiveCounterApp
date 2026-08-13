namespace CompetitiveCounterApp.Pages;

public partial class EditGamePage : ContentPage
{
	bool isSelectingImage = true;

	public EditGamePage(EditGamePageModel editGamePageModel)
	{
		InitializeComponent();
		BindingContext = editGamePageModel;

		selectorImagen.IsVisible = isSelectingImage;
	}

    private void SfSegmentedControl_SelectionChanged(object sender, Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
    {
		isSelectingImage = e.NewIndex == 0;
		selectorImagen.IsVisible = isSelectingImage;
		selectorIcon.IsVisible = !isSelectingImage;
    }
}
