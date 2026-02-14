namespace CompetitiveCounterApp.Pages;

public partial class EditGamePage : ContentPage
{
	public EditGamePage(EditGamePageModel editGamePageModel)
	{
		InitializeComponent();
		BindingContext = editGamePageModel;
	}
}
