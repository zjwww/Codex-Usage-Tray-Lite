using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CodexUsageTrayLite.Infrastructure;
using CodexUsageTrayLite.Models;
using Microsoft.Win32;
using Microsoft.Web.WebView2.Core;

namespace CodexUsageTrayLite.UI
{
    internal sealed class ThemePalette
    {
        public AppThemeMode EffectiveMode { get; set; }
        public bool IsDark { get; set; }
        public Color WindowBackground { get; set; }
        public Color SurfaceBackground { get; set; }
        public Color InputBackground { get; set; }
        public Color Text { get; set; }
        public Color MutedText { get; set; }
        public Color SummaryText { get; set; }
        public Color DisabledText { get; set; }
        public Color DisabledBackground { get; set; }
        public Color Border { get; set; }
        public Color Separator { get; set; }
        public Color Selection { get; set; }
        public bool HighContrast { get; set; }
    }

    internal static class ThemeService
    {
        private const string PersonalizeRegistryPath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string AppsUseLightThemeValue = "AppsUseLightTheme";
        private const int DwmUseImmersiveDarkMode = 20;
        private const int DwmUseImmersiveDarkModeBefore20H1 = 19;

        internal static TextFormatFlags MenuLabelTextFormatFlags
        {
            get
            {
                // Keep native TextRenderer glyph padding so custom status/summary
                // labels share the same left edge as ordinary ToolStripMenuItem text.
                return TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine |
                    TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
            }
        }

        public static AppThemeMode Normalize(AppThemeMode mode)
        {
            return Enum.IsDefined(typeof(AppThemeMode), mode) ? mode : AppThemeMode.System;
        }

        public static AppThemeMode ResolveEffectiveMode(AppThemeMode mode)
        {
            return ResolveEffectiveMode(mode, IsSystemDarkMode());
        }

        internal static AppThemeMode ResolveEffectiveMode(AppThemeMode mode, bool systemDark)
        {
            mode = Normalize(mode);
            return mode == AppThemeMode.System
                ? (systemDark ? AppThemeMode.Dark : AppThemeMode.Light)
                : mode;
        }

        public static ThemePalette GetPalette(AppThemeMode mode)
        {
            var effective = ResolveEffectiveMode(mode);
            if (SystemInformation.HighContrast)
            {
                return new ThemePalette
                {
                    EffectiveMode = effective,
                    IsDark = effective == AppThemeMode.Dark,
                    HighContrast = true,
                    WindowBackground = SystemColors.Window,
                    SurfaceBackground = SystemColors.Menu,
                    InputBackground = SystemColors.Window,
                    Text = SystemColors.MenuText,
                    MutedText = SystemColors.GrayText,
                    SummaryText = SystemColors.MenuText,
                    DisabledText = SystemColors.GrayText,
                    DisabledBackground = SystemColors.Control,
                    Border = SystemColors.WindowFrame,
                    Separator = SystemColors.WindowFrame,
                    Selection = SystemColors.Highlight
                };
            }
            if (effective == AppThemeMode.Dark)
            {
                return new ThemePalette
                {
                    EffectiveMode = effective,
                    IsDark = true,
                    WindowBackground = Color.FromArgb(32, 32, 32),
                    SurfaceBackground = Color.FromArgb(43, 43, 43),
                    InputBackground = Color.FromArgb(50, 50, 50),
                    Text = Color.FromArgb(243, 243, 243),
                    MutedText = Color.FromArgb(166, 166, 166),
                    SummaryText = Color.FromArgb(192, 195, 202),
                    DisabledText = Color.FromArgb(119, 119, 119),
                    DisabledBackground = Color.FromArgb(38, 38, 38),
                    Border = Color.FromArgb(78, 78, 78),
                    Separator = Color.FromArgb(73, 73, 73),
                    Selection = Color.FromArgb(59, 59, 59)
                };
            }
            return new ThemePalette
            {
                EffectiveMode = effective,
                IsDark = false,
                WindowBackground = SystemColors.Control,
                SurfaceBackground = Color.FromArgb(250, 250, 250),
                InputBackground = SystemColors.Window,
                Text = Color.FromArgb(22, 25, 29),
                MutedText = SystemColors.GrayText,
                SummaryText = Color.FromArgb(83, 89, 99),
                DisabledText = Color.FromArgb(142, 142, 142),
                DisabledBackground = SystemColors.Control,
                Border = Color.FromArgb(207, 211, 217),
                Separator = Color.FromArgb(216, 220, 225),
                Selection = Color.FromArgb(232, 237, 243)
            };
        }

        public static void ApplyToMenu(ToolStrip menu, AppThemeMode mode)
        {
            if (menu == null) return;
            var palette = GetPalette(mode);
            menu.Renderer = new AppToolStripRenderer(palette);
            menu.BackColor = palette.SurfaceBackground;
            menu.ForeColor = palette.Text;
            ApplyMenuLayout(menu);
            ApplyToolStripItems(menu.Items, palette);
            menu.Invalidate(true);
        }

        internal static void ApplyMenuLayout(ToolStrip menu)
        {
            if (menu == null) return;
            var scale = menu.DeviceDpi / 96F;
            Func<int, int> scaled = value => Math.Max(1, (int)Math.Round(value * scale));
            menu.Padding = new Padding(scaled(4));
            foreach (ToolStripItem item in menu.Items)
            {
                var separator = item as ToolStripSeparator;
                if (separator != null)
                {
                    separator.Margin = new Padding(scaled(2), 0, scaled(2), 0);
                    separator.Padding = new Padding(0, scaled(3), 0, scaled(3));
                    continue;
                }
                item.Margin = Padding.Empty;
                item.Padding = new Padding(0, scaled(2), 0, scaled(2));
                var dropDownItem = item as ToolStripDropDownItem;
                if (dropDownItem != null)
                {
                    dropDownItem.DropDown.Font = menu.Font;
                    ApplyMenuLayout(dropDownItem.DropDown);
                }
            }
        }

        public static void ApplyToForm(Form form, AppThemeMode mode)
        {
            if (form == null) return;
            var palette = GetPalette(mode);
            form.BackColor = palette.WindowBackground;
            form.ForeColor = palette.Text;
            ApplyControls(form.Controls, palette);
            if (form.IsHandleCreated) ApplyTitleBar(form.Handle, palette.IsDark);
            form.Invalidate(true);
        }

        public static CoreWebView2PreferredColorScheme ToWebViewColorScheme(AppThemeMode mode)
        {
            switch (Normalize(mode))
            {
                case AppThemeMode.Dark:
                    return CoreWebView2PreferredColorScheme.Dark;
                case AppThemeMode.Light:
                    return CoreWebView2PreferredColorScheme.Light;
                default:
                    return CoreWebView2PreferredColorScheme.Auto;
            }
        }

        public static void ApplyToWebView(CoreWebView2 core, AppThemeMode mode)
        {
            if (core == null) return;
            try
            {
                core.Profile.PreferredColorScheme = ToWebViewColorScheme(mode);
            }
            catch (Exception ex)
            {
                SafeLogger.Write("Theme.WebViewApplyFailed", ex.GetType().Name);
            }
        }

        public static bool IsSystemDarkMode()
        {
            try
            {
                var value = Registry.GetValue(PersonalizeRegistryPath, AppsUseLightThemeValue, 1);
                return Convert.ToInt32(value) == 0;
            }
            catch
            {
                return false;
            }
        }

        public static void ApplyTitleBar(IntPtr windowHandle, bool dark)
        {
            if (windowHandle == IntPtr.Zero) return;
            try
            {
                var enabled = dark ? 1 : 0;
                var result = DwmSetWindowAttribute(windowHandle, DwmUseImmersiveDarkMode, ref enabled, sizeof(int));
                if (result != 0)
                {
                    DwmSetWindowAttribute(windowHandle, DwmUseImmersiveDarkModeBefore20H1, ref enabled, sizeof(int));
                }
            }
            catch (DllNotFoundException)
            {
            }
            catch (EntryPointNotFoundException)
            {
            }
        }

        private static void ApplyControls(Control.ControlCollection controls, ThemePalette palette)
        {
            foreach (Control control in controls)
            {
                var textBox = control as TextBoxBase;
                var comboBox = control as ComboBox;
                var button = control as Button;
                var toolStrip = control as ToolStrip;

                control.ForeColor = control.Enabled ? palette.Text : palette.MutedText;
                if (textBox != null)
                {
                    textBox.BackColor = control.Enabled ? palette.InputBackground : palette.DisabledBackground;
                }
                else if (comboBox != null)
                {
                    comboBox.BackColor = control.Enabled ? palette.InputBackground : palette.DisabledBackground;
                    comboBox.FlatStyle = palette.IsDark ? FlatStyle.Flat : FlatStyle.Standard;
                }
                else if (button != null)
                {
                    if (palette.IsDark)
                    {
                        button.UseVisualStyleBackColor = false;
                        button.FlatStyle = FlatStyle.Flat;
                        button.FlatAppearance.BorderColor = palette.Border;
                        button.BackColor = palette.SurfaceBackground;
                    }
                    else
                    {
                        button.UseVisualStyleBackColor = true;
                        button.FlatStyle = FlatStyle.Standard;
                        button.BackColor = SystemColors.Control;
                    }
                }
                else if (toolStrip != null)
                {
                    ApplyToMenu(toolStrip, palette.EffectiveMode);
                }
                else if (!(control is Label))
                {
                    control.BackColor = palette.WindowBackground;
                }

                if (control.HasChildren) ApplyControls(control.Controls, palette);
            }
        }

        private static void ApplyToolStripItems(ToolStripItemCollection items, ThemePalette palette)
        {
            foreach (ToolStripItem item in items)
            {
                item.BackColor = palette.SurfaceBackground;
                item.ForeColor = MenuItemTextColor(item, palette);
                var dropDownItem = item as ToolStripDropDownItem;
                if (dropDownItem != null)
                {
                    dropDownItem.DropDown.BackColor = palette.SurfaceBackground;
                    dropDownItem.DropDown.ForeColor = palette.Text;
                    dropDownItem.DropDown.Renderer = new AppToolStripRenderer(palette);
                    ApplyToolStripItems(dropDownItem.DropDownItems, palette);
                }
            }
        }

        internal static Color MenuItemTextColor(ToolStripItem item, ThemePalette palette)
        {
            var trayItem = item as TrayMenuItem;
            if (trayItem != null)
            {
                if (trayItem.TextRole == TrayMenuTextRole.PrimaryStatus) return palette.Text;
                if (trayItem.TextRole == TrayMenuTextRole.SecondaryStatus) return palette.SummaryText;
            }
            return item.Enabled ? palette.Text : palette.DisabledText;
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr windowHandle, int attribute, ref int value, int valueSize);

        private sealed class AppThemeColorTable : ProfessionalColorTable
        {
            private readonly ThemePalette palette;

            public AppThemeColorTable(ThemePalette palette)
            {
                this.palette = palette;
                UseSystemColors = palette.HighContrast;
            }

            public override Color ToolStripDropDownBackground { get { return palette.SurfaceBackground; } }
            public override Color ImageMarginGradientBegin { get { return palette.SurfaceBackground; } }
            public override Color ImageMarginGradientMiddle { get { return palette.SurfaceBackground; } }
            public override Color ImageMarginGradientEnd { get { return palette.SurfaceBackground; } }
            public override Color MenuBorder { get { return palette.Border; } }
            public override Color MenuItemBorder { get { return palette.Selection; } }
            public override Color MenuItemSelected { get { return palette.Selection; } }
            public override Color MenuItemSelectedGradientBegin { get { return palette.Selection; } }
            public override Color MenuItemSelectedGradientEnd { get { return palette.Selection; } }
            public override Color MenuItemPressedGradientBegin { get { return palette.Selection; } }
            public override Color MenuItemPressedGradientMiddle { get { return palette.Selection; } }
            public override Color MenuItemPressedGradientEnd { get { return palette.Selection; } }
            public override Color SeparatorDark { get { return palette.Separator; } }
            public override Color SeparatorLight { get { return palette.SurfaceBackground; } }
            public override Color CheckBackground { get { return palette.Selection; } }
            public override Color CheckSelectedBackground { get { return palette.Selection; } }
            public override Color CheckPressedBackground { get { return palette.Selection; } }
        }

        private sealed class AppToolStripRenderer : ToolStripProfessionalRenderer
        {
            private readonly ThemePalette palette;

            public AppToolStripRenderer(ThemePalette palette)
                : base(new AppThemeColorTable(palette))
            {
                this.palette = palette;
                RoundedEdges = true;
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs args)
            {
                args.TextColor = palette.HighContrast && args.Item.Selected
                    ? SystemColors.HighlightText
                    : MenuItemTextColor(args.Item, palette);
                var trayItem = args.Item as TrayMenuItem;
                if (trayItem == null)
                {
                    base.OnRenderItemText(args);
                    return;
                }

                var hasSummary = !string.IsNullOrEmpty(trayItem.SummaryText) && trayItem.Owner != null;
                if (hasSummary && string.Equals(args.Text, trayItem.ShortcutKeyDisplayString, StringComparison.Ordinal))
                {
                    return;
                }
                if (trayItem.TextRole == TrayMenuTextRole.Default && !hasSummary)
                {
                    base.OnRenderItemText(args);
                    return;
                }

                var labelRectangle = args.TextRectangle;
                if (hasSummary)
                {
                    using (var summaryFont = TrayMenuItem.CreateSummaryFont(trayItem.Font))
                    {
                        var summarySize = TextRenderer.MeasureText(
                            trayItem.SummaryText,
                            summaryFont,
                            Size.Empty,
                            TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                        var arrowReserve = trayItem.HasDropDownItems ? trayItem.ScaleLogical(26) : trayItem.ScaleLogical(8);
                        var gap = trayItem.ScaleLogical(16);
                        var availableWidth = Math.Max(0, trayItem.Owner.ClientRectangle.Right - arrowReserve - args.TextRectangle.Left);
                        var summaryWidth = Math.Min(summarySize.Width, Math.Max(0, availableWidth * 46 / 100));
                        var summaryRectangle = new Rectangle(
                            trayItem.Owner.ClientRectangle.Right - arrowReserve - summaryWidth,
                            args.TextRectangle.Top,
                            summaryWidth,
                            args.TextRectangle.Height);
                        labelRectangle.Width = Math.Max(0, summaryRectangle.Left - gap - labelRectangle.Left);
                        var summaryColor = palette.HighContrast && trayItem.Selected
                            ? SystemColors.HighlightText
                            : trayItem.TextRole == TrayMenuTextRole.PrimaryStatus ? args.TextColor : palette.SummaryText;
                        TextRenderer.DrawText(
                            args.Graphics,
                            trayItem.SummaryText,
                            summaryFont,
                            summaryRectangle,
                            summaryColor,
                            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine |
                            TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    }
                }

                if (labelRectangle.Width > 0)
                {
                    TextRenderer.DrawText(
                        args.Graphics,
                        trayItem.Text,
                        trayItem.Font,
                        labelRectangle,
                        args.TextColor,
                        ThemeService.MenuLabelTextFormatFlags);
                }
            }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs args)
            {
                if (!args.Item.Selected)
                {
                    return;
                }
                var rectangle = new Rectangle(2, 1, Math.Max(0, args.Item.Width - 4), Math.Max(0, args.Item.Height - 2));
                if (rectangle.Width <= 0 || rectangle.Height <= 0) return;
                using (var brush = new SolidBrush(palette.HighContrast ? SystemColors.Highlight : palette.Selection))
                using (var path = CreateRoundedRectangle(rectangle, Math.Max(2, args.Item.Owner.DeviceDpi * 4 / 96)))
                {
                    args.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    args.Graphics.FillPath(brush, path);
                    args.Graphics.SmoothingMode = SmoothingMode.Default;
                }
            }

            protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs args)
            {
                using (var pen = new Pen(palette.Separator))
                {
                    if (args.Vertical)
                    {
                        var x = args.Item.Width / 2;
                        args.Graphics.DrawLine(pen, x, 4, x, Math.Max(4, args.Item.Height - 4));
                    }
                    else
                    {
                        var inset = Math.Max(8, args.Item.Owner.DeviceDpi * 10 / 96);
                        var y = args.Item.Height / 2;
                        args.Graphics.DrawLine(pen, inset, y, Math.Max(inset, args.Item.Width - inset), y);
                    }
                }
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs args)
            {
                var dropDown = args.ToolStrip as ToolStripDropDown;
                if (dropDown == null)
                {
                    base.OnRenderToolStripBorder(args);
                    return;
                }
                var rectangle = new Rectangle(0, 0, Math.Max(0, dropDown.Width - 1), Math.Max(0, dropDown.Height - 1));
                if (rectangle.Width <= 0 || rectangle.Height <= 0) return;
                using (var pen = new Pen(palette.Border))
                using (var path = CreateRoundedRectangle(rectangle, Math.Max(3, dropDown.DeviceDpi * 6 / 96)))
                {
                    args.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    args.Graphics.DrawPath(pen, path);
                    args.Graphics.SmoothingMode = SmoothingMode.Default;
                }
            }

            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs args)
            {
                args.ArrowColor = palette.HighContrast && args.Item.Selected
                    ? SystemColors.HighlightText
                    : args.Item.Enabled ? palette.Text : palette.DisabledText;
                base.OnRenderArrow(args);
            }

            protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs args)
            {
                var rectangle = args.ImageRectangle;
                var points = new[]
                {
                    new Point(rectangle.Left + rectangle.Width / 5, rectangle.Top + rectangle.Height / 2),
                    new Point(rectangle.Left + rectangle.Width * 2 / 5, rectangle.Bottom - rectangle.Height / 4),
                    new Point(rectangle.Right - rectangle.Width / 6, rectangle.Top + rectangle.Height / 4)
                };
                var checkColor = palette.HighContrast && args.Item.Selected ? SystemColors.HighlightText : palette.Text;
                using (var pen = new Pen(checkColor, Math.Max(1.5F, args.Item.Owner.DeviceDpi * 1.8F / 96F)))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    pen.LineJoin = LineJoin.Round;
                    args.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    args.Graphics.DrawLines(pen, points);
                    args.Graphics.SmoothingMode = SmoothingMode.Default;
                }
            }

            private static GraphicsPath CreateRoundedRectangle(Rectangle rectangle, int radius)
            {
                var path = new GraphicsPath();
                radius = Math.Max(1, Math.Min(radius, Math.Min(rectangle.Width, rectangle.Height) / 2));
                var diameter = radius * 2;
                path.AddArc(rectangle.Left, rectangle.Top, diameter, diameter, 180, 90);
                path.AddArc(rectangle.Right - diameter, rectangle.Top, diameter, diameter, 270, 90);
                path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(rectangle.Left, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();
                return path;
            }
        }
    }
}
