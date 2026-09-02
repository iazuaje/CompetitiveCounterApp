using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
                string.Equals(c.ColorLight, player.ColorHex, StringComparison.OrdinalIgnoreCase)
                || string.Equals(c.ColorDark, player.ColorHex, StringComparison.OrdinalIgnoreCase));

            foreach (var c in PlayerColors)
                c.IsSelected = false;

            SelectedColor = match ?? PlayerColors[0];
            SelectedColor.IsSelected = true;
        }

        protected void UpdatePlayerFromForm(Player player)
        {
            player.Name = Name.Trim();
            player.ColorHex = SelectedColor.ColorLight;
        }
    }
}
