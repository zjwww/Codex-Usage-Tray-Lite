using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using CodexUsageTrayLite.Models;
using CodexUsageTrayLite.Infrastructure;
using CodexUsageTrayLite.Services;
using Microsoft.Web.WebView2.Core;

namespace CodexUsageTrayLite.Tests
{
    // Opt-in, offline integration test. Uses a private disposable browser profile
    // and the production script, never the user's profile or network account.
    internal static class WebViewOverviewProbe
    {
        private static string scriptFixtureDirectory;
        private static int fixtureSequence;
        public static int RunLive(int count = 1)
        {
            if (count < 1 || count > 20) throw new ArgumentOutOfRangeException(nameof(count));
            if (!File.Exists(AppPaths.SettingsPath) || !Directory.Exists(AppPaths.WebViewProfileDirectory))
            {
                Console.WriteLine("SKIP Existing settings/profile unavailable in this Windows context.");
                return 3;
            }
            var result = 2;
            using (var context = new ApplicationContext())
            {
                EventHandler idle = null;
                idle = async (sender, args) =>
                {
                    Application.Idle -= idle;
                    try
                    {
                        // Initialize a UI synchronization context without showing a window.
                        using (var host = new BackgroundWebViewHostForm(new Size(1, 1)))
                        {
                            host.CreateHostHandle();
                            using (var service = new UsageFetchService())
                            {
                                var successes = 0;
                                for (var index = 0; index < count; index++)
                                {
                                var outcome = await service.FetchAsync(UsageSource.WebView2,
                                    SettingsStore.Load().GetProxySettings(), CancellationToken.None);
                                Console.WriteLine("Sample " + (index + 1) + "/" + count + " Status: " + outcome.Status + " durationMs=" + (long)outcome.Duration.TotalMilliseconds);
                                if (outcome.IsSuccess)
                                {
                                    var snapshot = outcome.Snapshot;
                                    Console.WriteLine("5-Hour: " + (snapshot.IsFiveHourLimitApplicable ? snapshot.fiveHourRemainingPercent.ToString() : "N/A"));
                                    Console.WriteLine("Weekly: " + snapshot.weeklyRemainingPercent);
                                    Console.WriteLine("Usage resets: " + snapshot.availableUsageResetCount);
                                    Console.WriteLine("Weekly reset available: " + snapshot.weeklyResetAt.HasValue);
                                    successes++;
                                }
                                if (outcome.Status == FetchStatus.LoginRequired) break;
                                if (index + 1 < count) await Task.Delay(1000);
                                }
                                Console.WriteLine("Live samples passed: " + successes + "/" + count);
                                result = successes == count ? 0 : 2;
                            }
                        }
                    }
                    catch (Exception ex) { Console.Error.WriteLine(ex.GetType().Name); }
                    finally { context.ExitThread(); }
                };
                Application.Idle += idle;
                Application.Run(context);
            }
            return result;
        }

        public static int Run()
        {
            var profile = Path.Combine(Path.GetTempPath(), "cutl-overview-probe-" + Guid.NewGuid().ToString("N"));
            var result = 2;
            using (var context = new ApplicationContext())
            using (var host = new BackgroundWebViewHostForm(new Size(1280, 900)))
            {
                CoreWebView2Controller controller = null;
                EventHandler idle = null;
                idle = async (sender, args) =>
                {
                    Application.Idle -= idle;
                    try
                    {
                        var environment = await CoreWebView2Environment.CreateAsync(null, profile);
                        controller = await environment.CreateCoreWebView2ControllerAsync(host.CreateHostHandle());
                        controller.IsVisible = false;
                        controller.Bounds = new Rectangle(Point.Empty, host.ClientSize);
                        var core = controller.CoreWebView2;
                        scriptFixtureDirectory = Path.Combine(profile, "script-fixtures");
                        Directory.CreateDirectory(scriptFixtureDirectory);
                        core.SetVirtualHostNameToFolderMapping("chatgpt.com", scriptFixtureDirectory, CoreWebView2HostResourceAccessKind.DenyCors);
                        var selected = "<h1>Usage</h1><div role='tablist'><button role='tab' aria-selected='false' onclick=\"this.setAttribute('aria-selected','true');document.getElementById('quota').hidden=false;window.overviewClicks++\">Overview</button><button role='tab'>Analytics</button></div><section id='quota' hidden>Weekly limit<br>Resets in 6d 15h<br>93% left<br>Usage limit resets<br>Available 3</section><button onclick='window.resetClicks++'>Use reset</button><script>window.overviewClicks=0;window.resetClicks=0;</script>";
                        await LoadAsync(core, selected);
                        Require(WebViewUsagePageNavigator.ParseResult(await core.ExecuteScriptAsync(WebViewUsagePageNavigator.Script)) == UsageOverviewState.Selected, "Analytics did not switch to Overview.");
                        Require(WebViewUsagePageNavigator.ParseResult(await core.ExecuteScriptAsync(WebViewUsagePageNavigator.Script)) == UsageOverviewState.Ready, "Selected Overview was clicked again.");
                        Require(await core.ExecuteScriptAsync("window.overviewClicks") == "1", "Overview clicked more than once.");
                        Require(await core.ExecuteScriptAsync("window.resetClicks") == "0", "A reset action was invoked.");
                        var text = new JavaScriptSerializer().Deserialize<string>(await core.ExecuteScriptAsync("document.body.innerText"));
                        UsageSnapshot snapshot;
                        string error;
                        Require(UsageParser.TryParse(text, new DateTime(2026, 10, 20, 10, 0, 0), out snapshot, out error), error);
                        Require(snapshot.weeklyRemainingPercent == 93 && snapshot.availableUsageResetCount == 3, "Rendered Overview fields differ from the fixture.");
                        Console.WriteLine("PASS Overview switch, no repeat click, reset untouched, rendered quota/reset parsing");

                        foreach (var blocked in new[] {
                            "<h1>Usage</h1><button onclick='window.bad++'>Overview</button><div><button>Analytics</button></div>",
                            "<h1>Other page</h1><div><button onclick='window.bad++'>Overview</button><button>Analytics</button></div>",
                            "<h1>Usage</h1><div><button disabled onclick='window.bad++'>Overview</button><button>Analytics</button></div>",
                            "<h1>Usage</h1><div><button hidden onclick='window.bad++'>Overview</button><button>Analytics</button></div>",
                            "<h1>Usage</h1><div><button onclick='window.bad++'>Overview</button><button onclick='window.bad++'>Overview</button><button>Analytics</button></div>"
                        })
                        {
                            await LoadAsync(core, blocked + "<script>window.bad=0</script>");
                            Require(WebViewUsagePageNavigator.ParseResult(await core.ExecuteScriptAsync(WebViewUsagePageNavigator.Script)) == UsageOverviewState.Unavailable, "An unsafe/ambiguous tab was accepted.");
                            Require(await core.ExecuteScriptAsync("window.bad") == "0", "An unrelated control was invoked.");
                        }
                        Console.WriteLine("PASS Ungrouped, unrelated, disabled, hidden, and ambiguous controls ignored");
                        var deep = "<h1>Usage</h1><div role='tablist'><div><div><div><div><div><button role='tab' onclick=\"this.setAttribute('aria-selected','true')\">Overview</button></div></div></div></div></div><div><button role='tab'>Analytics</button></div></div>";
                        await LoadAsync(core, deep);
                        Require(WebViewUsagePageNavigator.ParseResult(await core.ExecuteScriptAsync(WebViewUsagePageNavigator.Script)) == UsageOverviewState.Selected, "Deeply wrapped navigation was not found.");
                        Console.WriteLine("PASS Deeply nested explicit tab group located");

                        var delayed = "<h1>Usage</h1><div role='tablist'><button id='overview' role='tab' aria-selected='false' onclick=\"window.clicks++;if(window.clicks===2){this.setAttribute('aria-selected','true');document.getElementById('analytics').setAttribute('aria-selected','false');setTimeout(()=>document.getElementById('quota').hidden=false,600)}\">Overview</button><button id='analytics' role='tab' aria-selected='true'>Analytics</button></div><section id='quota' hidden>Weekly limit<br>Resets in 6d 15h<br>87% left<br>Usage limit resets<br>Available 3</section><button onclick='window.resetClicks++'>Use reset</button><script>window.clicks=0;window.resetClicks=0;</script>";
                        await LoadAsync(core, delayed);
                        Require(WebViewUsagePageNavigator.ParseResult(await core.ExecuteScriptAsync(WebViewUsagePageNavigator.Script)) == UsageOverviewState.Selected, "First click was not dispatched.");
                        var pending = WebViewUsagePageNavigator.ParseObservation(await core.ExecuteScriptAsync(WebViewUsagePageNavigator.BuildScript(false)));
                        Require(pending.AnalyticsSelected && pending.Reason == "switch-pending", "An ineffective first click was mistaken for confirmed Overview.");
                        Require(!UsageParser.TryParse(pending.Text, DateTime.Now, out snapshot, out error), "Empty Analytics content was accepted.");
                        Require(await core.ExecuteScriptAsync("window.clicks") == "1", "Polling without click permission activated a control.");
                        await core.ExecuteScriptAsync(WebViewUsagePageNavigator.Script);
                        var selectedButLoading = WebViewUsagePageNavigator.ParseObservation(await core.ExecuteScriptAsync(WebViewUsagePageNavigator.BuildScript(false)));
                        Require(selectedButLoading.State == UsageOverviewState.Ready && !UsageParser.TryParse(selectedButLoading.Text, DateTime.Now, out snapshot, out error), "An active tab without quota content was mistaken for data readiness.");
                        var delayedReady = false;
                        for (var poll = 0; poll < 20; poll++)
                        {
                            await Task.Delay(300);
                            var loaded = WebViewUsagePageNavigator.ParseObservation(await core.ExecuteScriptAsync(WebViewUsagePageNavigator.BuildScript(false)));
                            if (UsageParser.TryParse(loaded.Text, DateTime.Now, out snapshot, out error) && snapshot.weeklyRemainingPercent == 87)
                            {
                                delayedReady = true;
                                break;
                            }
                        }
                        Require(delayedReady, "Delayed quota did not become readable.");
                        Require(await core.ExecuteScriptAsync("window.resetClicks") == "0", "Recovery activated a reset.");
                        Console.WriteLine("PASS Ineffective click, pending confirmation, bounded re-click, and delayed quota");
                        await VerifyProductionRecoveryAsync(environment, profile);
                        Require(!host.Visible, "Offline test activated its host window.");
                        Console.WriteLine("PASS Hidden host remains invisible");
                        result = 0;
                    }
                    catch (Exception ex) { Console.Error.WriteLine(ex); }
                    finally { controller?.Close(); context.ExitThread(); }
                };
                Application.Idle += idle;
                Application.Run(context);
            }
            // The Runtime exits asynchronously; leave its private temp profile
            // available for cleanup rather than interfering with process exit.
            return result;
        }

        private static async Task VerifyProductionRecoveryAsync(CoreWebView2Environment environment, string profile)
        {
            var fixture = Path.Combine(profile, "fixture");
            Directory.CreateDirectory(fixture);
            File.WriteAllText(Path.Combine(fixture, "usage.html"),
                "<h1>Usage</h1><script>let n=Number(sessionStorage.getItem('loadCount')||'0')+1;sessionStorage.setItem('loadCount',n);if(n>1)document.write('Weekly limit<br>87% left<br>Resets in 6d 15h<br>Usage limit resets<br>Available 3')</script>");
            var navigations = 0;
            var usageNavigations = new HashSet<ulong>();
            Action<CoreWebView2> prepare = core =>
            {
                core.SetVirtualHostNameToFolderMapping("chatgpt.com", fixture, CoreWebView2HostResourceAccessKind.DenyCors);
                core.NavigationStarting += (sender, args) => { if (args.Uri == "https://chatgpt.com/usage.html") usageNavigations.Add(args.NavigationId); };
                core.NavigationCompleted += (sender, args) => { if (args.IsSuccess && usageNavigations.Remove(args.NavigationId)) navigations++; };
            };
            using (var session = new WebViewFetchSession(new ProxySettings(), environment, "https://chatgpt.com/usage.html", prepare))
            using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(44)))
            {
                var outcome = await session.RunAsync(timeout.Token);
                Require(outcome.IsSuccess && outcome.Snapshot.weeklyRemainingPercent == 87 && outcome.Snapshot.availableUsageResetCount == 3,
                    "The real fetch session did not recover from its first blank page.");
                Require(navigations == 2, "The real session did not enforce exactly one recovery reload.");
            }
            Console.WriteLine("PASS Production fetch: blank page -> one reload -> stable Weekly/reset result");

            File.WriteAllText(Path.Combine(fixture, "cancel.html"), "<h1>Usage</h1><p>Still loading</p>");
            var timer = Stopwatch.StartNew();
            using (var session = new WebViewFetchSession(new ProxySettings(), environment, "https://chatgpt.com/cancel.html", prepare))
            using (var cancellation = new CancellationTokenSource(700))
            {
                var cancelled = false;
                try { await session.RunAsync(cancellation.Token); }
                catch (OperationCanceledException) { cancelled = true; }
                Require(cancelled && timer.Elapsed < TimeSpan.FromSeconds(5), "Cancellation did not promptly complete a pending page wait.");
            }
            Console.WriteLine("PASS Production page-wait cancellation and hidden controller disposal");
        }

        private static async Task LoadAsync(CoreWebView2 core, string html)
        {
            var completion = new TaskCompletionSource<bool>();
            EventHandler<CoreWebView2NavigationCompletedEventArgs> handler = (sender, args) => completion.TrySetResult(args.IsSuccess);
            core.NavigationCompleted += handler;
            try
            {
                var fileName = "fixture-" + (++fixtureSequence) + ".html";
                File.WriteAllText(Path.Combine(scriptFixtureDirectory, fileName), html);
                core.Navigate("https://chatgpt.com/" + fileName);
                var winner = await Task.WhenAny(completion.Task, Task.Delay(10000));
                Require(winner == completion.Task && await completion.Task, "Fixture navigation failed or timed out.");
            }
            finally { core.NavigationCompleted -= handler; }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
