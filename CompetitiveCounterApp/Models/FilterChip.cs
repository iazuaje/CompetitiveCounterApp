using CommunityToolkit.Mvvm.ComponentModel;

namespace CompetitiveCounterApp.Models;

public enum FilterChipKind
{
    Category,
    ActiveSession,
    Unplayed,

    /// <summary>Muestra solo los archivados (sin elegirlo, quedan ocultos).</summary>
    Archived
}

/// <summary>Chip seleccionable: una categoría o un filtro de estado (sesión activa, sin jugar, archivados).</summary>
public partial class FilterChip : ObservableObject
{
    public FilterChipKind Kind { get; init; }

    /// <summary>Solo para <see cref="FilterChipKind.Category"/>.</summary>
    public Category? Category { get; set; }

    public string Glyph { get; init; } = string.Empty;

    [ObservableProperty]
    private string _label = string.Empty;

    [ObservableProperty]
    private bool _isSelected;

    public static FilterChip ForCategory(Category category, bool isSelected = false) => new()
    {
        Kind = FilterChipKind.Category,
        Category = category,
        Label = category.Name,
        Glyph = FluentUI.tag_24_regular,
        IsSelected = isSelected
    };
}
