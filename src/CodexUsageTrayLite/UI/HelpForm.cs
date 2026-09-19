using System;
using System.Drawing;
using System.Windows.Forms;
using CodexUsageTrayLite.Models;

namespace CodexUsageTrayLite.UI
{
    internal sealed class HelpForm : ThemedForm
    {
        internal string HelpText { get; private set; }

        public HelpForm(AppThemeMode themeMode = AppThemeMode.System, AppLanguage language = AppLanguage.English)
            : base(themeMode)
        {
            var ui = UiText.For(language);
            Text = ui.HelpDialogTitle;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            ClientSize = new Size(680, 620);

            var title = new Label
            {
                Text = ui.HelpHeading,
                AutoSize = false,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point),
                Location = new Point(30, 25),
                Size = new Size(620, 44),
                TextAlign = ContentAlignment.MiddleLeft
            };

            HelpText = NormalizeMultilineText(ui.HelpBody);

            var help = new TextBox
            {
                Text = HelpText,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = SystemColors.Window,
                Location = new Point(30, 88),
                Size = new Size(620, 460),
                TabStop = false
            };

            var ok = new Button
            {
                Text = ui.Ok,
                DialogResult = DialogResult.OK,
                Location = new Point(536, 565),
                Size = new Size(114, 38)
            };

            Controls.AddRange(new Control[] { title, help, ok });
            AcceptButton = ok;
            CancelButton = ok;
            ApplyAppTheme(themeMode);
        }

        internal static string NormalizeMultilineText(string value)
        {
            return (value ?? string.Empty)
                .Replace("\r\n", "\n")
                .Replace('\r', '\n')
                .Replace("\n", Environment.NewLine);
        }
    }
}
