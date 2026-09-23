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
                ShowShortcutKeys = !string.IsNullOrEmpty(summaryText);
                Invalidate();
            }
        }
    }
}
