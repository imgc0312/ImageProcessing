using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HW1_PCXreader
{
    public partial class Form_HighBoost : HW1_PCXreader.OperationForm
    {
        protected ProgressMonitor progress = new ProgressMonitor();
        public string[] info = new string[]{
            "Mask \t\t:\t",
            "Weight \t\t:\t",
            "SNR(dB) \t\t:\t",
            "Cost Time (ms) \t:\t"
        };
        MyFilter countFilter = new MyFilter(3);

        /// <summary>
        /// function-->
        /// </summary>
        /// 
        public Form_HighBoost() : base()
        {
            InitializeComponent();
            initialForm();
        }

        public Form_HighBoost(Form1 form1) : base(form1)
        {
            InitializeComponent();
            initialForm();
        }

        public Form_HighBoost(OperationForm form1) : base(form1)
        {
            InitializeComponent();
            initialForm();
        }

        protected new void initialForm()
        {
            openEnable = openEnable;
            highBoostToolStripMenuItem.Enabled = false;
            mode = mode;
            progress.view = progressBar1;
            progress.view.Visible = false;
            comboBox1.SelectedIndex = 1;
            textBox1.Lines = info;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MyFilter.FilterCount countMethod = MyFilter.highBoost;
            double SNR = 0.0;
            double costTime = 0;
            DateTime curTime = DateTime.Now;
            outView = MyDeal.filter2D(imgView, MyFilter.BorderMethod.NEAR, countMethod, countFilter, progress);
            costTime = DateTime.Now.Subtract(curTime).TotalMilliseconds;
            SNR = MyDeal.SNR(imgView, outView);
            textBox1.Lines = countInfo(comboBox1.Text, (double)trackBar1.Value / 100, SNR, costTime);
        }

        private string[] countInfo(string maskName, double weight, double SNR, double time)
        {
            string[] newLines = new string[info.Length];
            info.CopyTo(newLines, 0);
            newLines[0] += "" + maskName;
            newLines[1] += "" + weight.ToString("0.00");
            newLines[2] += "" + SNR.ToString("0.000000");
            newLines[3] += "" + time;
            return newLines;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            countFilter = MyFilter.HighPassKernel(comboBox1.SelectedIndex);
            countFilter[1, 1] = (1 + (double)trackBar1.Value / 100) * countFilter[1, 1] - 1;
            setMaskView();
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            countFilter = MyFilter.HighPassKernel(comboBox1.SelectedIndex);
            countFilter[1, 1] = (1 + (double)trackBar1.Value / 100) * countFilter[1, 1] - 1;
            setMaskView();
        }

        private void setMaskView()
        {
            string name = "textBox";
            for (int i = 0; i < 9; i++)
            {
                TextBox target = (TextBox)tableLayoutPanel3.Controls.Find(name + (2+i), true)[0];
                target.Text = countFilter[i%3,i/3].ToString();
            }
        }
    }
}
