using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CompetitiveCounterApp.Helpers;
using CompetitiveCounterApp.Messages;
using CompetitiveCounterApp.Models;
using System.Collections.ObjectModel;

namespace CompetitiveCounterApp.PageModels;


public abstract partial class GameFormPageModelBase : ObservableObject
{
    protected readonly GameRepository _gameRepository;
    protected readonly CategoryRepository _categoryRepository;
    protected readonly ModalErrorHandler _errorHandler;

    /// <summary>Todas las categorías como chips; las seleccionadas se guardan con el juego.</summary>
    [ObservableProperty]
    private ObservableCollection<FilterChip> _categoryChips = [];

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
    private int _mediaSegmentIndex;

    public bool IsImageSegmentSelected => MediaSegmentIndex == 0;

    public bool IsIconSegmentSelected => MediaSegmentIndex == 1;

    [ObservableProperty]
    private bool _isBusy;

    protected GameFormPageModelBase(
        GameRepository gameRepository,
        CategoryRepository categoryRepository,
        ModalErrorHandler errorHandler)
    {
        _gameRepository = gameRepository;
        _categoryRepository = categoryRepository;
        _errorHandler = errorHandler;
        _icons = new ObservableCollection<IconData>(GameDataService.GetIcons());
        _gameColors = GameDataService.GetGameColors();
        _selectedIcon = GameDataService.GetDefaultIcon();
        _selectedColor = GameDataService.GetDefaultColor();
        _selectedColor.IsSelected = true;

        WeakReferenceMessenger.Default.Register<AppThemeChangedMessage>(this, static (r, _) =>
            ((GameFormPageModelBase)r).NotifyThemeChanged());
    }

    private void NotifyThemeChanged()
    {
        foreach (var color in GameColors)
            color.NotifyThemeChanged();

        SelectedColor?.NotifyThemeChanged();
    }

    partial void OnMediaSegmentIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsImageSegmentSelected));
        OnPropertyChanged(nameof(IsIconSegmentSelected));
    }

    // Imagen seleccionada y manejo temporal (compartido)
    [ObservableProperty]
    private ImageSource? _selectedImage;

    protected string? _temporaryImagePath;

    [RelayCommand]
    public async Task SelectImage()
    {
        HapticFeedbackHelper.Click();

        try
        {
            var results = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
            {
                SelectionLimit = 1
            });
            var result = results?.FirstOrDefault();
            if (result is null) return;

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
        HapticFeedbackHelper.Click();

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
        HapticFeedbackHelper.Click();

        foreach (var c in GameColors)
        {
            c.IsSelected = false;
        }
        color.IsSelected = true;
        SelectedColor = color;
    }

    /// <summary>Carga todas las categorías marcando como seleccionadas las del juego.</summary>
    protected async Task LoadCategoriesAsync(IEnumerable<int> selectedIds)
    {
        var selected = selectedIds.ToHashSet();
        var categories = await _categoryRepository.ListAsync();
        CategoryChips = new ObservableCollection<FilterChip>(
            categories.Select(c => FilterChip.ForCategory(c, selected.Contains(c.ID))));
    }

    /// <summary>Guarda las categorías seleccionadas para el juego (ya persistido).</summary>
    protected Task SaveCategoriesAsync(int gameId) =>
        _gameRepository.SetCategoriesAsync(
            gameId,
            CategoryChips.Where(c => c.IsSelected && c.Category is not null).Select(c => c.Category!.ID));

    [RelayCommand]
    private void ToggleCategory(FilterChip chip)
    {
        HapticFeedbackHelper.Click();
        chip.IsSelected = !chip.IsSelected;
    }

    [RelayCommand]
    private async Task AddCategory()
    {
        HapticFeedbackHelper.Click();

        try
        {
            var category = await CategoryPrompts.CreateAsync(_categoryRepository);
            if (category is null)
                return;

            // Si ya existía, se selecciona su chip en vez de duplicarlo.
            var existing = CategoryChips.FirstOrDefault(c => c.Category?.ID == category.ID);
            if (existing is not null)
            {
                existing.IsSelected = true;
                return;
            }

            CategoryChips.Add(FilterChip.ForCategory(category, isSelected: true));
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }
    }

    /// <summary>Mantener presionado un chip: renombrar o eliminar la categoría.</summary>
    [RelayCommand]
    private async Task CategoryOptions(FilterChip chip)
    {
        if (chip.Category is null)
            return;

        try
        {
            switch (await CategoryPrompts.ShowOptionsAsync(chip.Category, _categoryRepository))
            {
                case CategoryChange.Renamed:
                    chip.Label = chip.Category.Name;
                    break;
                case CategoryChange.Deleted:
                    CategoryChips.Remove(chip);
                    break;
            }
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }
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
            var existingColor = new GameColor
            {
                Name = "Color actual",
                ColorLight = game.ColorLight,
                ColorDark = game.ColorDark,
                IsSelected = true
            };

            GameColors.Insert(0, existingColor);
            SelectedColor = existingColor;
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