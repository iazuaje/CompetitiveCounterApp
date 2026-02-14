using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace CompetitiveCounterApp.Models
{
    public partial class IconData : ObservableObject
    {
        [ObservableProperty]
        private string? _icon;

        [ObservableProperty]
        private string? _description;

        [ObservableProperty]
        private bool _isSelected;
    }
}
