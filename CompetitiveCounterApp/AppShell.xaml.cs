using System.ComponentModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Platform;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp
{
    public partial class AppShell : Shell
    {
        private Page? _statusBarPage;

        public AppShell()
        {
            InitializeComponent();
            var currentTheme = Application.Current!.RequestedTheme;
            ThemeSegmentedControl.SelectedIndex = currentTheme == AppTheme.Light ? 0 : 1;
        }

        public static async Task DisplayToastAsync(string message)
        {
            var toast = Toast.Make(message, textSize: 18);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await toast.Show(cts.Token);
        }

        private void SfSegmentedControl_SelectionChanged(object sender, Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
        {
            Application.Current!.UserAppTheme = e.NewIndex == 0 ? AppTheme.Light : AppTheme.Dark;
        }

        protected override void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);
            TrackStatusBarPage(CurrentPage);
        }

        // La barra de estado del sistema copia el Shell.BackgroundColor de la página visible,
        // incluso cuando cambia por binding (carga de datos, color elegido en un formulario, tema).
        private void TrackStatusBarPage(Page? page)
        {
            if (_statusBarPage is not null)
                _statusBarPage.PropertyChanged -= OnStatusBarPagePropertyChanged;

            _statusBarPage = page;

            if (_statusBarPage is not null)
                _statusBarPage.PropertyChanged += OnStatusBarPagePropertyChanged;

            UpdateStatusBar();
        }

        private void OnStatusBarPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Shell.BackgroundColorProperty.PropertyName)
                UpdateStatusBar();
        }

        private void UpdateStatusBar()
        {
            if (_statusBarPage is null || Shell.GetBackgroundColor(_statusBarPage) is not Color color)
                return;

            // El Toolkit solo soporta la barra de estado desde Android 6.0 (API 23).
            if (OperatingSystem.IsAndroidVersionAtLeast(23) || OperatingSystem.IsIOS())
            {
                StatusBar.SetColor(color);
                StatusBar.SetStyle(ThemeColorPair.PrefersDarkText(color)
                    ? StatusBarStyle.DarkContent
                    : StatusBarStyle.LightContent);
            }
        }
    }
}
