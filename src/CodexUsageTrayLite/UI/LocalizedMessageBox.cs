using System;
using System.Drawing;
using System.Windows.Forms;
using CodexUsageTrayLite.Models;

namespace CodexUsageTrayLite.UI
{
    internal static class LocalizedMessageBox
    {
        public static DialogResult Show(
            IWin32Window owner,
            string message,
            string title,
            MessageBoxButtons buttons,
            MessageBoxIcon icon,
            AppThemeMode themeMode,
            AppLanguage language)
        {
            using (var dialog = new LocalizedMessageForm(message, title, buttons, icon, themeMode, language, owner != null))
            {
                return owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
            }
        }

        private sealed class LocalizedMessageForm : ThemedForm
        {
            public LocalizedMessageForm(
                string message,
                string title,
                MessageBoxButtons buttons,
                MessageBoxIcon icon,
                AppThemeMode themeMode,
                AppLanguage language,
                bool hasOwner)
                : base(themeMode)
            {
                var ui = UiText.For(language);
                Text = title ?? AppConstants.Name;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = hasOwner ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowInTaskbar = false;
                AutoScaleMode = AutoScaleMode.Dpi;
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

                const int textWidth = 490;
                var measured = TextRenderer.MeasureText(message ?? string.Empty, Font, new Size(textWidth, 0),
                    TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
                var textHeight = Math.Max(62, Math.Min(330, measured.Height + 10));
                ClientSize = new Size(620, textHeight + 98);

                var picture = new PictureBox
                {
                    Image = ResolveIcon(icon).ToBitmap(),
                    Location = new Point(28, 30),
                    Size = new Size(40, 40),
                    SizeMode = PictureBoxSizeMode.StretchImage
                };
                var text = new Label
                {
                    Text = message ?? string.Empty,
                    AutoSize = false,
                    Location = new Point(92, 26),
                    Size = new Size(textWidth, textHeight),
                    TextAlign = ContentAlignment.TopLeft
                };

                if (buttons == MessageBoxButtons.YesNo)
                {
                    var yes = CreateButton(ui.Yes, DialogResult.Yes, ClientSize.Width - 240, ClientSize.Height - 58);
                    var no = CreateButton(ui.No, DialogResult.No, ClientSize.Width - 124, ClientSize.Height - 58);
                    Controls.AddRange(new Control[] { picture, text, yes, no });
                    AcceptButton = yes;
                    CancelButton = no;
                }
                else
                {
                    var ok = CreateButton(ui.Ok, DialogResult.OK, ClientSize.Width - 124, ClientSize.Height - 58);
                    Controls.AddRange(new Control[] { picture, text, ok });
                    AcceptButton = ok;
                    CancelButton = ok;
                }
                ApplyAppTheme(themeMode);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    foreach (Control control in Controls)
                    {
                        var picture = control as PictureBox;
                        if (picture != null && picture.Image != null)
                        {
                            picture.Image.Dispose();
                            picture.Image = null;
                        }
                    }
                }
                base.Dispose(disposing);
            }

            private static Button CreateButton(string text, DialogResult result, int left, int top)
            {
                return new Button { Text = text, DialogResult = result, Location = new Point(left, top), Size = new Size(104, 38) };
            }

            private static Icon ResolveIcon(MessageBoxIcon icon)
            {
                switch (icon)
                {
                    case MessageBoxIcon.Error: return SystemIcons.Error;
                    case MessageBoxIcon.Warning: return SystemIcons.Warning;
                    case MessageBoxIcon.Question: return SystemIcons.Question;
                    default: return SystemIcons.Information;
                }
            }
        }
    }
}
