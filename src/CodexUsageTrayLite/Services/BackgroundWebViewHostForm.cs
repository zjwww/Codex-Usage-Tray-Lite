using System;
using System.Drawing;
using System.Windows.Forms;

namespace CodexUsageTrayLite.Services
{
    internal sealed class BackgroundWebViewHostForm : Form
    {
        private const int WsExToolWindow = 0x00000080;
        private const int WsExNoActivate = 0x08000000;

        public BackgroundWebViewHostForm(Size viewportSize)
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(-32000, -32000);
            ClientSize = viewportSize;
            Opacity = 0;
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var parameters = base.CreateParams;
                parameters.ExStyle |= WsExToolWindow | WsExNoActivate;
                return parameters;
            }
        }

        internal IntPtr CreateHostHandle()
        {
            return Handle;
        }

        internal bool SuppressesActivation
        {
            get { return ShowWithoutActivation && (CreateParams.ExStyle & WsExNoActivate) != 0; }
        }
    }
}
