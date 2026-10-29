using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompetitiveCounterApp.Helpers;

namespace CompetitiveCounterApp.PageModels;

/// <summary>Respaldo, restauración y borrado de todos los datos (sección "Respaldo" del flyout).</summary>
public partial class BackupPageModel : ObservableObject
{
    const string LastBackupKey = "backup.last";

    private readonly BackupService _backupService;
    private readonly GameRepository _gameRepository;
    private readonly PlayerRepository _playerRepository;
    private readonly SessionRepository _sessionRepository;
    private readonly ModalErrorHandler _errorHandler;

    /// <summary>"5 juegos · 3 jugadores · 12 sesiones".</summary>
    [ObservableProperty]
    private string _summaryText = string.Empty;

    [ObservableProperty]
    private string _lastBackupText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsIdle))]
    private bool _isBusy;

    public bool IsIdle => !IsBusy;

    public BackupPageModel(
        BackupService backupService,
        GameRepository gameRepository,
        PlayerRepository playerRepository,
        SessionRepository sessionRepository,
        ModalErrorHandler errorHandler)
    {
        _backupService = backupService;
        _gameRepository = gameRepository;
        _playerRepository = playerRepository;
        _sessionRepository = sessionRepository;
        _errorHandler = errorHandler;
    }

    [RelayCommand]
    private async Task Appearing()
    {
        try
        {
            await LoadSummary();
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }
    }

    private async Task LoadSummary()
    {
        var games = (await _gameRepository.ListAsync()).Count;
        var players = (await _playerRepository.ListAsync()).Count;
        var sessions = (await _sessionRepository.ListAsync()).Count;
        SummaryText = $"{Count(games, "juego", "juegos")} · {Count(players, "jugador", "jugadores")} · {Count(sessions, "sesión", "sesiones")}";

        var last = Preferences.Default.Get(LastBackupKey, DateTime.MinValue);
        LastBackupText = last == DateTime.MinValue
            ? "Todavía no creaste ningún respaldo."
            : $"Último respaldo: {last:dd/MM/yyyy HH:mm}";
    }

    static string Count(int value, string singular, string plural) => $"{value} {(value == 1 ? singular : plural)}";

    [RelayCommand]
    private async Task CreateBackup()
    {
        HapticFeedbackHelper.Click();

        const string save = "Guardar en el teléfono";
        const string share = "Compartir (Drive, WhatsApp…)";
        var choice = await Shell.Current.DisplayActionSheetAsync("Crear respaldo", "Cancelar", null, save, share);
        if (choice is not (save or share))
            return;

        try
        {
            IsBusy = true;
            var path = await _backupService.CreateAsync();
            var saved = false;

            if (choice == save)
            {
                await using var stream = File.OpenRead(path);
                var result = await FileSaver.Default.SaveAsync(Path.GetFileName(path), stream, CancellationToken.None);
                // Cancelar el selector de Android también llega como fallo: solo se informa si hubo un error real.
                saved = result.IsSuccessful;
                if (saved)
                    await AppShell.DisplayToastAsync("Respaldo guardado");
            }
            else
            {
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Respaldo de CompetitiveCounter",
                    File = new ShareFile(path, "application/zip")
                });
                saved = true;
            }

            if (saved)
            {
                Preferences.Default.Set(LastBackupKey, DateTime.Now);
                await LoadSummary();
            }
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
    private async Task RestoreBackup()
    {
        HapticFeedbackHelper.Click();

        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Elegir respaldo",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    [DevicePlatform.Android] = ["application/zip", "application/x-zip-compressed", "application/octet-stream"],
                    [DevicePlatform.iOS] = ["public.zip-archive"]
                })
            });
            if (file is null)
                return;

            var confirm = await Shell.Current.DisplayAlertAsync(
                "Restaurar respaldo",
                $"Se reemplazarán los datos actuales ({SummaryText}) por los de «{file.FileName}». No se puede deshacer: si quieres conservarlos, crea un respaldo antes.",
                "Restaurar",
                "Cancelar");
            if (!confirm)
                return;

            IsBusy = true;
            await using (var stream = await file.OpenReadAsync())
                await _backupService.RestoreAsync(stream);

            await LoadSummary();
            await AppShell.DisplayToastAsync("Respaldo restaurado");
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
    private async Task DeleteAllData()
    {
        HapticFeedbackHelper.Click();

        try
        {
            var confirm = await Shell.Current.DisplayAlertAsync(
                "Borrar todos los datos",
                $"Se borrarán todos los juegos, jugadores, sesiones e imágenes ({SummaryText}). No se puede deshacer: si quieres conservarlos, crea un respaldo antes.",
                "Borrar todo",
                "Cancelar");
            if (!confirm)
                return;

            IsBusy = true;
            await _backupService.DeleteAllAsync();

            await LoadSummary();
            await AppShell.DisplayToastAsync("Datos borrados");
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
