using CompetitiveCounterApp.Models;
using System.Windows.Input;

namespace CompetitiveCounterApp.Pages.Controls;

public partial class ColorPicker : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(
            nameof(ItemsSource),
            typeof(List<GameColor>),
            typeof(ColorPicker),
            default(List<GameColor>));

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(
            nameof(SelectedItem),
            typeof(GameColor),
            typeof(ColorPicker),
            default(GameColor),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty ItemCommandProperty =
        BindableProperty.Create(
            nameof(ItemCommand),
            typeof(ICommand),
            typeof(ColorPicker),
            default(ICommand));

    public List<GameColor> ItemsSource
    {
        get => (List<GameColor>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    public GameColor SelectedItem
    {
        get => (GameColor)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public ICommand ItemCommand
    {
        get => (ICommand)GetValue(ItemCommandProperty);
        set => SetValue(ItemCommandProperty, value);
    }

    public ColorPicker()
    {
        InitializeComponent();
    }

    private void OnItemTapped(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is GameColor color)
        {
            SelectedItem = color;
            ItemCommand?.Execute(color);
        }
    }
}