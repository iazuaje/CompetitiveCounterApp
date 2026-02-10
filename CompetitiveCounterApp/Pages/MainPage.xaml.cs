using CompetitiveCounterApp.Models;
using CompetitiveCounterApp.PageModels;

namespace CompetitiveCounterApp.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}