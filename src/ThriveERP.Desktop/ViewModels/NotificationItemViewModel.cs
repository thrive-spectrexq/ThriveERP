using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ThriveERP.Desktop.ViewModels;

public partial class NotificationItemViewModel : ObservableObject
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Title { get; }
    public string Message { get; }
    public string TimeAgo { get; }
    public string Icon { get; } // "⚠️", "📦", "💳", "📄"
    public string BadgeColor { get; } // Hex color string
    public Type? NavigationTargetType { get; }

    [ObservableProperty]
    private bool _isRead;

    public NotificationItemViewModel(string title, string message, string timeAgo, string icon, string badgeColor = "#3B82F6", Type? navigationTargetType = null, bool isRead = false)
    {
        Title = title;
        Message = message;
        TimeAgo = timeAgo;
        Icon = icon;
        BadgeColor = badgeColor;
        NavigationTargetType = navigationTargetType;
        IsRead = isRead;
    }
}
