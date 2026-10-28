namespace CompetitiveCounterApp.Helpers;

/// <summary>
/// Confirmación compartida al crear una sesión (nueva o revancha) cuando el juego ya tiene una activa.
/// Misma regla que <c>SessionRepository.CreateAsync</c>: con victorias se cierra, sin victorias se descarta.
/// </summary>
public static class NewSessionPrompt
{
    public static Task<bool> ConfirmReplaceActiveAsync(bool activeHasWins, string title = "Nueva sesión")
    {
        var message = activeHasWins
            ? "Hay una sesión activa. Al crear una nueva se cerrará la actual. ¿Continuar?"
            : "Hay una sesión activa sin victorias. Al crear una nueva se descartará la actual. ¿Continuar?";

        return Shell.Current.DisplayAlertAsync(title, message, "Crear", "Cancelar");
    }
}
