using MudBlazor;

namespace Qwertide.Client.Theme;

/// <summary>
/// Dark terminal-mono MudTheme. We never ship MudBlazor in its default Material
/// state: the palette is mapped to the Qwertide tokens (see qwertide.css) and
/// the type is forced to JetBrains Mono / Space Grotesk so any Mud primitive we
/// do use (snackbar, form field) reads as part of the same language.
/// </summary>
public static class QwertideTheme
{
    public static readonly MudTheme Instance = new()
    {
        PaletteDark = new PaletteDark
        {
            Black = "#0e0e10",
            Background = "#0e0e10",
            BackgroundGray = "#161619",
            Surface = "#161619",
            DrawerBackground = "#0e0e10",
            AppbarBackground = "#0e0e10",
            AppbarText = "#f4f4f5",

            TextPrimary = "#d4d4d8",
            TextSecondary = "#6b6b73",
            TextDisabled = "#44444c",

            Primary = "#e2b340",        // single locked accent
            PrimaryContrastText = "#14110a",
            Secondary = "#d4d4d8",
            Tertiary = "#e2b340",

            Error = "#e5484d",
            ErrorContrastText = "#14110a",
            Success = "#e2b340",        // we don't introduce a second hue
            Info = "#6b6b73",
            Warning = "#e2b340",

            ActionDefault = "#d4d4d8",
            ActionDisabled = "#44444c",
            Divider = "#2a2a31",
            DividerLight = "#1f1f25",
            Dark = "#0e0e10",
            LinesDefault = "#2a2a31",
            LinesInputs = "#2a2a31",
            TableLines = "#1f1f25",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = new[] { "Space Grotesk", "system-ui", "sans-serif" } },
            Body1 = new Body1Typography { FontFamily = new[] { "Space Grotesk", "system-ui", "sans-serif" } },
            Button = new ButtonTypography { FontFamily = new[] { "Space Grotesk", "system-ui", "sans-serif" }, TextTransform = "none" },
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "4px",
        },
    };
}
