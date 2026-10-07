namespace CompetitiveCounterApp.Pages;

public partial class GeneralStatsPage : ContentPage
{
    public GeneralStatsPage(GeneralStatsPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
