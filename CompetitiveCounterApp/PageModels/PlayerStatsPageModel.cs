using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CompetitiveCounterApp.Helpers;
using CompetitiveCounterApp.Messages;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels;

/// <summary>Estadísticas de un jugador en todos los juegos.</summary>
public partial class PlayerStatsPageModel : ObservableObject, IQueryAttributable
{
    private readonly PlayerRepository _playerRepository;
    private readonly ModalErrorHandler _errorHandler;
    private int _playerId;

    [ObservableProperty]
    private Player? _player;

    [ObservableProperty]
    private string _title = "Estadísticas";

    public PlayerStatsPageModel(PlayerRepository playerRepository, ModalErrorHandler errorHandler)
    {
        _playerRepository = playerRepository;
        _errorHandler = errorHandler;

        WeakReferenceMessenger.Default.Register<AppThemeChangedMessage>(this, static (r, _) =>
            ((PlayerStatsPageModel)r).Player?.NotifyThemeChanged());
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var value))
        {
            _playerId = Convert.ToInt32(value);
            LoadData().FireAndForgetSafeAsync(_errorHandler);
        }
        else
        {
            Shell.Current.GoToAsync("..").FireAndForgetSafeAsync(_errorHandler);
        }
    }

    // Recarga al volver de editar (nombre y color pueden haber cambiado).
    [RelayCommand]
    private async Task Appearing()
    {
        if (_playerId > 0)
            await LoadData();
    }

    private async Task LoadData()
    {
        try
        {
            Player = await _playerRepository.GetAsync(_playerId);

            if (Player is null)
            {
                _errorHandler.HandleError(new Exception("No se encontró el jugador."));
                await Shell.Current.GoToAsync("..");
                return;
            }

            Title = Player.Name;
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }
    }

    [RelayCommand]
    private async Task Edit()
    {
        if (_playerId <= 0)
            return;

        HapticFeedbackHelper.Click();
        await Shell.Current.GoToAsync($"editplayer?id={_playerId}");
    }
}
