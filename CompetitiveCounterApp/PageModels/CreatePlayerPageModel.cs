using CommunityToolkit.Mvvm.Input;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels
{
    public partial class CreatePlayerPageModel : PlayerFormPageModelBase
    {
        public CreatePlayerPageModel(PlayerRepository playerRepository, ModalErrorHandler errorHandler)
            : base(playerRepository, errorHandler)
        {
        }

        [RelayCommand]
        private async Task Save()
        {
            if (!ValidateForm())
            {
                await AppShell.DisplayToastAsync("El nombre del jugador es requerido");
                return;
            }

            try
            {
                IsBusy = true;
                var player = new Player();
                UpdatePlayerFromForm(player);
                await _playerRepository.SaveItemAsync(player);
                await Shell.Current.GoToAsync("..");
                await AppShell.DisplayToastAsync("Jugador creado exitosamente");
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
    }
}
