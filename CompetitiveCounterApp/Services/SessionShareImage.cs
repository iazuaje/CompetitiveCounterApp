using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.Services;

/// <summary>
/// Imagen PNG con el resultado de una sesión para compartir (juego, fecha y tabla de victorias, con corona
/// para quien ganó; empate en el máximo corona a todos). Se dibuja siempre con los colores claros del juego,
/// sin depender del tema de la app.
/// </summary>
public static class SessionShareImage
{
    const int Width = 1080;
    const float Padding = 64;
    const float HeaderHeight = 250;
    const float RowHeight = 116;
    const float FooterHeight = 110;

    public static async Task<string> CreateAsync(Session session, IReadOnlyList<SessionPlayer> players)
    {
        var path = Path.Combine(FileSystem.CacheDirectory, $"sesion-{session.ID}.png");

        await Task.Run(() =>
        {
            var rows = players
                .Where(sp => sp.Player is not null)
                .OrderByDescending(sp => sp.Wins)
                .ThenBy(sp => sp.Player!.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            var height = (int)(HeaderHeight + Padding + rows.Count * RowHeight + Padding / 2 + FooterHeight);

#if ANDROID
            using var context = new Microsoft.Maui.Graphics.Platform.PlatformBitmapExportService().CreateContext(Width, height);
            Draw(context.Canvas, session, rows, height);
            using var stream = File.Create(path);
            context.WriteToStream(stream);
#else
            throw new PlatformNotSupportedException("Compartir resultados solo está disponible en Android.");
#endif
        });

        return path;
    }

    static void Draw(ICanvas canvas, Session session, List<SessionPlayer> rows, int height)
    {
        var gameColor = session.Game?.GameColorLight ?? Color.FromArgb("#C45C3E");
        var onGame = ThemeColorPair.ContrastingTextColor(gameColor);
        var surface = Color.FromArgb("#FFF8F3");
        var text = Color.FromArgb("#3D3229");
        var muted = Color.FromArgb("#85766A");
        var maxWins = rows.Count > 0 ? rows[0].Wins : 0;

        canvas.FillColor = gameColor;
        canvas.FillRectangle(0, 0, Width, height);

        // Encabezado: juego y fecha.
        canvas.FontColor = onGame;
        canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
        canvas.FontSize = 64;
        canvas.DrawString(session.Game?.Name ?? "Sesión", Padding, Padding, Width - Padding * 2, 90,
            HorizontalAlignment.Left, VerticalAlignment.Center);

        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontSize = 36;
        var state = session.IsActive ? "en curso" : "resultado final";
        canvas.DrawString($"{session.SessionDateLocal:dd/MM/yyyy HH:mm} · {state}", Padding, Padding + 96, Width - Padding * 2, 50,
            HorizontalAlignment.Left, VerticalAlignment.Center);

        // Tabla sobre una superficie clara.
        var top = HeaderHeight;
        canvas.FillColor = surface;
        canvas.FillRoundedRectangle(Padding / 2, top, Width - Padding, Padding / 2 + rows.Count * RowHeight + Padding / 2, 36);

        var rank = 0;
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (i == 0 || row.Wins != rows[i - 1].Wins)
                rank = i + 1;

            var y = top + Padding / 2 + i * RowHeight;
            var winner = maxWins > 0 && row.Wins == maxWins;

            if (winner)
            {
                canvas.FillColor = gameColor.WithAlpha(0.14f);
                canvas.FillRoundedRectangle(Padding / 2 + 16, y + 8, Width - Padding - 32, RowHeight - 16, 24);
            }

            canvas.FontColor = muted;
            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.FontSize = 40;
            canvas.DrawString($"{rank}", Padding, y, 70, RowHeight, HorizontalAlignment.Center, VerticalAlignment.Center);

            var playerColor = row.Player!.ThemeColors.LightThemeColor;
            canvas.FillColor = playerColor;
            canvas.FillCircle(Padding + 120, y + RowHeight / 2, 30);
            canvas.FontColor = ThemeColorPair.ContrastingTextColor(playerColor);
            canvas.FontSize = 30;
            canvas.DrawString(Initial(row.Player.Name), Padding + 90, y + RowHeight / 2 - 30, 60, 60,
                HorizontalAlignment.Center, VerticalAlignment.Center);

            canvas.FontColor = text;
            canvas.Font = winner ? Microsoft.Maui.Graphics.Font.DefaultBold : Microsoft.Maui.Graphics.Font.Default;
            canvas.FontSize = 44;
            canvas.DrawString(winner ? $"{row.Player.Name} 👑" : row.Player.Name, Padding + 180, y, Width - Padding * 2 - 180 - 200, RowHeight,
                HorizontalAlignment.Left, VerticalAlignment.Center);

            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.FontSize = 48;
            canvas.DrawString(row.Wins.ToString(), Width - Padding - 200, y + 14, 140, 60, HorizontalAlignment.Right, VerticalAlignment.Center);
            canvas.Font = Microsoft.Maui.Graphics.Font.Default;
            canvas.FontSize = 26;
            canvas.FontColor = muted;
            canvas.DrawString(row.Wins == 1 ? "victoria" : "victorias", Width - Padding - 200, y + 74, 140, 30,
                HorizontalAlignment.Right, VerticalAlignment.Center);
        }

        canvas.FontColor = onGame.WithAlpha(0.85f);
        canvas.Font = Microsoft.Maui.Graphics.Font.Default;
        canvas.FontSize = 30;
        canvas.DrawString("CompetitiveCounter", Padding, height - FooterHeight, Width - Padding * 2, FooterHeight,
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    static string Initial(string name) =>
        string.IsNullOrWhiteSpace(name) ? "?" : name.Trim()[..1].ToUpperInvariant();
}
