using System;
using Avalonia.Media;
using Avalonia.Styling;

namespace ThriveERP.Desktop.Services;

public enum AppTheme
{
    DimmedGray,
    CoolSlate,
    WarmNeutral,
    SoftDark
}

public static class ThemeManager
{
    public static AppTheme CurrentTheme { get; private set; } = AppTheme.CoolSlate;

    public static void ApplyTheme(AppTheme theme)
    {
        CurrentTheme = theme;
        if (Avalonia.Application.Current is null) return;

        var res = Avalonia.Application.Current.Resources;

        switch (theme)
        {
            case AppTheme.DimmedGray:
                Avalonia.Application.Current.RequestedThemeVariant = ThemeVariant.Light;
                SetBrushes(res,
                    windowBg: "#E2E8F0",     // Dimmed slate gray
                    headerBg: "#EDF2F7",     // Soft matte slate header
                    cardBg: "#F1F5F9",       // Soft dimmed card surface (no pure white glare!)
                    cardBorder: "#CBD5E1",   // Well-defined slate border
                    textPrimary: "#0F172A",  // Deep slate text
                    textSecondary: "#475569",// Muted slate
                    hoverBg: "#CBD5E1",      // Slate hover
                    accent: "#2563EB");      // Royal blue
                break;

            case AppTheme.CoolSlate:
                Avalonia.Application.Current.RequestedThemeVariant = ThemeVariant.Light;
                SetBrushes(res,
                    windowBg: "#CBD5E1",
                    headerBg: "#E2E8F0",
                    cardBg: "#EDF2F7",
                    cardBorder: "#94A3B8",
                    textPrimary: "#0F172A",
                    textSecondary: "#334155",
                    hoverBg: "#94A3B8",
                    accent: "#1D4ED8");
                break;

            case AppTheme.WarmNeutral:
                Avalonia.Application.Current.RequestedThemeVariant = ThemeVariant.Light;
                SetBrushes(res,
                    windowBg: "#E7E5E4",
                    headerBg: "#F5F5F4",
                    cardBg: "#FAFAF9",
                    cardBorder: "#D6D3D1",
                    textPrimary: "#1C1917",
                    textSecondary: "#44403C",
                    hoverBg: "#D6D3D1",
                    accent: "#D97706");
                break;

            case AppTheme.SoftDark:
                Avalonia.Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
                SetBrushes(res,
                    windowBg: "#0F172A",
                    headerBg: "#1E293B",
                    cardBg: "#1E293B",
                    cardBorder: "#334155",
                    textPrimary: "#F8FAFC",
                    textSecondary: "#94A3B8",
                    hoverBg: "#334155",
                    accent: "#38BDF8");
                break;
        }
    }

    private static void SetBrushes(
        Avalonia.Controls.IResourceDictionary res,
        string windowBg, string headerBg, string cardBg, string cardBorder,
        string textPrimary, string textSecondary, string hoverBg, string accent)
    {
        res["AppWindowBackground"] = Brush.Parse(windowBg);
        res["AppHeaderBackground"] = Brush.Parse(headerBg);
        res["AppSidebarBackground"] = Brush.Parse(headerBg);
        res["AppCardBackground"] = Brush.Parse(cardBg);
        res["AppSurfaceBackground"] = Brush.Parse(cardBg);
        res["AppBorderBrush"] = Brush.Parse(cardBorder);
        res["AppTextPrimary"] = Brush.Parse(textPrimary);
        res["AppTextSecondary"] = Brush.Parse(textSecondary);
        res["AppHoverBackground"] = Brush.Parse(hoverBg);
        res["AppAccentPrimary"] = Brush.Parse(accent);

        // Dynamic System Overrides
        res["SystemRegionBrush"] = Brush.Parse(windowBg);
        res["SystemControlPageBackgroundChromeLowBrush"] = Brush.Parse(windowBg);
        res["SystemControlForegroundBaseHighBrush"] = Brush.Parse(textPrimary);
        res["SystemControlForegroundBaseMediumBrush"] = Brush.Parse(textSecondary);
        res["SystemControlForegroundBaseLowBrush"] = Brush.Parse(cardBorder);

        res["CardBackground"] = Brush.Parse(cardBg);
        res["SurfaceBackground"] = Brush.Parse(windowBg);
        res["TextPrimary"] = Brush.Parse(textPrimary);
        res["TextSecondary"] = Brush.Parse(textSecondary);
    }
}
