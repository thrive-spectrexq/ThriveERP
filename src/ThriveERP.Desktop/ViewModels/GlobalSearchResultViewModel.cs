using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ThriveERP.Desktop.ViewModels;

public partial class GlobalSearchResultViewModel : ObservableObject
{
    public string Title { get; }
    public string Subtitle { get; }
    public string Category { get; } // "Product", "Customer", "Sales Order", "Purchase Order", "Employee"
    public string Icon { get; }
    public Type ViewModelType { get; }
    public object? TargetItem { get; }

    public GlobalSearchResultViewModel(string title, string subtitle, string category, string icon, Type viewModelType, object? targetItem = null)
    {
        Title = title;
        Subtitle = subtitle;
        Category = category;
        Icon = icon;
        ViewModelType = viewModelType;
        TargetItem = targetItem;
    }
}
