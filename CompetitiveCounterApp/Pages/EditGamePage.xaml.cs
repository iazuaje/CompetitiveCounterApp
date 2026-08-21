namespace CompetitiveCounterApp.Pages;

public partial class EditGamePage : ContentPage
{
	public EditGamePage(EditGamePageModel editGamePageModel)
	{
		InitializeComponent();
		BindingContext = editGamePageModel;
	}

    private void MediaSegment_SelectionChanged(object sender, Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
    {
        if (BindingContext is PageModels.GameFormPageModelBase viewModel
            && e.NewIndex is int newIndex
            && viewModel.MediaSegmentIndex != newIndex)
        {
            viewModel.MediaSegmentIndex = newIndex;
        }
    }
}
