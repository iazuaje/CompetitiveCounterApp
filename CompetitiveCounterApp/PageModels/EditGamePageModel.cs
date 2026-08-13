using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels;

public partial class EditGamePageModel : GameFormPageModelBase, IQueryAttributable
{
    private Game? _game;
    private readonly GameOperationsService _gameOperations;

    public EditGamePageModel(
        GameRepository gameRepository, 
        ModalErrorHandler errorHandler,
        GameOperationsService gameOperations)
        : base(gameRepository, errorHandler)
    {
        _gameOperations = gameOperations;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("id"))
        {
            int id = Convert.ToInt32(query["id"]);
            LoadData(id).FireAndForgetSafeAsync(_errorHandler);
        }
        else
        {
            Shell.Current.GoToAsync("..").FireAndForgetSafeAsync(_errorHandler);
        }
    }

    private async Task LoadData(int id)
    {
        try
        {
            IsBusy = true;

            _game = await _gameRepository.GetAsync(id);

            if (_game.IsNullOrNew())
            {
                _errorHandler.HandleError(new Exception($"Game with id {id} could not be found."));
                await Shell.Current.GoToAsync("..");
                return;
            }

            ApplyGameData(_game);
            SetSelectedImageFromPath(_game.ImagePath);
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (_game is null)
        {
            _errorHandler.HandleError(new Exception("Game is null. Cannot Save."));
            return;
        }

        if (!ValidateForm())
        {
            await ShowValidationErrorAsync("El nombre del juego es requerido");
            return;
        }

        string? previousImagePath = null;
        string? newImagePath = null;
        bool gameSaved = false;

        try
        {
            IsBusy = true;

            UpdateGameFromForm(_game);

            newImagePath = MoveTemporaryImageToPermanent();
            if (!string.IsNullOrEmpty(newImagePath))
            {
                previousImagePath = _game.ImagePath;
                _game.ImagePath = newImagePath;
            }
            
            await _gameRepository.SaveItemAsync(_game);
            gameSaved = true;

            if (!string.IsNullOrEmpty(previousImagePath) && File.Exists(previousImagePath))
                File.Delete(previousImagePath);

            await Shell.Current.GoToAsync("..");
            await AppShell.DisplayToastAsync("Juego actualizado exitosamente");
        }
        catch (Exception e)
        {
            if (!gameSaved && !string.IsNullOrEmpty(newImagePath))
            {
                _game.ImagePath = previousImagePath ?? string.Empty;
                if (File.Exists(newImagePath))
                    File.Delete(newImagePath);
                SetSelectedImageFromPath(previousImagePath);
            }

            _errorHandler.HandleError(e);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Delete()
    {
        try
        {
            IsBusy = true;
            await _gameOperations.DeleteGameAsync(_game, _errorHandler);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
