using System;
using System.IO;
using System.Threading;

namespace CodexUsageTrayLite.Services
{
    internal static class ProfileSessionManager
    {
        public static void ClearOwnProfile()
        {
            var target = Path.GetFullPath(Infrastructure.AppPaths.WebViewProfileDirectory).TrimEnd(Path.DirectorySeparatorChar);
            var expected = Path.GetFullPath(Path.Combine(Infrastructure.AppPaths.DataDirectory, "webview2-profile")).TrimEnd(Path.DirectorySeparatorChar);
            if (!string.Equals(target, expected, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Refusing to clear an unexpected profile path.");
            }
            if (!Directory.Exists(target))
            {
                return;
            }
            Exception last = null;
            for (var attempt = 0; attempt < 20; attempt++)
            {
                try
                {
                    Directory.Delete(target, true);
                    return;
                }
                catch (Exception ex)
                {
                    last = ex;
                    if (attempt < 19) Thread.Sleep(250);
                }
            }
            throw last ?? new IOException("The login profile could not be removed.");
        }
    }
}
