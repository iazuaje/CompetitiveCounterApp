using CompetitiveCounterApp.Models;

namespace CompetitiveCounterApp.Helpers;

public enum CategoryChange
{
    None,
    Renamed,
    Deleted
}

/// <summary>Diálogos compartidos para crear, renombrar y eliminar categorías (formularios y filtro de Juegos).</summary>
public static class CategoryPrompts
{
    /// <summary>Pide un nombre y crea la categoría (o devuelve la existente con ese nombre). Null si se cancela.</summary>
    public static async Task<Category?> CreateAsync(CategoryRepository repository)
    {
        var name = await Shell.Current.DisplayPromptAsync(
            "Nueva categoría",
            "Nombre de la categoría",
            "Crear",
            "Cancelar",
            placeholder: "Ej.: Pelea, Mesa, Cartas",
            maxLength: 30,
            keyboard: Keyboard.Text);

        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await repository.GetOrCreateAsync(name);
    }

    /// <summary>Mantener presionado un chip de categoría: renombrar o eliminar (con confirmación).</summary>
    public static async Task<CategoryChange> ShowOptionsAsync(Category category, CategoryRepository repository)
    {
        const string rename = "Renombrar";
        const string delete = "Eliminar categoría";

        var choice = await Shell.Current.DisplayActionSheetAsync(category.Name, "Cancelar", delete, rename);

        if (choice == rename)
        {
            var name = await Shell.Current.DisplayPromptAsync(
                "Renombrar categoría",
                "Nuevo nombre",
                "Guardar",
                "Cancelar",
                maxLength: 30,
                keyboard: Keyboard.Text,
                initialValue: category.Name);

            if (string.IsNullOrWhiteSpace(name) || name.Trim() == category.Name)
                return CategoryChange.None;

            await repository.RenameAsync(category.ID, name);
            category.Name = name.Trim();
            return CategoryChange.Renamed;
        }

        if (choice == delete)
        {
            bool confirm = await Shell.Current.DisplayAlertAsync(
                "Eliminar categoría",
                $"¿Eliminar \"{category.Name}\"? Los juegos no se borran, solo pierden esta categoría.",
                "Eliminar",
                "Cancelar");

            if (!confirm)
                return CategoryChange.None;

            await repository.DeleteAsync(category.ID);
            return CategoryChange.Deleted;
        }

        return CategoryChange.None;
    }
}
