using CompetitiveCounterApp.Models;
using System.Windows.Input;

namespace CompetitiveCounterApp.Pages.Controls;

public partial class IconPicker : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IEnumerable<IconData>),
            typeof(IconPicker),
            default(IEnumerable<IconData>));

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(
            nameof(SelectedItem),
            typeof(IconData),
            typeof(IconPicker),
            default(IconData),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty ItemCommandProperty =
        BindableProperty.Create(
            nameof(ItemCommand),
            typeof(ICommand),
            typeof(IconPicker),
            default(ICommand));

    public static readonly BindableProperty SelectedColorProperty =
        BindableProperty.Create(
            nameof(SelectedColor),
            typeof(GameColor),
            typeof(IconPicker),
            default(GameColor));

    public IEnumerable<IconData> ItemsSource
    {
        get => (IEnumerable<IconData>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public IconData SelectedItem
    {
        get => (IconData)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public GameColor SelectedColor
    {
        get => (GameColor)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public ICommand ItemCommand
    {
        get => (ICommand)GetValue(ItemCommandProperty);
        set => SetValue(ItemCommandProperty, value);
    }

    public IconPicker()
    {
        InitializeComponent();
    }

    private void OnItemTapped(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is IconData iconData)
        {
            SelectedItem = iconData;
            ItemCommand?.Execute(iconData);
        }
    }
}