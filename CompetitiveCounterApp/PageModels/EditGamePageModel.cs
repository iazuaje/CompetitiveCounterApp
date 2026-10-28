using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompetitiveCounterApp.Helpers;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels;

public partial class EditGamePageModel : GameFormPageModelBase, IQueryAttributable
{
    private Game? _game;
    private readonly GameOperationsService _gameOperations;

    public EditGamePageModel(
        GameRepository gameRepository,
        CategoryRepository categoryRepository,
        ModalErrorHandler errorHandler,
        GameOperationsService gameOperations)
        : base(gameRepository, categoryRepository, errorHandler)
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
            ArchiveText = _game.IsArchived ? "Desarchivar" : "Archivar";
            SetSelectedImageFromPath(_game.ImagePath);
            await LoadCategoriesAsync(_game.Categories.Select(c => c.ID));
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
        HapticFeedbackHelper.Click();

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
            await SaveCategoriesAsync(_game.ID);

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

    /// <summary>Texto del botón de la barra según el estado del juego.</summary>
    [ObservableProperty]
    private string _archiveText = "Archivar";

    [RelayCommand]
    private async Task ToggleArchive()
    {
        if (_game is null)
            return;

        HapticFeedbackHelper.Click();
        var archive = !_game.IsArchived;

        if (archive && !await Shell.Current.DisplayAlertAsync(
                "Archivar juego",
                $"«{_game.Name}» se ocultará de la lista de juegos. Sus sesiones siguen contando en las estadísticas y puedes verlo con el filtro «Archivados».",
                "Archivar",
                "Cancelar"))
            return;

        try
        {
            IsBusy = true;
            await _gameRepository.SetArchivedAsync(_game.ID, archive);
            _game.IsArchived = archive;
            await Shell.Current.GoToAsync("..");
            await AppShell.DisplayToastAsync(archive ? "Juego archivado" : "Juego restaurado");
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
            HapticFeedbackHelper.Click();
            IsBusy = true;
            await _gameOperations.DeleteGameAsync(_game, _errorHandler);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
