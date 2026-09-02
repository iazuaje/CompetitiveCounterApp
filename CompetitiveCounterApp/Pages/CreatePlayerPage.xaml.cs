namespace CompetitiveCounterApp.Pages;

public partial class CreatePlayerPage : ContentPage
{
    public CreatePlayerPage(CreatePlayerPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
