namespace CompetitiveCounterApp.Helpers;

public static class HapticFeedbackHelper
{
    public static void Click()
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
        }
        catch (FeatureNotSupportedException)
        {
            // Plataforma sin háptica.
        }
        catch (Exception)
        {
            // Permiso denegado u otro fallo: no bloquear la UI.
        }
    }
}
