using CommunityToolkit.Mvvm.ComponentModel;

namespace CompetitiveCounterApp.PageModels;

/// <summary>Estadísticas generales, sin juego ni jugador concreto.</summary>
public partial class GeneralStatsPageModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Estadísticas";
}
