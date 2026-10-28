namespace CompetitiveCounterApp.Pages;

public partial class BackupPage : ContentPage
{
    public BackupPage(BackupPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
