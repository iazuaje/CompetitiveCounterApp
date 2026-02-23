using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels
{
    public partial class CreateGamePageModel : GameFormPageModelBase
    {
        [ObservableProperty]
        private ImageSource _selectedImage;

        public CreateGamePageModel(GameRepository gameRepository, ModalErrorHandler errorHandler)
            : base(gameRepository, errorHandler)
        {
        }

        [RelayCommand]
        private async Task SelectImage()
        {
            if (!MediaPicker.Default.IsCaptureSupported)
                return;

            var result = await MediaPicker.Default.PickPhotoAsync();

            if (result == null)
                return;

            using var stream = await result.OpenReadAsync();
            SelectedImage = ImageSource.FromStream(() => stream);
        }

        [RelayCommand]
        private async Task Save()
        {
            if (!ValidateForm())
            {
                await ShowValidationErrorAsync("El nombre del juego es requerido");
                return;
            }

            try
            {
                IsBusy = true;

                var game = new Game
                {
                    CreatedDate = DateTime.Now
                };

                UpdateGameFromForm(game);

                await _gameRepository.SaveItemAsync(game);

                await Shell.Current.GoToAsync("..");
                await AppShell.DisplayToastAsync("Juego creado exitosamente");
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
