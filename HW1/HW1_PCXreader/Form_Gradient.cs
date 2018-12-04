using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HW1_PCXreader
{
    public partial class Form_Gradient : HW1_PCXreader.OperationForm
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
        public Form_Gradient() : base()
        {
            InitializeComponent();
            initialForm();
        }
        
        public Form_Gradient(Form1 form1) : base(form1)
        {
            InitializeComponent();
            initialForm();
        }

        public Form_Gradient(OperationForm form1) : base(form1)
        {
            InitializeComponent();
            initialForm();
        }

        protected new void initialForm()
        {
            openEnable = openEnable;
            //highBoostToolStripMenuItem.Enabled = false;
            mode = mode;
            progress.view = progressBar1;
            progress.view.Visible = false;
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            textBox1.Lines = info;
        }
        
        
        private void button1_Click(object sender, EventArgs e)
        {
            MyFilter.FilterCount countMethod = null;
            double SNR = 0.0;
            double costTime = 0;
            DateTime curTime = DateTime.Now;
            outView = MyDeal.filter2D(imgView, MyFilter.BorderMethod.NEAR, countMethod, null, progress);
            costTime = DateTime.Now.Subtract(curTime).TotalMilliseconds;
            SNR = MyDeal.SNR(imgView, outView);
            textBox1.Lines = countInfo(comboBox1.Text, comboBox2.Text, SNR, costTime);
        }
        
        
        private string[] countInfo(string operatorName, string direction, double SNR, double time)
        {
            string[] newLines = new string[info.Length];
            info.CopyTo(newLines, 0);
            newLines[0] += "" + operatorName;
            newLines[1] += "" + direction;
            newLines[2] += "" + SNR.ToString("0.000000");
            newLines[3] += "" + time;
            return newLines;
        }

    }
}
