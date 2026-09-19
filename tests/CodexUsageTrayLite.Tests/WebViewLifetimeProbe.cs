using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodexUsageTrayLite.Services;
using Microsoft.Web.WebView2.Core;

namespace CodexUsageTrayLite.Tests
{
    internal static class WebViewLifetimeProbe
    {
        public static int Run(int count)
        {
            return Run(count, true);
        }

        public static int RunTelemetry()
        {
            return Run(1, false);
        }

        private static int Run(int count, bool requireHandleGate)
        {
            if (count < 1 || count > 100) throw new ArgumentOutOfRangeException("count");
            var result = 2;
            using (var runner = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                Location = new Point(-32000, -32000),
                Size = new Size(1, 1),
                Opacity = 0
            })
            {
                runner.Shown += async (sender, args) =>
                {
                    try { result = await ExecuteAsync(count, requireHandleGate); }
                    catch (Exception ex) { Console.Error.WriteLine(ex); result = 2; }
                    finally { runner.Close(); }
                };
                Application.Run(runner);
            }
            return result;
        }

        public static int RunForms(int count)
        {
            var result = 2;
            using (var runner = new Form { ShowInTaskbar = false, Opacity = 0, Size = new Size(1, 1) })
            {
                runner.Shown += async (sender, args) =>
                {
                    var process = Process.GetCurrentProcess();
                    process.Refresh();
                    var before = process.HandleCount;
                    for (var index = 0; index < count; index++)
                    {
                        using (var form = new Form { ShowInTaskbar = false, Opacity = 0, Size = new Size(2, 2) })
                        {
                            form.Show();
                            form.Close();
                        }
                        await Task.Delay(25);
                    }
                    process.Refresh();
                    Console.WriteLine("Plain Form handles: " + before + "->" + process.HandleCount + " loops=" + count);
                    result = 0;
                    runner.Close();
                };
                Application.Run(runner);
            }
            return result;
        }

        private static async Task<int> ExecuteAsync(int count, bool requireHandleGate)
        {
            var runtime = CoreWebView2Environment.GetAvailableBrowserVersionString();
            if (string.IsNullOrWhiteSpace(runtime))
            {
                Console.WriteLine("WebView2 runtime not installed; probe skipped.");
                return 3;
            }
            var profile = Path.Combine(Path.GetTempPath(), "CodexUsageTrayLite-WebViewProbe-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(profile);
            var process = Process.GetCurrentProcess();
            process.Refresh();
            var handlesStart = process.HandleCount;
            var workingStart = process.WorkingSet64;
            var peakOwned = 0;
            Console.WriteLine("WebView2 probe runtime=" + runtime + " loops=" + count);
            try
            {
                var sharedEnvironment = await CoreWebView2Environment.CreateAsync(null, profile, new CoreWebView2EnvironmentOptions("--no-first-run"));
                WebViewProcessTelemetry.TrackEnvironment(sharedEnvironment);
                var browserExited = new TaskCompletionSource<bool>();
                EventHandler<CoreWebView2BrowserProcessExitedEventArgs> exitedHandler = (sender, args) => browserExited.TrySetResult(true);
                for (var index = 0; index < count; index++)
                {
                    var viewport = WebViewUsageResetDomProbe.BackgroundViewportSize;
                    using (var host = new BackgroundWebViewHostForm(viewport))
                    {
                        var hostHandle = host.CreateHostHandle();
                        var controllerTask = sharedEnvironment.CreateCoreWebView2ControllerAsync(hostHandle);
                        var createWinner = await Task.WhenAny(controllerTask, Task.Delay(10000));
                        if (createWinner != controllerTask)
                        {
                            Console.WriteLine("Controller creation timed out at loop " + (index + 1));
                            return 1;
                        }
                        var controller = await controllerTask;
                        controller.Bounds = host.ClientRectangle;
                        controller.IsVisible = false;
                        var core = controller.CoreWebView2;
                        var browserProcessId = WebViewProcessTelemetry.ControllerCreated("probe", core);
                        if (index == 0) peakOwned = sharedEnvironment.GetProcessInfos().Count;
                        var completed = new TaskCompletionSource<bool>();
                        EventHandler<CoreWebView2NavigationCompletedEventArgs> handler = (sender, args) => completed.TrySetResult(args.IsSuccess);
                        core.NavigationCompleted += handler;
                        core.NavigateToString("<!doctype html><title>Lifecycle probe</title><p>CodexUsageTrayLite</p>");
                        var winner = await Task.WhenAny(completed.Task, Task.Delay(10000));
                        core.NavigationCompleted -= handler;
                        if (winner != completed.Task)
                        {
                            Console.WriteLine("Probe navigation timed out at loop " + (index + 1));
                            return 1;
                        }
                        var expectedViewport = "\"" + viewport.Width + "x" + viewport.Height + "\"";
                        var actualViewport = await core.ExecuteScriptAsync("window.innerWidth + 'x' + window.innerHeight");
                        if (!string.Equals(expectedViewport, actualViewport, StringComparison.Ordinal))
                        {
                            Console.WriteLine("Hidden viewport mismatch at loop " + (index + 1));
                            return 1;
                        }
                        core.Stop();
                        if (index == count - 1) sharedEnvironment.BrowserProcessExited += exitedHandler;
                        controller.Close();
                        WebViewProcessTelemetry.ControllerCloseReturned("probe", browserProcessId);
                        core = null;
                        controller = null;
                        host.Close();
                    }
                    await Task.Delay(250);
                    if ((index + 1) % 10 == 0) Console.WriteLine("Completed controller loops: " + (index + 1));
                }

                var exitWinner = await Task.WhenAny(browserExited.Task, Task.Delay(15000));
                sharedEnvironment.BrowserProcessExited -= exitedHandler;
                var browserDidExit = exitWinner == browserExited.Task;
                process.Refresh();
                var handlesAfterWait = process.HandleCount;
                var workingAfterWait = process.WorkingSet64;
                Console.WriteLine("WebView2 process infos: peak=" + peakOwned + " browserExited=" + browserDidExit);
                Console.WriteLine("Host handles after runtime wait: " + handlesStart + "->" + handlesAfterWait);
                Console.WriteLine("Host working set after runtime wait: " + workingStart + "->" + workingAfterWait);
                Console.WriteLine("Hidden desktop viewport matched: true");
                Console.WriteLine("No GC.Collect and no process kill were used.");
                sharedEnvironment = null;
                return browserDidExit && (!requireHandleGate || handlesAfterWait <= handlesStart + 20) ? 0 : 1;
            }
            finally
            {
                DeleteDirectoryWithRetry(profile);
            }
        }

        private static void DeleteDirectoryWithRetry(string path)
        {
            for (var attempt = 0; attempt < 10; attempt++)
            {
                try
                {
                    if (Directory.Exists(path)) Directory.Delete(path, true);
                    return;
                }
                catch
                {
                    if (attempt == 9) return;
                    System.Threading.Thread.Sleep(250);
                }
            }
        }

    }
}
