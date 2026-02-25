using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels
{
    public partial class CreateGamePageModel : GameFormPageModelBase
    {
        [ObservableProperty]
        private ImageSource _selectedImage;

        private string _temporaryImagePath;

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

            // Guardar en una ubicación temporal
            var tempPath = Path.Combine(FileSystem.CacheDirectory, $"temp_{Guid.NewGuid()}{Path.GetExtension(result.FileName)}");

            using var sourceStream = await result.OpenReadAsync();
            using var fileStream = File.Create(tempPath);
            await sourceStream.CopyToAsync(fileStream);

            _temporaryImagePath = tempPath;
            SelectedImage = ImageSource.FromFile(_temporaryImagePath);
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

                // Si hay imagen seleccionada, moverla a la ubicación permanente
                if (!string.IsNullOrEmpty(_temporaryImagePath) && File.Exists(_temporaryImagePath))
                {
                    var imagesDirectory = Path.Combine(FileSystem.AppDataDirectory, "GameImages");
                    Directory.CreateDirectory(imagesDirectory);

                    var fileName = $"game_{Guid.NewGuid()}{Path.GetExtension(_temporaryImagePath)}";
                    var permanentPath = Path.Combine(imagesDirectory, fileName);

                    File.Move(_temporaryImagePath, permanentPath);
                    game.ImagePath = permanentPath;
                }

                await _gameRepository.SaveItemAsync(game);

                await Shell.Current.GoToAsync("..");
                await AppShell.DisplayToastAsync("Juego creado exitosamente");
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);

                // Limpiar archivo temporal en caso de error
                if (!string.IsNullOrEmpty(_temporaryImagePath) && File.Exists(_temporaryImagePath))
                {
                    File.Delete(_temporaryImagePath);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}