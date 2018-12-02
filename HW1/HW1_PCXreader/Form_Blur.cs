using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HW1_PCXreader
{
    public partial class Form_Blur : HW1_PCXreader.OperationForm
    {

        protected ProgressMonitor progress = new ProgressMonitor();
        /// <summary>
        /// function-->
        /// </summary>
        /// 
        public Form_Blur() : base()
        {
            InitializeComponent();
        }

        public Form_Blur(Form1 form1) : base(form1)
        {
            InitializeComponent();
            initialForm();
        }

        public Form_Blur(OperationForm form1) : base(form1)
        {
            InitializeComponent();
            initialForm();
        }

        protected new void initialForm()
        {
            openEnable = openEnable;
            lowPassToolStripMenuItem.Enabled = false;
            mode = mode;
            progress.view = progressBar1;
            progress.view.Visible = false;
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            TrackBar here = (TrackBar)sender;
            textBox1.Text = here.Value.ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int value = 0;
            TextBox here = (TextBox)sender;
            if (!Int32.TryParse(here.Text, out value))
                value = -1;//no value

            if ((value < trackBar1.Minimum) || (value > trackBar1.Maximum))
            {// illegal number
                MessageBox.Show("Please input an integer number(" + trackBar1.Minimum + "~" + trackBar1.Maximum + ")");
                here.Text = trackBar1.Value.ToString();
            }
            else
            {
                trackBar1.Value = value;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(radioButton1.Checked)
                outView = MyDeal.filter2D(imgView, MyFilter.BorderMethod.NULL, MyFilter.blur, new MyFilter(trackBar1.Value), progress);
            else if(radioButton2.Checked)
                outView = MyDeal.filter2D(imgView, MyFilter.BorderMethod.NULL, MyFilter.blur, new MyFilter(trackBar1.Value), progress);

        }
    }
}
