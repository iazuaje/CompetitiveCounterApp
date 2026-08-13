using CommunityToolkit.Mvvm.Input;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels
{
    public partial class CreateGamePageModel : GameFormPageModelBase
    {
        public CreateGamePageModel(GameRepository gameRepository, ModalErrorHandler errorHandler)
            : base(gameRepository, errorHandler)
        {
        }

        [RelayCommand]
        private async Task Save()
        {
            if (!ValidateForm())
            {
                await ShowValidationErrorAsync("El nombre del juego es requerido");
                return;
            }

            string? imagePath = null;
            bool gameSaved = false;

            try
            {
                IsBusy = true;

                var game = new Game
                {
                    CreatedDate = DateTime.Now
                };

                UpdateGameFromForm(game);

                imagePath = MoveTemporaryImageToPermanent();
                if (!string.IsNullOrEmpty(imagePath))
                    game.ImagePath = imagePath;

                await _gameRepository.SaveItemAsync(game);
                gameSaved = true;

                await Shell.Current.GoToAsync("..");
                await AppShell.DisplayToastAsync("Juego creado exitosamente");
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);

                if (!gameSaved && !string.IsNullOrEmpty(imagePath))
                {
                    if (File.Exists(imagePath))
                        File.Delete(imagePath);
                    SelectedImage = null;
                }

                if (!string.IsNullOrEmpty(_temporaryImagePath) && File.Exists(_temporaryImagePath))
                    File.Delete(_temporaryImagePath);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}