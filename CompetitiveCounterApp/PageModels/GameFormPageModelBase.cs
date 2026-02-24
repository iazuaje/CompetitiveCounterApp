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