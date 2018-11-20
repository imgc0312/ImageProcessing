using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace HW1_PCXreader
{
    public partial class Form_Equalization : HW1_PCXreader.OperationForm
    {

        /// <summary>
        /// function-->
        /// </summary>

        public Form_Equalization() : base()
        {
            InitializeComponent();
        }


        public Form_Equalization(Form1 form1) : base(form1)
        {
            InitializeComponent();
            initialForm();
        }

        public Form_Equalization(OperationForm form1) : base(form1)
        {
            InitializeComponent();
            initialForm();
        }

        protected new void initialForm()
        {
            openEnable = openEnable;
            equalizeToolStripMenuItem.Enabled = false;
            mode = mode;
            //chart1
            ChartArea chartArea = chart1.ChartAreas[0];
            chartArea.AxisX.Minimum = 0;
            chartArea.AxisX.Maximum = 255;
            //chart2
            chartArea = chart2.ChartAreas[0];
            chartArea.AxisX.Minimum = 0;
            chartArea.AxisX.Maximum = 255;
            chartArea.AxisY.Minimum = 0;
            chartArea.AxisY.Maximum = 1.0;
        }

        protected override void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            base.pictureBox1_Paint(sender, e);
            count();
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            buildChart(outView, chart1);
            buildLine(outView, chart2);
        }

        private void count()
        {
            outView = MyDeal.equalize(imgView);
        }

        private void buildLine(Bitmap view, Chart chart)
        {
            try
            {
                chart.Series.Clear();
                Series target;
                if (outView == null)
                    return;
                switch (mode)
                {
                    case (int)imgMode.GRAY:
                        target = MyDeal.buildSeries(view, MyDeal.colorMode.GRAY, MyDeal.seriesMode.CDF);
                        chart.Series.Add(target);
                        return;
                    case (int)imgMode.R:
                        target = MyDeal.buildSeries(view, MyDeal.colorMode.R, MyDeal.seriesMode.CDF);
                        chart.Series.Add(target);
                        return;
                    case (int)imgMode.G:
                        target = MyDeal.buildSeries(view, MyDeal.colorMode.G, MyDeal.seriesMode.CDF);
                        chart.Series.Add(target);
                        return;
                    case (int)imgMode.B:
                        target = MyDeal.buildSeries(view, MyDeal.colorMode.B, MyDeal.seriesMode.CDF);
                        chart.Series.Add(target);
                        return;
                    case (int)imgMode.ORI:
                    case (int)imgMode.NEG:
                    default:
                        target = MyDeal.buildSeries(view, MyDeal.colorMode.R, MyDeal.seriesMode.CDF);
                        chart.Series.Add(target);
                        target = MyDeal.buildSeries(view, MyDeal.colorMode.G, MyDeal.seriesMode.CDF);
                        chart.Series.Add(target);
                        target = MyDeal.buildSeries(view, MyDeal.colorMode.B, MyDeal.seriesMode.CDF);
                        chart.Series.Add(target);
                        return;
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString() + e.StackTrace);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            buildChart(imgView, chart1);
            buildLine(imgView, chart2);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            buildChart(outView, chart1);
            buildLine(outView, chart2);
        }
    }
}
