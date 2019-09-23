using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace Karpach.DebugAttachManager
{
    public class Colors
    {
        public static ThemeResourceKey ToobarBackgroundBegin=> EnvironmentColors.CommandBarGradientBeginColorKey;
        public static ThemeResourceKey ToobarBackgroundEnd => EnvironmentColors.CommandBarGradientEndColorKey;
        public static ThemeResourceKey MainBackground => EnvironmentColors.ToolWindowBackgroundBrushKey;
        public static ThemeResourceKey MainForeground => EnvironmentColors.ToolWindowTextBrushKey;
        public static ThemeResourceKey SearchBackground => EnvironmentColors.SearchBoxBackgroundBrushKey;
        public static ThemeResourceKey SearchBorder => EnvironmentColors.SearchBoxBorderBrushKey;
        public static ThemeResourceKey SearchPlaceHolderForeground => EnvironmentColors.ControlEditHintTextBrushKey;
        public static ThemeResourceKey LinkForeground => EnvironmentColors.ControlLinkTextBrushKey;
        public static ThemeResourceKey LinkHoverForeground => EnvironmentColors.MainWindowActiveIconDebuggingBrushKey;
        public static ThemeResourceKey ToolbarHoverBackground => EnvironmentColors.CommandBarHoverBrushKey;
        public static ThemeResourceKey ToolbarPressedBackground => EnvironmentColors.SearchBoxPressedBackgroundBrushKey;
        public static ThemeResourceKey ToolbarHoverBorder => EnvironmentColors.CommandBarHoverOverSelectedIconBorderBrushKey;
        public static ThemeResourceKey SortTextColor => EnvironmentColors.SortTextColorKey;
    }
}