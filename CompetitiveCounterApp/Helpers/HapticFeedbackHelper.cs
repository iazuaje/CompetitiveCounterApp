namespace CompetitiveCounterApp.Helpers;

public static class HapticFeedbackHelper
{
    public static void Click() => Perform(HapticFeedbackType.Click);

    public static void LongPress() => Perform(HapticFeedbackType.LongPress);

    static void Perform(HapticFeedbackType type)
    {
        try
        {
            HapticFeedback.Default.Perform(type);
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
