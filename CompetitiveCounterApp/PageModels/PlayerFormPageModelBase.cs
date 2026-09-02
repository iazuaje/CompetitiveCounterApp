using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CompetitiveCounterApp.Messages;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels
{
    public abstract partial class PlayerFormPageModelBase : ObservableObject
    {
        protected readonly PlayerRepository _playerRepository;
        protected readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private List<GameColor> _playerColors = [];

        [ObservableProperty]
        private GameColor _selectedColor;

        [ObservableProperty]
        private bool _isBusy;

        protected PlayerFormPageModelBase(PlayerRepository playerRepository, ModalErrorHandler errorHandler)
        {
            _playerRepository = playerRepository;
            _errorHandler = errorHandler;
            PlayerColors = GameDataService.GetGameColors();
            SelectedColor = PlayerColors[0];
            SelectedColor.IsSelected = true;

            WeakReferenceMessenger.Default.Register<AppThemeChangedMessage>(this, static (r, _) =>
                ((PlayerFormPageModelBase)r).NotifyThemeChanged());
        }

        private void NotifyThemeChanged()
        {
            foreach (var color in PlayerColors)
                color.NotifyThemeChanged();

            SelectedColor?.NotifyThemeChanged();
        }

        [RelayCommand]
        private void SelectColor(GameColor color)
        {
            foreach (var c in PlayerColors)
                c.IsSelected = false;

            color.IsSelected = true;
            SelectedColor = color;
        }

        protected bool ValidateForm() => !string.IsNullOrWhiteSpace(Name);

        protected void ApplyPlayerData(Player player)
        {
            Name = player.Name;
            var match = PlayerColors.FirstOrDefault(c =>
                c.ColorLight == player.ColorLight && c.ColorDark == player.ColorDark);

            foreach (var c in PlayerColors)
                c.IsSelected = false;

            if (match is null)
            {
                match = new GameColor
                {
                    Name = "Personalizado",
                    ColorLight = player.ColorLight,
                    ColorDark = player.ColorDark,
                    IsSelected = true
                };
                PlayerColors.Insert(0, match);
            }
            else
            {
                match.IsSelected = true;
            }

            SelectedColor = match;
        }

        protected void UpdatePlayerFromForm(Player player)
        {
            player.Name = Name.Trim();
            player.ColorLight = SelectedColor?.ColorLight ?? "#C62828";
            player.ColorDark = SelectedColor?.ColorDark ?? "#EF9A9A";
        }
    }
}
