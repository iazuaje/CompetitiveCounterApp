using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace CompetitiveCounterApp.Helpers;

/// <summary>
/// Aviso inferior con «Deshacer» tras una acción que se aplica al instante (victorias, quitar un jugador,
/// eliminar una sesión). Uno a la vez: el nuevo reemplaza al anterior, que ya no se puede deshacer.
/// </summary>
public static class UndoSnackbar
{
    static ISnackbar? _current;

    public static async Task ShowAsync(string message, Func<Task> undo, IErrorHandler errorHandler, double seconds = 4)
    {
        var snackbar = Snackbar.Make(
            message,
            () =>
            {
                HapticFeedbackHelper.Click();
                undo().FireAndForgetSafeAsync(errorHandler);
            },
            "Deshacer",
            TimeSpan.FromSeconds(seconds),
            Options());

        var previous = _current;
        _current = snackbar;
        if (previous is not null)
            await previous.Dismiss();

        await snackbar.Show();
    }

    /// <summary>Superficie invertida respecto del tema, con la acción en el color primario.</summary>
    static SnackbarOptions Options()
    {
        var dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        return new SnackbarOptions
        {
            BackgroundColor = Resource(dark ? "LightOnDarkBackground" : "DarkOnLightBackground"),
            TextColor = Resource(dark ? "DarkBackground" : "LightBackground"),
            ActionButtonTextColor = Resource(dark ? "Primary" : "PrimaryDark"),
            CornerRadius = new CornerRadius(14),
            Font = Microsoft.Maui.Font.SystemFontOfSize(15)
        };
    }

    static Color Resource(string key) =>
        Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color ? color : Colors.Gray;
}
