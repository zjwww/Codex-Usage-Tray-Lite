using System;
using System.Drawing;
using System.Windows.Forms;
using CodexUsageTrayLite.Models;
using CodexUsageTrayLite.Services;

namespace CodexUsageTrayLite.UI
{
    internal sealed class AboutForm : ThemedForm
    {
        internal string DetailsText { get; private set; }

        public AboutForm(AppThemeMode themeMode = AppThemeMode.System, AppLanguage language = AppLanguage.English)
            : base(themeMode)
        {
            var ui = UiText.For(language);
            var info = AboutInfoProvider.GetCurrent();
            Text = ui.AboutDialogTitle;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            ClientSize = new Size(660, 440);

            var title = new Label
            {
                Text = info.ProgramName,
                AutoSize = false,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point),
                Location = new Point(30, 25),
                Size = new Size(600, 44),
                TextAlign = ContentAlignment.MiddleLeft
            };

            DetailsText = ui.BuildAboutDetails(info);

            var details = new TextBox
            {
                Text = DetailsText,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = SystemColors.Window,
                Location = new Point(30, 88),
                Size = new Size(600, 270),
                TabStop = false
            };

            var ok = new Button
            {
                Text = ui.Ok,
                DialogResult = DialogResult.OK,
                Location = new Point(516, 375),
                Size = new Size(114, 38)
            };

            Controls.AddRange(new Control[] { title, details, ok });
            AcceptButton = ok;
            CancelButton = ok;
            ApplyAppTheme(themeMode);
        }
    }
}
