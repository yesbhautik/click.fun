using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClickTracker.Forms
{
    public partial class LoadingForm : Form
    {
        public LoadingForm()
        {
            InitializeComponent();
            this.Text = "Please Wait";
            this.Size = new Size(200, 100);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = false;

            var label = new Label
            {
                Text = "Loading...",
                AutoSize = true,
                Location = new Point(70, 30)
            };

            this.Controls.Add(label);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_NOCLOSE = 0x200;
                var cp = base.CreateParams;
                cp.ClassStyle |= CS_NOCLOSE;
                return cp;
            }
        }
    }
} 