using System;
using System.Drawing;
using System.Windows.Forms;

namespace CodexUsageTrayLite.UI
{
    internal enum TrayMenuTextRole
    {
        Default = 0,
        PrimaryStatus = 1,
        SecondaryStatus = 2
    }

    internal sealed class TrayMenuItem : ToolStripMenuItem
    {
        internal const int MinimumLogicalHeight = 34;
        internal const float SummaryFontScale = 0.88F;
        private string summaryText = string.Empty;

        public TrayMenuItem(string text)
            : this(text, TrayMenuTextRole.Default)
        {
        }

        public TrayMenuItem(string text, TrayMenuTextRole textRole)
            : base(text)
        {
            TextRole = textRole;
        }

        public TrayMenuTextRole TextRole { get; private set; }

        public string SummaryText
        {
            get { return summaryText; }
            set
            {
                summaryText = value ?? string.Empty;
                ShortcutKeys = Keys.None;
                ShortcutKeyDisplayString = summaryText;
                ShowShortcutKeys = summaryText.Length > 0;
                Invalidate();
            }
        }

        public override Size GetPreferredSize(Size constrainingSize)
        {
            var preferred = base.GetPreferredSize(constrainingSize);
            preferred.Height = Math.Max(preferred.Height, ScaleLogical(MinimumLogicalHeight));
            return preferred;
        }

        internal static Font CreateSummaryFont(Font source)
        {
            if (source == null) throw new ArgumentNullException("source");
            return new Font(
                source.FontFamily,
                Math.Max(6F, source.SizeInPoints * SummaryFontScale),
                source.Style,
                GraphicsUnit.Point);
        }

        internal int ScaleLogical(int logicalPixels)
        {
            var dpi = Owner == null ? 96 : Owner.DeviceDpi;
            return Math.Max(1, (int)Math.Round(logicalPixels * dpi / 96F));
        }
    }
}
