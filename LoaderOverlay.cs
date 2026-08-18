using System.Drawing;
using System.Windows.Forms;

namespace SincroDatosApp
{

    public sealed class LoaderOverlay
    {
        private readonly Control _host;
        private readonly Panel _panel;
        private readonly Label _label;
        private readonly ProgressBar _bar;

        public LoaderOverlay(Control host)
        {
            _host = host;

            _panel = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false,
                BackColor = Color.FromArgb(160, Color.White),
            };

            _label = new Label
            {
                AutoSize = true,
                Text = "Procesando...",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            _bar = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Width = 320,
                Height = 18
            };

            var inner = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent
            };
            inner.Controls.Add(_label);
            inner.Controls.Add(_bar);

            var center = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Transparent
            };
            center.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            center.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            center.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            center.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            center.Controls.Add(inner, 0, 1);

            inner.Anchor = AnchorStyles.None;

            _panel.Controls.Add(center);
            _host.Controls.Add(_panel);
            _panel.BringToFront();
        }

        public void Show(string message = "Procesando...")
        {
            _label.Text = message;
            _panel.Visible = true;
            _panel.BringToFront();
            _panel.Refresh();
            Application.DoEvents();
        }

        public void Hide()
        {
            _panel.Visible = false;
        }
    }

}
