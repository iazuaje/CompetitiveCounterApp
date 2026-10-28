using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompetitiveCounterApp.Helpers;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels
{
    public partial class EditPlayerPageModel : PlayerFormPageModelBase, IQueryAttributable
    {
        private Player? _player;

        /// <summary>Texto del botón de la barra según el estado del jugador.</summary>
        [ObservableProperty]
        private string _archiveText = "Archivar";

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
                ArchiveText = _player.IsArchived ? "Desarchivar" : "Archivar";
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
        private async Task ToggleArchive()
        {
            if (_player is null)
                return;

            HapticFeedbackHelper.Click();
            var archive = !_player.IsArchived;

            if (archive && !await Shell.Current.DisplayAlertAsync(
                    "Archivar jugador",
                    $"«{_player.Name}» se ocultará de la lista de jugadores y no se podrá agregar a sesiones nuevas. Sus estadísticas se conservan.",
                    "Archivar",
                    "Cancelar"))
                return;

            try
            {
                IsBusy = true;
                await SetArchivedAsync(archive);
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

        private async Task SetArchivedAsync(bool archive)
        {
            await _playerRepository.SetArchivedAsync(_player!.ID, archive);
            _player.IsArchived = archive;
            await Shell.Current.GoToAsync("..");
            await AppShell.DisplayToastAsync(archive ? "Jugador archivado" : "Jugador restaurado");
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (_player is null)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            HapticFeedbackHelper.Click();

            bool confirm = await Shell.Current.DisplayAlertAsync(
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
                    // Eliminarlo borraría historial ajeno: se ofrece archivarlo.
                    if (!_player.IsArchived && await Shell.Current.DisplayAlertAsync(
                            "No se puede eliminar",
                            $"«{_player.Name}» participa en sesiones. ¿Quieres archivarlo? Se oculta de la lista y de las sesiones nuevas, y sus estadísticas se conservan.",
                            "Archivar",
                            "Cancelar"))
                        await SetArchivedAsync(true);
                    else if (_player.IsArchived)
                        await Shell.Current.DisplayAlertAsync(
                            "No se puede eliminar",
                            $"«{_player.Name}» participa en sesiones, por eso queda archivado en lugar de eliminarse.",
                            "OK");
                    return;
                }

                await _playerRepository.DeleteItemAsync(_player);
                // Saltea también las estadísticas del jugador eliminado y vuelve a la lista.
                await Shell.Current.GoToAsync("../..");
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
