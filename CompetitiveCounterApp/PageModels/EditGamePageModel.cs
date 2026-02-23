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

        try
        {
            IsBusy = true;

            UpdateGameFromForm(_game);
            
            await _gameRepository.SaveItemAsync(_game);

            await Shell.Current.GoToAsync("..");
            await AppShell.DisplayToastAsync("Juego actualizado exitosamente");
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
