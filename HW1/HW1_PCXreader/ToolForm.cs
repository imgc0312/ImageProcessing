using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HW1_PCXreader
{
    public partial class ToolForm : HW1_PCXreader.EmptyForm
    {
        public ToolForm() : base()
        {
            InitializeComponent();
            initialForm();
        }

        public ToolForm(Form1 form1) : base(form1)
        {
            InitializeComponent();
            initialForm();
        }

        protected override void initialForm()
        {
            base.initialForm();
            menuStrip1.Enabled = false;
        }

        protected new void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            base.pictureBox_MouseMove(sender, e);
        }

        protected new void pictureBox_MouseLeave(object sender, EventArgs e)
        {
            base.pictureBox_MouseLeave(sender, e);
        }

    }
}
