using System;
using System.Drawing;
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
        public Color DisabledBackground { get; set; }
        public Color Border { get; set; }
        public Color Selection { get; set; }
    }

    internal static class ThemeService
    {
        private const string PersonalizeRegistryPath = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string AppsUseLightThemeValue = "AppsUseLightTheme";
        private const int DwmUseImmersiveDarkMode = 20;
        private const int DwmUseImmersiveDarkModeBefore20H1 = 19;

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
                    DisabledBackground = Color.FromArgb(38, 38, 38),
                    Border = Color.FromArgb(82, 82, 82),
                    Selection = Color.FromArgb(62, 62, 62)
                };
            }
            return new ThemePalette
            {
                EffectiveMode = effective,
                IsDark = false,
                WindowBackground = SystemColors.Control,
                SurfaceBackground = SystemColors.Control,
                InputBackground = SystemColors.Window,
                Text = SystemColors.ControlText,
                MutedText = SystemColors.GrayText,
                DisabledBackground = SystemColors.Control,
                Border = SystemColors.ControlDark,
                Selection = SystemColors.Highlight
            };
        }

        public static void ApplyToMenu(ToolStrip menu, AppThemeMode mode)
        {
            if (menu == null) return;
            var palette = GetPalette(mode);
            menu.Renderer = new AppToolStripRenderer(palette);
            menu.BackColor = palette.SurfaceBackground;
            menu.ForeColor = palette.Text;
            ApplyToolStripItems(menu.Items, palette);
            menu.Invalidate(true);
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
                item.ForeColor = item.Enabled ? palette.Text : palette.MutedText;
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

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr windowHandle, int attribute, ref int value, int valueSize);

        private sealed class AppThemeColorTable : ProfessionalColorTable
        {
            private readonly ThemePalette palette;

            public AppThemeColorTable(ThemePalette palette)
            {
                this.palette = palette;
                UseSystemColors = false;
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
            public override Color SeparatorDark { get { return palette.Border; } }
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
                RoundedEdges = false;
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs args)
            {
                args.TextColor = args.Item.Enabled ? palette.Text : palette.MutedText;
                base.OnRenderItemText(args);
            }

            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs args)
            {
                args.ArrowColor = args.Item.Enabled ? palette.Text : palette.MutedText;
                base.OnRenderArrow(args);
            }

            protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs args)
            {
                using (var background = new SolidBrush(palette.Selection))
                {
                    args.Graphics.FillRectangle(background, args.ImageRectangle);
                }
                var rectangle = args.ImageRectangle;
                var points = new[]
                {
                    new Point(rectangle.Left + rectangle.Width / 5, rectangle.Top + rectangle.Height / 2),
                    new Point(rectangle.Left + rectangle.Width * 2 / 5, rectangle.Bottom - rectangle.Height / 4),
                    new Point(rectangle.Right - rectangle.Width / 6, rectangle.Top + rectangle.Height / 4)
                };
                using (var pen = new Pen(palette.Text, 2F))
                {
                    args.Graphics.DrawLines(pen, points);
                }
            }
        }
    }
}
