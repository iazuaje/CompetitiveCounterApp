using CommunityToolkit.Mvvm.Input;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels
{
    public partial class EditPlayerPageModel : PlayerFormPageModelBase, IQueryAttributable
    {
        private Player? _player;

        public EditPlayerPageModel(PlayerRepository playerRepository, ModalErrorHandler errorHandler)
            : base(playerRepository, errorHandler)
        {
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
                _player = await _playerRepository.GetAsync(id);

                if (_player is null || _player.ID == 0)
                {
                    _errorHandler.HandleError(new Exception("No se encontró el jugador."));
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                ApplyPlayerData(_player);
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
            if (_player is null)
            {
                _errorHandler.HandleError(new Exception("No se pudo identificar el jugador."));
                return;
            }

            if (!ValidateForm())
            {
                await AppShell.DisplayToastAsync("El nombre del jugador es requerido");
                return;
            }

            try
            {
                IsBusy = true;
                UpdatePlayerFromForm(_player);
                await _playerRepository.SaveItemAsync(_player);
                await Shell.Current.GoToAsync("..");
                await AppShell.DisplayToastAsync("Jugador actualizado exitosamente");
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
            if (_player is null)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert(
                "Eliminar jugador",
                $"¿Estás seguro de eliminar a '{_player.Name}'?",
                "Sí",
                "No");

            if (!confirm)
                return;

            try
            {
                IsBusy = true;

                if (await _playerRepository.HasParticipationsAsync(_player.ID))
                {
                    await Shell.Current.DisplayAlert(
                        "No se puede eliminar",
                        "El jugador participa en sesiones. Quitá esas participaciones o dejalo en el catálogo.",
                        "OK");
                    return;
                }

                await _playerRepository.DeleteItemAsync(_player);
                await Shell.Current.GoToAsync("..");
                await AppShell.DisplayToastAsync("Jugador eliminado exitosamente");
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
