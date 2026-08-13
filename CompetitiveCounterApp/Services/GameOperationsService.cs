using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.Services;

public class GameOperationsService
{
    private readonly GameRepository _gameRepository;

    public GameOperationsService(GameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<bool> DeleteGameAsync(Game? game, ModalErrorHandler errorHandler)
    {
        if (game.IsNullOrNew())
        {
            await Shell.Current.GoToAsync("..");
            return false;
        }

        bool confirm = await Shell.Current.DisplayAlert(
            "Eliminar Juego",
            $"ùEstùs seguro de eliminar '{game.Name}'? Esto eliminarù todas las sesiones asociadas.",
            "Sù",
            "No");

        if (!confirm) return false;

        try
        {
            await _gameRepository.DeleteItemAsync(game);

            try
            {
                if (!string.IsNullOrEmpty(game.ImagePath) && File.Exists(game.ImagePath))
                    File.Delete(game.ImagePath);
            }
            catch (Exception e)
            {
                errorHandler.HandleError(e);
            }

            await Shell.Current.GoToAsync("..");
            await AppShell.DisplayToastAsync("Juego eliminado exitosamente");
            return true;
        }
        catch (Exception e)
        {
            errorHandler.HandleError(e);
            return false;
        }
    }
}