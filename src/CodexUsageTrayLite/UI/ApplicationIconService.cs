using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace CodexUsageTrayLite.UI
{
    internal sealed class OwnedApplicationIcon : IDisposable
    {
        private Stream stream;

        internal OwnedApplicationIcon(Stream stream, Icon icon)
        {
            this.stream = stream;
            Icon = icon;
        }

        internal Icon Icon { get; private set; }

        public void Dispose()
        {
            var icon = Icon;
            Icon = null;
            if (icon != null) icon.Dispose();
            var ownedStream = stream;
            stream = null;
            if (ownedStream != null) ownedStream.Dispose();
        }
    }

    internal static class ApplicationIconService
    {
        internal const string LightResourceName = "CodexUsageTrayLite.Resources.AppIconLight.ico";
        internal const string DarkResourceName = "CodexUsageTrayLite.Resources.AppIconDark.ico";

        internal static OwnedApplicationIcon Create(bool dark)
        {
            var resourceName = dark ? DarkResourceName : LightResourceName;
            var stream = typeof(ApplicationIconService).Assembly.GetManifestResourceStream(resourceName);
            if (stream == null) throw new InvalidOperationException("Application icon resource is missing: " + resourceName);
            try
            {
                return new OwnedApplicationIcon(stream, new Icon(stream));
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }

        internal static int[] ReadEmbeddedSizes(bool dark)
        {
            var resourceName = dark ? DarkResourceName : LightResourceName;
            using (var stream = typeof(ApplicationIconService).Assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) throw new InvalidOperationException("Application icon resource is missing: " + resourceName);
                using (var reader = new BinaryReader(stream))
                {
                    if (reader.ReadUInt16() != 0 || reader.ReadUInt16() != 1)
                    {
                        throw new InvalidDataException("Application icon has an invalid ICO header.");
                    }
                    var count = reader.ReadUInt16();
                    var sizes = new int[count];
                    for (var index = 0; index < count; index++)
                    {
                        var width = reader.ReadByte();
                        var height = reader.ReadByte();
                        reader.ReadBytes(14);
                        var decodedWidth = width == 0 ? 256 : width;
                        var decodedHeight = height == 0 ? 256 : height;
                        if (decodedWidth != decodedHeight) throw new InvalidDataException("Application icon frame is not square.");
                        sizes[index] = decodedWidth;
                    }
                    return sizes;
                }
            }
        }

        internal static bool IsSystemAppearanceMessage(int message)
        {
            return message == 0x001A || message == 0x031A || message == 0x02E0;
        }
    }
}
