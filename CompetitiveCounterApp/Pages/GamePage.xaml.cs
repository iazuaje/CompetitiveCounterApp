using System.ComponentModel;
using CompetitiveCounterApp.Helpers;

namespace CompetitiveCounterApp.Pages;

public partial class GamesPage : ContentPage
{
	readonly GamesPageModel _model;
	readonly LiveReorderController _reorder;

	public GamesPage(GamesPageModel gamesPageModel)
	{
		InitializeComponent();
		BindingContext = _model = gamesPageModel;

		// Reordenar arrastrando en la grilla o la lista: la vista anima, el page model aplica y guarda el orden.
		_reorder = new LiveReorderController(
			() => gamesPageModel.IsListView ? GamesList : GamesGrid,
			GamesScroll,
			canReorder: () => gamesPageModel.CanReorder,
			onBlocked: () => gamesPageModel.OfferManualOrderAsync().FireAndForgetSafeAsync(),
			onMoved: (from, to) => gamesPageModel.MoveGameAsync(from, to).FireAndForgetSafeAsync());
		gamesPageModel.IsTapSuppressed = () => _reorder.SuppressTap;

		gamesPageModel.PropertyChanged += OnModelPropertyChanged;
		UpdateViewToggle();
	}

	void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(GamesPageModel.IsListView))
			UpdateViewToggle();
	}

	/// <summary>
	/// El botón muestra la vista a la que se cambia. Se arma acá (y no por binding) para crearlo ya con
	/// el color de la barra: AppShell solo vuelve a teñir los íconos al aparecer la página o cambiar el tema.
	/// </summary>
	void UpdateViewToggle()
	{
		ViewToggleItem.Text = _model.IsListView ? "Ver como grilla" : "Ver como lista";
		ViewToggleItem.IconImageSource = new FontImageSource
		{
			Glyph = _model.IsListView ? FluentUI.grid_24_regular : FluentUI.apps_list_24_regular,
			FontFamily = FluentUI.FontFamily,
			Size = Application.Current?.Resources["IconSortShell"] is FontImageSource sort ? sort.Size : 20,
			Color = Shell.GetForegroundColor(this) ?? Colors.White
		};
	}
}
