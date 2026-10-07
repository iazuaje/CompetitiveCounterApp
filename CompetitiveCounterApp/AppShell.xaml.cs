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

        // Sigue los colores de barra de la página visible, incluso cuando cambian por binding
        // (carga de datos, color elegido en un formulario, tema):
        // - la barra de estado del sistema copia Shell.BackgroundColor;
        // - los íconos de la barra toman Shell.ForegroundColor (Android no los tiñe solo).
        private void TrackStatusBarPage(Page? page)
        {
            if (_statusBarPage is not null)
            {
                _statusBarPage.PropertyChanged -= OnStatusBarPagePropertyChanged;
                _statusBarPage.Appearing -= OnStatusBarPageAppearing;
            }

            _statusBarPage = page;

            if (_statusBarPage is not null)
            {
                _statusBarPage.PropertyChanged += OnStatusBarPagePropertyChanged;
                _statusBarPage.Appearing += OnStatusBarPageAppearing;
            }

            UpdateStatusBar();
            UpdateToolbarIcons();
        }

        // En el arranque la barra nativa se arma después de OnNavigated: se reaplica al aparecer.
        private void OnStatusBarPageAppearing(object? sender, EventArgs e)
        {
            Dispatcher.Dispatch(() =>
            {
                UpdateStatusBar();
                UpdateToolbarIcons(force: true);
            });
        }

        private void OnStatusBarPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Shell.BackgroundColorProperty.PropertyName)
                UpdateStatusBar();
            else if (e.PropertyName == Shell.ForegroundColorProperty.PropertyName)
                UpdateToolbarIcons();
        }

        private void UpdateToolbarIcons(bool force = false)
        {
            if (_statusBarPage is null || Shell.GetForegroundColor(_statusBarPage) is not Color color)
                return;

            // Copia del FontImageSource: los de Icons.xaml son recursos compartidos entre páginas.
            // force: reasigna aunque el color coincida, para que la barra nativa recién creada lo tome.
            foreach (var item in _statusBarPage.ToolbarItems)
            {
                if (item.IconImageSource is FontImageSource icon && (force || !color.Equals(icon.Color)))
                {
                    item.IconImageSource = new FontImageSource
                    {
                        Glyph = icon.Glyph,
                        FontFamily = icon.FontFamily,
                        Size = icon.Size,
                        Color = color
                    };
                }
            }
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
