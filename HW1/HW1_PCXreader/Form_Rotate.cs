using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HW1_PCXreader
{
    public partial class Form_Rotate : HW1_PCXreader.OperationForm
    {
        public override int mode
        {
            get
            {
                return base.mode;
            }
            set
            {
                base.mode = value;

                try
                {
                    comboBox1.SelectedIndex = 0;
                    comboBox2.SelectedIndex = 0;
                    trackBar1.Value = 0;
                }
                catch
                {

                }

            }
        }
        protected override bool openEnable
        {
            get
            {
                return base.openEnable;
            }
            set
            {
                base.openEnable = value;
                try
                {
                    if (tableLayoutPanel1 != null)
                    {
                        if (value)
                            trackBar1.Value = 0;
                        tableLayoutPanel1.Enabled = !value;
                    }
                }
                catch
                {

                }

            }
        }

        protected ProgressMonitor progress = new ProgressMonitor();

        /// <summary>
        /// function-->
        /// </summary>

        public Form_Rotate() : base()
        {
            InitializeComponent();
            initialForm();
        }

        public Form_Rotate(Form1 form1) : base(form1)
        {
            InitializeComponent();
            initialForm();
        }

        public Form_Rotate(OperationForm form1) : base(form1)
        {
            InitializeComponent();
            initialForm();
        }

        protected new void initialForm()
        {
            openEnable = openEnable;
            rotateToolStripMenuItem.Enabled = false;
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            progress.view = progressBar1;
            progress.view.Visible = false;
        }

        protected override void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            base.clearToolStripMenuItem_Click(sender, e);
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }

        protected override void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            count();
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            int value = 0;
            TextBox here = (TextBox)sender;
            if (!Int32.TryParse(here.Text, out value))
                value = -1;//no value

            if ((value < -180) || (value > 180))
            {// illegal number
                MessageBox.Show("Please input an integer number(-180~180)");
                if (here.Tag.Equals("CH1"))
                    here.Text = trackBar1.Value.ToString();
                
            }
            else
            {
                if (here.Tag.Equals("CH1"))
                    trackBar1.Value = value;
            }
        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            TrackBar here = (TrackBar)sender;
            if (here.Tag.Equals("CH1"))
            {
                textBox1.Text = here.Value.ToString();
            }
            count();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            count();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            count();
        }

        private void count()
        {
            switch (comboBox2.SelectedIndex)
            {
                case 0:// rotate
                    groupBox5.Visible = true;
                    rotate();
                    break;
                case 1:// mirror
                    groupBox5.Visible = false;
                    mirror();
                    break;
                default:
                    groupBox5.Visible = true;
                    rotate();
                    break;
            }
        }

        private void rotate()
        {
           switch (comboBox1.SelectedIndex)
            {
                case 0:
                    outView = MyDeal.rotate(imgView, trackBar1.Value, MyDeal.RotateMethod.posi, progress);
                    break;
                case 1:
                default:
                    outView = MyDeal.rotate(imgView, trackBar1.Value, MyDeal.RotateMethod.nega, progress);
                    break;
            }
        }

        private void mirror()
        {
            outView = MyDeal.mirror(imgView, trackBar1.Value, progress);
        }

    }
}
