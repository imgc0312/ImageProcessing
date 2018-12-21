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
    public partial class Form_Specification : HW1_PCXreader.OperationForm
    {
        protected Bitmap[] refImg = new Bitmap[(int)imgMode.Size];//ref view-> 0:original , 1:negative , 2:gray , 3:R , 4:G , 5:B
        public Bitmap refView
        {
            get
            {
                switch (mode)
                {
                    case (int)imgMode.ORI:
                        return refImg[0];
                    case (int)imgMode.NEG:
                        if (refImg[mode] == null)
                            refImg[mode] = MyDeal.negative(refImg[0]);
                        return refImg[mode];
                    case (int)imgMode.GRAY:
                        if (refImg[mode] == null)
                            refImg[mode] = MyDeal.gray(refImg[0]);
                        return refImg[mode];
                    case (int)imgMode.R:
                        if (refImg[mode] == null)
                            refImg[mode] = MyDeal.selectCh(refImg[0], (int)MyDeal.colorMode.R);
                        return refImg[mode];
                    case (int)imgMode.B:
                        if (refImg[mode] == null)
                            refImg[mode] = MyDeal.selectCh(refImg[0], (int)MyDeal.colorMode.B);
                        return refImg[mode];
                    case (int)imgMode.G:
                        if (refImg[mode] == null)
                            refImg[mode] = MyDeal.selectCh(refImg[0], (int)MyDeal.colorMode.G);
                        return refImg[mode];
                    default:
                        return refImg[0];
                }
           }
           set
           {
                if (value == null)
                {
                    for (int i = 0; i< (int)imgMode.Size; i++)
                        refImg[i] = null;
                }
                else
                {
                    refImg[0] = new Bitmap(value); // set origin Img
                    for (int i = 1; i< (int)imgMode.Size; i++) // clear other Img
                        refImg[i] = null;
                }
            }
        }

        /// <summary>
        /// function-->
        /// </summary>
        /// 
        public Form_Specification() : base()
        {
            InitializeComponent();
        }

        public Form_Specification(Form1 form1) : base(form1)
        {
            InitializeComponent();
            initialForm();
        }

        public Form_Specification(OperationForm form1) : base(form1)
        {
            InitializeComponent();
            initialForm();
        }

        protected new void initialForm()
        {
            openEnable = openEnable;
            specificationToolStripMenuItem.Enabled = false;
            mode = mode;
            //chart1 input origin histogram
            ChartArea chartArea = chart1.ChartAreas[0];
            chartArea.AxisX.Minimum = 0;
            chartArea.AxisX.Maximum = 255;
            chartArea.AxisY.Minimum = 0;
            chartArea.AxisY.Maximum = 1.0;
            //chart2 reference histogram
            chartArea = chart2.ChartAreas[0];
            chartArea.AxisX.Minimum = 0;
            chartArea.AxisX.Maximum = 255;
            chartArea.AxisY.Minimum = 0;
            chartArea.AxisY.Maximum = 1.0;
            //chart3 output histogram
            chartArea = chart3.ChartAreas[0];
            chartArea.AxisX.Minimum = 0;
            chartArea.AxisX.Maximum = 255;
            chartArea.AxisY.Minimum = 0;
            chartArea.AxisY.Maximum = 1.0;
        }

        protected override void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            base.pictureBox1_Paint(sender, e);
            buildChart(imgView, chart1);
            addLine(imgView, chart1);
            count();

        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            buildChart(outView, chart3);
            addLine(outView, chart3);
        }

        private void pictureBoxRef_Paint(object sender, PaintEventArgs e)
        {
            buildChart(refView, chart2);
            addLine(refView, chart2);
            count();
        }

        private void count()
        {
            outView = MyDeal.specification(imgView, refView);
        }

        private void addLine(Bitmap view, Chart chart)
        {//add cdf line
            try
            {
                Series target;
                if (view == null)
                    return;
                if (chart == null)
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

        protected void openPCX_Ref(string filePath, PictureBox pictureBox)
        {
            fileName = filePath;
            MyPCX thePCX = new MyPCX();
            if (!thePCX.from(filePath))
            {
                MessageBox.Show(filePath, "開啟失敗");
                return;
            }
            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
                pictureBox.Image = null;
            }
            refView = thePCX.getView();
            pictureBox.Image = refView;
            MessageBox.Show(filePath, "開啟成功");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                openPCX_Ref(openFileDialog1.FileName, pictureBoxRef);
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton here = (RadioButton)sender;
            if (here.Checked)
            {
                pictureBoxRef.Visible = true;
                chart1.Visible = false;
                chart2.Visible = false;
                chart3.Visible = false;
            }

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton here = (RadioButton)sender;
            if (here.Checked)
            {
                //pictureBoxTarget.Visible = true;
                chart1.Visible = false;
                chart2.Visible = true;
                chart3.Visible = false;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton here = (RadioButton)sender;
            if (here.Checked)
            {
                //pictureBoxTarget.Visible = true;
                chart1.Visible = true;
                chart2.Visible = false;
                chart3.Visible = false;
            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton here = (RadioButton)sender;
            if (here.Checked)
            {
                //pictureBoxTarget.Visible = true;
                chart1.Visible = false;
                chart2.Visible = false;
                chart3.Visible = true;
            }
        }

    }
}
