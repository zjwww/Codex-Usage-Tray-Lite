using System;
using System.Threading;
using System.Windows.Forms;
using CodexUsageTrayLite.Infrastructure;
using CodexUsageTrayLite.Models;
using CodexUsageTrayLite.UI;

namespace CodexUsageTrayLite
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var ui = UiText.For(SettingsStore.LoadLanguageForStartup());
            bool created;
            using (var mutex = new Mutex(true, @"Local\CodexUsageTrayLite", out created))
            {
                if (!created)
                {
                    LocalizedMessageBox.Show(null, ui.AlreadyRunning, AppConstants.Name, MessageBoxButtons.OK, MessageBoxIcon.Information, AppThemeMode.System, ui.Language);
                    return;
                }
                AppPaths.EnsureBaseDirectories();
                SafeLogger.Write("Startup", "version=" + AppConstants.Version + " baseline=" + AppConstants.BaselineTag);
                try
                {
                    using (var context = new TrayApplicationContext())
                    {
                        Application.Run(context);
                    }
                }
                catch (Exception ex)
                {
                    SafeLogger.Write("Fatal", ex.GetType().Name + ": " + ex.Message);
                    LocalizedMessageBox.Show(null, ui.FatalStart(ex.Message, AppPaths.LogPath), AppConstants.Name,
                        MessageBoxButtons.OK, MessageBoxIcon.Error, AppThemeMode.System, ui.Language);
                }
            }
        }
    }
}
