using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HW1_PCXreader
{
    public partial class Form_ImageView : HW1_PCXreader.EmptyForm
    {
        public Form_ImageView(Image view, int selMode)
        {
            InitializeComponent();
            mode = selMode;
            pictureBox1.Image = view;
            menuStrip1.Enabled = false;
            panel1.MaximumSize = new Size(this.MaximumSize.Width - this.MinimumSize.Width + pictureBox1.MinimumSize.Width, this.MaximumSize.Height - this.MinimumSize.Height + pictureBox1.MinimumSize.Height);
        }

        public Form_ImageView(Image view, int selMode, String Text)
        {
            InitializeComponent();
            mode = selMode;
            this.Text = Text;
            pictureBox1.Image = view;
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

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            PictureBox here = (PictureBox)sender;
            int newWidth = here.MinimumSize.Width;
            int newHeight = here.MinimumSize.Height;
            if (here.Image != null)
            {
                if (here.Image.Width >= newWidth)
                    newWidth = here.Image.Width;
                if (here.Image.Height >= newHeight)
                    newHeight = here.Image.Height;
            }
            here.Size = new Size(newWidth, newHeight);
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            PictureBox here = (PictureBox)sender;
            this.Size = new Size(this.MinimumSize.Width + here.Width - here.MinimumSize.Width, this.MinimumSize.Height + here.Height - here.MinimumSize.Height);
            panel1.Size = here.Size;
        }
    }
}
