using System;
using System.Windows.Forms;
using CodexUsageTrayLite.Infrastructure;
using CodexUsageTrayLite.Models;

namespace CodexUsageTrayLite.UI
{
    internal abstract class ThemedForm : Form
    {
        private OwnedApplicationIcon ownedApplicationIcon;
        private bool systemEventsSubscribed;

        protected ThemedForm(AppThemeMode themeMode)
        {
            ThemeMode = ThemeService.Normalize(themeMode);
        }

        internal AppThemeMode ThemeMode { get; private set; }
        internal bool? UsesDarkApplicationIcon { get; private set; }

        internal void ApplyAppTheme(AppThemeMode themeMode)
        {
            ThemeMode = ThemeService.Normalize(themeMode);
            var palette = ThemeService.GetPalette(ThemeMode);
            ThemeService.ApplyToForm(this, ThemeMode);
            ApplyApplicationIcon(palette.IsDark, false);
            OnAppThemeApplied(palette);
        }

        internal void ApplyApplicationIcon(bool dark, bool force)
        {
            if (!force && UsesDarkApplicationIcon == dark && ownedApplicationIcon != null) return;
            OwnedApplicationIcon next;
            try
            {
                next = ApplicationIconService.Create(dark);
            }
            catch (Exception ex)
            {
                SafeLogger.Write("ApplicationIcon.LoadFailed", "variant=" + (dark ? "dark" : "light") + " error=" + ex.GetType().Name);
                return;
            }

            var previous = ownedApplicationIcon;
            ownedApplicationIcon = next;
            UsesDarkApplicationIcon = dark;
            Icon = next.Icon;
            if (previous != null) previous.Dispose();
        }

        protected virtual void OnAppThemeApplied(ThemePalette palette)
        {
        }

        protected override void OnHandleCreated(System.EventArgs args)
        {
            base.OnHandleCreated(args);
            var palette = ThemeService.GetPalette(ThemeMode);
            ApplyApplicationIcon(palette.IsDark, true);
            ThemeService.ApplyTitleBar(Handle, palette.IsDark);
            SubscribeToSystemAppearanceEvents();
        }

        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);
            if (ApplicationIconService.IsSystemAppearanceMessage(message.Msg) && !IsDisposed)
            {
                RefreshSystemThemeIfNeeded();
            }
        }

        private void SubscribeToSystemAppearanceEvents()
        {
            if (systemEventsSubscribed) return;
            Microsoft.Win32.SystemEvents.UserPreferenceChanged += HandleSystemAppearanceChanged;
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += HandleSystemAppearanceChanged;
            systemEventsSubscribed = true;
        }

        private void HandleSystemAppearanceChanged(object sender, System.EventArgs args)
        {
            if (ThemeMode != AppThemeMode.System || IsDisposed || !IsHandleCreated) return;
            try
            {
                BeginInvoke(new Action(RefreshSystemThemeIfNeeded));
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void RefreshSystemThemeIfNeeded()
        {
            if (ThemeMode == AppThemeMode.System && !IsDisposed) ApplyAppTheme(AppThemeMode.System);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (systemEventsSubscribed)
                {
                    Microsoft.Win32.SystemEvents.UserPreferenceChanged -= HandleSystemAppearanceChanged;
                    Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= HandleSystemAppearanceChanged;
                    systemEventsSubscribed = false;
                }
                if (ownedApplicationIcon != null)
                {
                    Icon = null;
                    ownedApplicationIcon.Dispose();
                    ownedApplicationIcon = null;
                    UsesDarkApplicationIcon = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
