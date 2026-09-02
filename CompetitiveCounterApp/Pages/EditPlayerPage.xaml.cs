namespace CompetitiveCounterApp.Pages;

public partial class EditPlayerPage : ContentPage
{
    public EditPlayerPage(EditPlayerPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
