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
        public string[] info = new string[]{
            "Method \t\t:\t",
            "Kernel Size \t:\t",
            "SNR(dB) \t\t:\t",
            "Cost Time (ms) \t:\t"
        };

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
            textBox2.Lines = info;
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
            MyFilter.FilterCount countMethod = null;
            double SNR = 0.0;
            double costTime = 0;
            if (radioButton1.Checked)
                countMethod = MyFilter.meanBlur;
            else if (radioButton2.Checked)
                countMethod = MyFilter.medianBlur;
            DateTime curTime = DateTime.Now;
            outView = MyDeal.filter2D(imgView, MyFilter.BorderMethod.NULL, countMethod, new MyFilter(trackBar1.Value), progress);
            costTime = DateTime.Now.Subtract(curTime).TotalMilliseconds;
            SNR = MyDeal.SNR(imgView, outView);
            textBox2.Lines = countInfo(countMethod, trackBar1.Value, SNR, costTime);
        }

        private string[] countInfo(MyFilter.FilterCount countMethod, int kernelSize, double SNR, double time)
        {
            string[] newLines = new string[info.Length];
            info.CopyTo(newLines, 0);
            newLines[0] += "" + countMethod.Method.Name;
            newLines[1] += "" + kernelSize;
            newLines[2] += "" + SNR.ToString("0.00");
            newLines[3] += "" + time.ToString("0.00");
            return newLines;
        }
    }
}
