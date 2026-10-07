using CommunityToolkit.Mvvm.ComponentModel;

namespace CompetitiveCounterApp.PageModels;

public partial class GameStatsPageModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    private string _title = "Estadísticas";

    private int _gameId;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var value))
            _gameId = Convert.ToInt32(value);
    }
}
