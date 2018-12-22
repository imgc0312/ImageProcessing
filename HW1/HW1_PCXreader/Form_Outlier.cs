using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HW1_PCXreader
{
    public partial class Form_Outlier : HW1_PCXreader.OperationForm
    {
        public string[] info = new string[]{
            "Noise Clean \t:\t",
            "Weight \t\t:\t",
            "SNR(dB) \t\t:\t",
            "Cost Time (ms) \t:\t"
        };
        MyFilter countFilter = new MyFilter(3);

        public Form_Outlier() : base()
        {
            InitializeComponent();
            initialForm();
        }

        public Form_Outlier(Form1 form1) : base(form1)
        {
            InitializeComponent();
            initialForm();
        }

        public Form_Outlier(OperationForm form1) : base(form1)
        {
            InitializeComponent();
            initialForm();
        }

        protected new void initialForm()
        {
            openEnable = openEnable;
            outlierToolStripMenuItem.Enabled = false;
            mode = mode;
            textBox2.Lines = info;
            countFilter.setData(1.0);
            countFilter[1, 1] = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MyFilter.FilterCount countMethod = MyFilter.outlier;
            double SNR = 0.0;
            double costTime = 0;
            DateTime curTime = DateTime.Now;
            outView = MyDeal.filter2D(imgView, MyFilter.BorderMethod.NEAR, countMethod, countFilter);
            costTime = DateTime.Now.Subtract(curTime).TotalMilliseconds;
            SNR = MyDeal.SNR(imgView, outView);
            textBox2.Lines = countInfo(trackBar1.Value, (double)trackBar1.Value / 100, SNR, costTime);
        }

        private string[] countInfo(int cleanValue, double weight, double SNR, double time)
        {
            string[] newLines = new string[info.Length];
            info.CopyTo(newLines, 0);
            newLines[0] += "" + cleanValue;
            newLines[1] += "" + weight.ToString("0.00");
            newLines[2] += "" + SNR.ToString("0.00");
            newLines[3] += "" + time.ToString("0.00");
            return newLines;
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            TrackBar here = (TrackBar)sender;
            textBox1.Text = here.Value.ToString();
            countFilter[1, 1] = here.Value;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int value = 0;
            TextBox here = (TextBox)sender;
            if (!Int32.TryParse(here.Text, out value))
                value = -1;//no value

            if ((value < trackBar1.Minimum) || (value > trackBar1.Maximum))
            {// illegal number
                MessageBox.Show(here.Text + "is illegal. Please input an integer number(" + trackBar1.Minimum + "~" + trackBar1.Maximum + ")");
                here.Text = trackBar1.Value.ToString();
            }
            else
            {
                trackBar1.Value = value;
            }
        }
    }
}
