using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CompetitiveCounterApp.Helpers;
using CompetitiveCounterApp.Messages;
using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.PageModels
{
    public enum GamesSortMode
    {
        Manual,
        Name,
        RecentlyPlayed,
        MostSessions
    }

    /// <summary>
    /// Lista de juegos con búsqueda, filtros (estado y categorías) y criterio de orden. El orden manual se
    /// cambia arrastrando en la grilla (<see cref="LiveReorderController"/> en la vista → <see cref="MoveGameAsync"/>).
    /// </summary>
    public partial class GamesPageModel : ObservableObject
    {
        private const string SortPreferenceKey = "games.sort";
        private const string ViewPreferenceKey = "games.view";

        private readonly GameRepository _gameRepository;
        private readonly SessionRepository _sessionRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly ModalErrorHandler _errorHandler;

        /// <summary>Todos los juegos (en orden manual); <see cref="Games"/> es la vista filtrada y ordenada.</summary>
        private List<Game> _allGames = [];

        [ObservableProperty]
        private List<Game> _games = [];

        /// <summary>Hay juegos creados (aunque los filtros no muestren ninguno).</summary>
        [ObservableProperty]
        private bool _hasGames;

        /// <summary>Hay juegos pero ninguno coincide con la búsqueda o los filtros.</summary>
        [ObservableProperty]
        private bool _hasNoResults;

        /// <summary>"3 juegos" o, con búsqueda/filtros, "1 de 3 juegos".</summary>
        [ObservableProperty]
        private string _gamesCountText = string.Empty;

        [ObservableProperty]
        private string _searchText = string.Empty;

        partial void OnSearchTextChanged(string value) => ApplyFilters();

        [ObservableProperty]
        private ObservableCollection<FilterChip> _filterChips = [];

        [ObservableProperty]
        private GamesSortMode _sortMode;

        [ObservableProperty]
        private string _sortLabel = string.Empty;

        partial void OnSortModeChanged(GamesSortMode value)
        {
            SortLabel = SortName(value);
            ApplyFilters();
        }

        /// <summary>Lista de una columna en lugar de la grilla de tarjetas (se recuerda entre sesiones).</summary>
        [ObservableProperty]
        private bool _isListView;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isRefreshing;

        public GamesPageModel(
            GameRepository gameRepository,
            SessionRepository sessionRepository,
            CategoryRepository categoryRepository,
            ModalErrorHandler errorHandler)
        {
            _gameRepository = gameRepository;
            _sessionRepository = sessionRepository;
            _categoryRepository = categoryRepository;
            _errorHandler = errorHandler;

            SortMode = (GamesSortMode)Preferences.Default.Get(SortPreferenceKey, (int)GamesSortMode.Manual);
            SortLabel = SortName(SortMode);
            IsListView = Preferences.Default.Get(ViewPreferenceKey, false);

            WeakReferenceMessenger.Default.Register<AppThemeChangedMessage>(this, static (r, _) =>
            {
                foreach (var game in ((GamesPageModel)r)._allGames)
                    game.NotifyThemeChanged();
            });
        }

        [RelayCommand]
        private async Task Appearing()
        {
            await LoadGames();
        }

        [RelayCommand]
        private async Task Refresh()
        {
            try
            {
                IsRefreshing = true;
                await LoadGames();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task LoadGames()
        {
            try
            {
                IsBusy = true;
                var games = await _gameRepository.ListAsync();
                var categories = await _categoryRepository.ListAsync();
                var sessionsByGame = (await _sessionRepository.ListAsync())
                    .GroupBy(s => s.GameID)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var game in games)
                {
                    var sessions = sessionsByGame.GetValueOrDefault(game.ID) ?? [];
                    game.SessionCount = sessions.Count;
                    game.HasActiveSession = sessions.Any(s => s.IsActive);
                    game.HasWins = sessions.Any(s => s.TotalWins > 0);
                    game.LastPlayedDate = sessions.Count == 0 ? null : sessions.Max(s => s.SessionDate);

                    // Mismo ranking que el podio del detalle (sesiones ganadas, luego victorias).
                    var leader = PlayerGameStatsCalculator.RankPlayers(sessions).FirstOrDefault();
                    game.LeaderName = leader?.Player.Name ?? string.Empty;
                    game.LeaderSessionsWon = leader?.SessionsWon ?? 0;
                }

                _allGames = games;
                HasGames = games.Count > 0;
                BuildFilterChips(categories);
                ApplyFilters();
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

        /// <summary>Chips de estado + una por categoría, conservando la selección previa.</summary>
        private void BuildFilterChips(IReadOnlyList<Category> categories)
        {
            var selectedKinds = FilterChips.Where(c => c.IsSelected && c.Kind != FilterChipKind.Category).Select(c => c.Kind).ToHashSet();
            var selectedCategories = FilterChips.Where(c => c.IsSelected && c.Category is not null).Select(c => c.Category!.ID).ToHashSet();

            var chips = new List<FilterChip>
            {
                new() { Kind = FilterChipKind.ActiveSession, Label = "Sesión activa", Glyph = FluentUI.play_circle_24_regular, IsSelected = selectedKinds.Contains(FilterChipKind.ActiveSession) },
                new() { Kind = FilterChipKind.Unplayed, Label = "Sin jugar", Glyph = FluentUI.sparkle_24_regular, IsSelected = selectedKinds.Contains(FilterChipKind.Unplayed) }
            };
            chips.AddRange(categories.Select(c => FilterChip.ForCategory(c, selectedCategories.Contains(c.ID))));

            FilterChips = new ObservableCollection<FilterChip>(chips);
        }

        /// <summary>
        /// Búsqueda (nombre, descripción o categoría, sin distinguir mayúsculas ni tildes) + filtros de estado (todos
        /// los elegidos) + categorías (cualquiera de las elegidas), y luego el criterio de orden.
        /// </summary>
        private void ApplyFilters()
        {
            var query = SearchText?.Trim() ?? string.Empty;
            var onlyActive = FilterChips.Any(c => c.Kind == FilterChipKind.ActiveSession && c.IsSelected);
            var onlyUnplayed = FilterChips.Any(c => c.Kind == FilterChipKind.Unplayed && c.IsSelected);
            var categoryIds = FilterChips
                .Where(c => c.IsSelected && c.Category is not null)
                .Select(c => c.Category!.ID)
                .ToHashSet();

            var filtered = _allGames.Where(g =>
                (query.Length == 0
                    || Contains(g.Name, query)
                    || Contains(g.Description, query)
                    || g.Categories.Any(c => Contains(c.Name, query)))
                && (!onlyActive || g.HasActiveSession)
                && (!onlyUnplayed || !g.HasWins)
                && (categoryIds.Count == 0 || g.Categories.Any(c => categoryIds.Contains(c.ID))));

            Games = (SortMode switch
            {
                GamesSortMode.Name => filtered.OrderBy(g => g.Name, StringComparer.CurrentCultureIgnoreCase),
                GamesSortMode.RecentlyPlayed => filtered
                    .OrderByDescending(g => g.LastPlayedDate ?? DateTime.MinValue)
                    .ThenBy(g => g.Name, StringComparer.CurrentCultureIgnoreCase),
                GamesSortMode.MostSessions => filtered
                    .OrderByDescending(g => g.SessionCount)
                    .ThenBy(g => g.Name, StringComparer.CurrentCultureIgnoreCase),
                _ => filtered // _allGames ya viene en orden manual.
            }).ToList();

            HasNoResults = HasGames && Games.Count == 0;
            var total = _allGames.Count == 1 ? "1 juego" : $"{_allGames.Count} juegos";
            GamesCountText = Games.Count == _allGames.Count ? total : $"{Games.Count} de {total}";
        }

        static bool Contains(string? source, string value) =>
            !string.IsNullOrEmpty(source)
            && CultureInfo.CurrentCulture.CompareInfo.IndexOf(
                source, value, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;

        static string SortName(GamesSortMode mode) => mode switch
        {
            GamesSortMode.Name => "Nombre",
            GamesSortMode.RecentlyPlayed => "Jugado recientemente",
            GamesSortMode.MostSessions => "Más sesiones",
            _ => "Manual"
        };

        [RelayCommand]
        private void ToggleFilter(FilterChip chip)
        {
            HapticFeedbackHelper.Click();
            chip.IsSelected = !chip.IsSelected;
            ApplyFilters();
        }

        [RelayCommand]
        private void ClearFilters()
        {
            HapticFeedbackHelper.Click();
            foreach (var chip in FilterChips)
                chip.IsSelected = false;
            SearchText = string.Empty;
            ApplyFilters();
        }

        /// <summary>Mantener presionada una categoría del filtro: renombrar o eliminar.</summary>
        [RelayCommand]
        private async Task FilterOptions(FilterChip chip)
        {
            if (chip.Category is null)
                return;

            try
            {
                if (await CategoryPrompts.ShowOptionsAsync(chip.Category, _categoryRepository) != CategoryChange.None)
                    await LoadGames();
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
        }

        [RelayCommand]
        private void ToggleView()
        {
            HapticFeedbackHelper.Click();
            IsListView = !IsListView;
            Preferences.Default.Set(ViewPreferenceKey, IsListView);
        }

        [RelayCommand]
        private async Task ChangeSort()
        {
            HapticFeedbackHelper.Click();

            var modes = Enum.GetValues<GamesSortMode>();
            var options = modes
                .Select(m => m == SortMode ? $"✓ {SortName(m)}" : SortName(m))
                .ToArray();

            var choice = await Shell.Current.DisplayActionSheetAsync("Ordenar por", "Cancelar", null, options);
            var index = Array.IndexOf(options, choice);
            if (index < 0)
                return;

            SortMode = modes[index];
            Preferences.Default.Set(SortPreferenceKey, (int)SortMode);
        }

        /// <summary>Lo asigna la vista mientras un arrastre recién terminado debe ignorar el tap.</summary>
        public Func<bool>? IsTapSuppressed { get; set; }

        /// <summary>Arrastrar solo tiene sentido viendo todos los juegos en orden manual.</summary>
        public bool CanReorder =>
            SortMode == GamesSortMode.Manual
            && string.IsNullOrWhiteSpace(SearchText)
            && !FilterChips.Any(c => c.IsSelected)
            && Games.Count > 1;

        /// <summary>Mantener presionado con otro orden o con filtros: ofrecer volver al orden manual completo.</summary>
        public async Task OfferManualOrderAsync()
        {
            if (_allGames.Count < 2)
                return;

            var change = await Shell.Current.DisplayAlertAsync(
                "Reordenar juegos",
                "Para mover los juegos, la lista tiene que estar en orden Manual y sin búsqueda ni filtros.",
                "Cambiar",
                "Cancelar");
            if (!change)
                return;

            foreach (var chip in FilterChips)
                chip.IsSelected = false;
            SearchText = string.Empty;
            SortMode = GamesSortMode.Manual;
            Preferences.Default.Set(SortPreferenceKey, (int)SortMode);
            ApplyFilters();
        }

        /// <summary>Soltar una tarjeta arrastrada: aplica el nuevo orden ya visible y lo guarda.</summary>
        public async Task MoveGameAsync(int from, int to)
        {
            if (from < 0 || from >= Games.Count || to < 0 || to >= Games.Count)
                return;

            var games = Games.ToList();
            var game = games[from];
            games.RemoveAt(from);
            games.Insert(to, game);
            _allGames = games;
            Games = games.ToList();

            try
            {
                await _gameRepository.SaveOrderAsync(games.Select(g => g.ID).ToList());
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
                await LoadGames();
            }
        }

        [RelayCommand]
        private async Task AddGame()
        {
            HapticFeedbackHelper.Click();
            await Shell.Current.GoToAsync("creategame");
        }

        [RelayCommand]
        private async Task NavigateToGame(Game game)
        {
            // El dedo que suelta una tarjeta arrastrada puede llegar como tap.
            if (IsTapSuppressed?.Invoke() == true)
                return;

            HapticFeedbackHelper.Click();
            await Shell.Current.GoToAsync($"gamedetail?id={game.ID}");
        }
    }
}
