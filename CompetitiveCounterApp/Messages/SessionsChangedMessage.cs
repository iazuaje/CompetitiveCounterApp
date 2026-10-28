namespace CompetitiveCounterApp.Messages;

/// <summary>
/// Cambiaron las sesiones de un juego fuera de la pantalla que las muestra (p. ej. «Deshacer» una sesión eliminada
/// desde su detalle, cuando ya se volvió al detalle del juego).
/// </summary>
public sealed record SessionsChangedMessage(int GameId);
