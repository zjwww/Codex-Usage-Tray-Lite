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
        private static int Main(string[] args)
        {
            var request = CommandLineRequest.Parse(args);
            if (request.Verb != CommandLineVerb.Run)
            {
                CommandLineInterface.PrepareConsole();
                try
                {
                    return ExecuteCommandLine(request);
                }
                finally
                {
                    CommandLineInterface.CompleteConsoleOutput();
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var ui = UiText.For(SettingsStore.LoadLanguageForStartup());
            bool created;
            using (var mutex = new Mutex(true, @"Local\CodexUsageTrayLite", out created))
            {
                if (!created)
                {
                    LocalizedMessageBox.Show(null, ui.AlreadyRunning, AppConstants.Name, MessageBoxButtons.OK, MessageBoxIcon.Information, AppThemeMode.System, ui.Language);
                    return 0;
                }
                AppPaths.EnsureBaseDirectories();
                SafeLogger.Write("Startup", "version=" + AppConstants.Version + " baseline=" + AppConstants.BaselineTag);
                try
                {
                    using (var context = new TrayApplicationContext())
                    using (var commandChannel = new SingleInstanceCommandChannel(context.PostSingleInstanceCommand))
                    {
                        Application.Run(context);
                    }
                    return 0;
                }
                catch (Exception ex)
                {
                    SafeLogger.Write("Fatal", ex.GetType().Name + ": " + ex.Message);
                    LocalizedMessageBox.Show(null, ui.FatalStart(ex.Message, AppPaths.LogPath), AppConstants.Name,
                        MessageBoxButtons.OK, MessageBoxIcon.Error, AppThemeMode.System, ui.Language);
                    return 1;
                }
            }
        }

        private static int ExecuteCommandLine(CommandLineRequest request)
        {
            switch (request.Verb)
            {
                case CommandLineVerb.Help:
                    Console.WriteLine(CommandLineInterface.HelpText);
                    return 0;
                case CommandLineVerb.Version:
                    Console.WriteLine(AppConstants.Name + " " + AppConstants.Version);
                    return 0;
                case CommandLineVerb.Status:
                    Console.WriteLine(CommandLineInterface.FormatSavedStatus(SettingsStore.Load()));
                    return 0;
                case CommandLineVerb.LogPath:
                    Console.WriteLine(AppPaths.LogPath);
                    return 0;
                case CommandLineVerb.ConfigPath:
                    Console.WriteLine(AppPaths.DataDirectory);
                    return 0;
                case CommandLineVerb.Refresh:
                    return SendCommand(SingleInstanceCommand.Refresh, "Refresh request sent to the running tray instance.");
                case CommandLineVerb.Exit:
                    return SendCommand(SingleInstanceCommand.Exit, "Exit request sent to the running tray instance.");
                default:
                    Console.Error.WriteLine("Error: " + (request.Error ?? "Invalid command-line option."));
                    Console.Error.WriteLine();
                    Console.Error.WriteLine(CommandLineInterface.HelpText);
                    return 64;
            }
        }

        private static int SendCommand(SingleInstanceCommand command, string successMessage)
        {
            if (SingleInstanceCommandChannel.TrySend(command, TimeSpan.FromSeconds(2)))
            {
                Console.WriteLine(successMessage);
                return 0;
            }
            Console.Error.WriteLine("Error: no responsive Codex Usage Tray Lite tray instance was found in this Windows session.");
            return 2;
        }
    }
}
