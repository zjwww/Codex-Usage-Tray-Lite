using System;
using System.Drawing;
using System.Windows.Forms;
using CodexUsageTrayLite.Models;
using CodexUsageTrayLite.Services;

namespace CodexUsageTrayLite.UI
{
    internal sealed class ProxySettingsForm : ThemedForm
    {
        private readonly ComboBox modeBox;
        private readonly TextBox hostBox;
        private readonly TextBox portBox;
        private readonly Label modeDescription;
        private readonly UiText ui;

        public ProxySettings SelectedProxy { get; private set; }

        public ProxySettingsForm(ProxySettings current, AppThemeMode themeMode = AppThemeMode.System, AppLanguage language = AppLanguage.English)
            : base(themeMode)
        {
            ui = UiText.For(language);
            current = current ?? new ProxySettings { mode = ProxyMode.System, host = "127.0.0.1", port = 7890 };
            Text = ui.ProxyDialogTitle;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            ClientSize = new Size(620, 340);

            var modeLabel = new Label
            {
                Text = ui.ProxyModeLabel,
                AutoSize = false,
                Location = new Point(32, 37),
                Size = new Size(145, 34),
                TextAlign = ContentAlignment.MiddleLeft
            };
            modeBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(210, 38), Width = 370 };
            modeBox.Items.AddRange(new object[] { ui.ProxySystem, ui.ProxyDirect, ui.ProxyHttp, ui.ProxySocks5 });
            var selectedMode = (int)current.mode;
            modeBox.SelectedIndex = selectedMode >= 0 && selectedMode < modeBox.Items.Count ? selectedMode : (int)ProxyMode.System;
            modeBox.SelectedIndexChanged += (sender, args) => UpdateEnabledState();

            modeDescription = new Label
            {
                AutoSize = false,
                ForeColor = SystemColors.GrayText,
                Location = new Point(210, 76),
                Size = new Size(370, 46),
                TextAlign = ContentAlignment.TopLeft
            };

            var hostLabel = new Label
            {
                Text = ui.ProxyHostLabel,
                AutoSize = false,
                Location = new Point(32, 137),
                Size = new Size(145, 34),
                TextAlign = ContentAlignment.MiddleLeft
            };
            hostBox = new TextBox { Location = new Point(210, 139), Width = 370, Text = current.host ?? "127.0.0.1" };
            var portLabel = new Label
            {
                Text = ui.ProxyPortLabel,
                AutoSize = false,
                Location = new Point(32, 193),
                Size = new Size(145, 34),
                TextAlign = ContentAlignment.MiddleLeft
            };
            portBox = new TextBox { Location = new Point(210, 195), Width = 170, Text = current.port.ToString() };

            var save = new Button { Text = ui.Save, Location = new Point(352, 274), Size = new Size(108, 38) };
            save.Click += HandleSave;
            var cancel = new Button { Text = ui.Cancel, DialogResult = DialogResult.Cancel, Location = new Point(472, 274), Size = new Size(108, 38) };

            Controls.AddRange(new Control[] { modeLabel, modeBox, modeDescription, hostLabel, hostBox, portLabel, portBox, save, cancel });
            AcceptButton = save;
            CancelButton = cancel;
            UpdateEnabledState();
        }

        protected override void OnAppThemeApplied(ThemePalette palette)
        {
            base.OnAppThemeApplied(palette);
            modeDescription.ForeColor = palette.MutedText;
        }

        private void UpdateEnabledState()
        {
            var custom = modeBox.SelectedIndex == (int)ProxyMode.Http || modeBox.SelectedIndex == (int)ProxyMode.Socks5;
            hostBox.Enabled = custom;
            portBox.Enabled = custom;
            switch ((ProxyMode)modeBox.SelectedIndex)
            {
                case ProxyMode.Direct:
                    modeDescription.Text = ui.ProxyDirectDescription;
                    break;
                case ProxyMode.Http:
                    modeDescription.Text = ui.ProxyHttpDescription;
                    break;
                case ProxyMode.Socks5:
                    modeDescription.Text = ui.ProxySocks5Description;
                    break;
                default:
                    modeDescription.Text = ui.ProxySystemDescription;
                    break;
            }
            ApplyAppTheme(ThemeMode);
        }

        private void HandleSave(object sender, EventArgs args)
        {
            int port;
            if (!int.TryParse(portBox.Text, out port)) port = 0;
            var selected = new ProxySettings
            {
                mode = (ProxyMode)modeBox.SelectedIndex,
                host = (hostBox.Text ?? string.Empty).Trim(),
                port = port
            };
            try
            {
                ProxyArgumentBuilder.Validate(selected);
                SelectedProxy = selected;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                LocalizedMessageBox.Show(this, ui.LocalizeProxyValidation(ex), ui.InvalidProxySettings,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, ThemeMode, ui.Language);
            }
        }
    }
}
