using CommunityToolkit.Mvvm.Messaging;
using CompetitiveCounterApp.Helpers;
using CompetitiveCounterApp.Messages;

namespace CompetitiveCounterApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            UserAppTheme = ThemePreference.Current;
            RequestedThemeChanged += OnRequestedThemeChanged;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        private static void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new AppThemeChangedMessage());
        }
    }
}