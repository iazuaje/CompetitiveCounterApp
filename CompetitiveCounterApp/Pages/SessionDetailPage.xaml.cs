namespace CompetitiveCounterApp.Pages;

public partial class SessionDetailPage : ContentPage
{
    public SessionDetailPage(SessionDetailPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
