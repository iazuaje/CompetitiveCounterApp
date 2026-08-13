using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompetitiveCounterApp.Models;
using System.Collections.ObjectModel;

namespace CompetitiveCounterApp.PageModels;


public abstract partial class GameFormPageModelBase : ObservableObject
{
    protected readonly GameRepository _gameRepository;
    protected readonly ModalErrorHandler _errorHandler;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private IconData _selectedIcon;

    [ObservableProperty]
    private ObservableCollection<IconData> _icons;

    [ObservableProperty]
    private GameColor _selectedColor;

    [ObservableProperty]
    private List<GameColor> _gameColors;

    [ObservableProperty]
    private bool _isBusy;

    protected GameFormPageModelBase(GameRepository gameRepository, ModalErrorHandler errorHandler)
    {
        _gameRepository = gameRepository;
        _errorHandler = errorHandler;
        _icons = new ObservableCollection<IconData>(GameDataService.GetIcons());
        _gameColors = GameDataService.GetGameColors();
        _selectedIcon = GameDataService.GetDefaultIcon();
        _selectedColor = GameDataService.GetDefaultColor();
        _selectedColor.IsSelected = true;
    }

    // Imagen seleccionada y manejo temporal (compartido)
    [ObservableProperty]
    private ImageSource? _selectedImage;

    protected string? _temporaryImagePath;

    [RelayCommand]
    public async Task SelectImage()
    {
        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync();
            if (result == null) return;

            if (!string.IsNullOrEmpty(_temporaryImagePath) && File.Exists(_temporaryImagePath))
                File.Delete(_temporaryImagePath);

            var tempPath = Path.Combine(FileSystem.CacheDirectory, $"temp_{Guid.NewGuid()}{Path.GetExtension(result.FileName)}");
            using var sourceStream = await result.OpenReadAsync();
            using var fileStream = File.Create(tempPath);
            await sourceStream.CopyToAsync(fileStream);

            _temporaryImagePath = tempPath;
            SelectedImage = ImageSource.FromFile(_temporaryImagePath);
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }
    }

    protected string? MoveTemporaryImageToPermanent()
    {
        if (string.IsNullOrEmpty(_temporaryImagePath) || !File.Exists(_temporaryImagePath))
            return null;

        var imagesDirectory = Path.Combine(FileSystem.AppDataDirectory, "GameImages");
        Directory.CreateDirectory(imagesDirectory);

        var fileName = $"game_{Guid.NewGuid()}{Path.GetExtension(_temporaryImagePath)}";
        var permanentPath = Path.Combine(imagesDirectory, fileName);

        File.Move(_temporaryImagePath, permanentPath);
        _temporaryImagePath = null;
        return permanentPath;
    }

    protected void SetSelectedImageFromPath(string? path)
    {
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
            SelectedImage = ImageSource.FromFile(path);
        else
            SelectedImage = null;
    }

    [RelayCommand]
    private void SelectIcon(IconData selectedIcon)
    {
        foreach(var i in Icons)
        {
            i.IsSelected = false;
        }

        selectedIcon.IsSelected = true;
        SelectedIcon = selectedIcon;
    }

    [RelayCommand]
    private void SelectColor(GameColor color)
    {
        foreach (var c in GameColors)
        {
            c.IsSelected = false;
        }
        color.IsSelected = true;
        SelectedColor = color;
    }

    protected bool ValidateForm()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }

    protected async Task<bool> ShowValidationErrorAsync(string message)
    {
        await AppShell.DisplayToastAsync(message);
        return false;
    }

    protected void ApplyGameData(Game game)
    {
        Name = game.Name;
        Description = game.Description;
        SelectedIcon = Icons.FirstOrDefault(i => i.Icon == game.Icon) ?? GameDataService.GetDefaultIcon();
        SelectedIcon.IsSelected = true;

        var matchingColor = GameColors.FirstOrDefault(c =>
            c.ColorLight == game.ColorLight && c.ColorDark == game.ColorDark);

        if (matchingColor != null)
        {
            matchingColor.IsSelected = true;
            SelectedColor = matchingColor;
        }
        else
        {
            SelectedColor = GameDataService.GetDefaultColor();
            SelectedColor.IsSelected = true;
        }
    }

    protected void UpdateGameFromForm(Game game)
    {
        game.Name = Name;
        game.Description = Description;
        game.Icon = SelectedIcon?.Icon ?? FluentUI.games_24_regular;
        game.ColorLight = SelectedColor?.ColorLight ?? "#E63946";
        game.ColorDark = SelectedColor?.ColorDark ?? "#FF5964";
    }
}