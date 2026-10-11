namespace CompetitiveCounterApp.Helpers;

/// <summary>Confirmación compartida para eliminar una sesión (detalle de sesión y detalle del juego).</summary>
public static class SessionDeletePrompt
{
    public static Task<bool> ConfirmAsync(int totalWins)
    {
        var detail = totalWins switch
        {
            0 => "No tiene victorias registradas.",
            1 => "Se perderá 1 victoria y el ranking del juego se recalculará.",
            _ => $"Se perderán {totalWins} victorias y el ranking del juego se recalculará."
        };

        return Shell.Current.DisplayAlertAsync(
            "Eliminar sesión",
            $"¿Eliminar esta sesión? {detail} Esta acción no se puede deshacer.",
            "Eliminar",
            "Cancelar");
    }
}
