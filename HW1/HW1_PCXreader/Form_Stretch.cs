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
    public partial class Form_Stretch : HW1_PCXreader.OperationForm
    {
        static int[,] oPoints = { { 0, 0 }, { 255, 255 }, { 0, 0 }, { 255, 255 }, { 0, 0 }, { 255, 255 } };
        int[,] points = { { 0, 0 }, { 255, 255 }, { 0, 0 }, { 255, 255 }, { 0, 0 }, { 255, 255 } };
        public override int mode
        {
            get
            {
                return base.mode;
            }
            set
            {
                base.mode = value;
                if(tableLayoutPanel3 != null)
                {
                    if (chUse == 1)
                    {
                        tableLayoutPanel3.Visible = false;
                        tableLayoutPanel4.Visible = false;
                    }
                    else
                    {
                        tableLayoutPanel3.Visible = true;
                        tableLayoutPanel4.Visible = true;
                    }
                    count();
                }
            }
        }

        /// <summary>
        /// function-->
        /// </summary>

        public Form_Stretch() : base()
        {
            InitializeComponent();
        }


        public Form_Stretch(Form1 form1) : base(form1)
        {
            InitializeComponent();
            initialForm();
        }

        public Form_Stretch(OperationForm form1) : base(form1)
        {
            InitializeComponent();
            initialForm();
        }

        protected new void initialForm()
        {
            openEnable = openEnable;
            thresholdToolStripMenuItem.Enabled = false;
            radioButton2.Checked = true;
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
            chartArea.AxisY.Maximum = 255;
        }

        protected override void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            base.clearToolStripMenuItem_Click(sender, e);
            for(int i = 0; i < points.GetLength(0); i++)
            {
                for(int j = 0; j < points.GetLength(1); j++){
                    points[i,j] = oPoints[i,j];
                }
            }
            setAllTextBox();
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            TextBox here = (TextBox)sender;
            try
            {
                int target = Int32.Parse(here.Name.Replace("textBox", ""));
                if ((target < 1) || (target > 12))
                {
                    Debug.Print("textBox_TextChanged : (target < 1) || (target > 12) ->" + target);
                    return;
                }
                int value = 0;
                target -= 1;
                if (!Int32.TryParse(here.Text, out value))
                    value = -1;//no value
                if ((value < 0) || (value > 255))
                {// illegal number
                    MessageBox.Show("Please input an integer number(0~255)");
                    here.Text = points[target / 2, target % 2].ToString();
                }
                else
                {
                    points[target / 2, target % 2] = value;
                    if ((target % 4) == 0)
                    {// target is 1st point
                        target += 2;
                        if (points[target / 2, target % 2] < value)
                            points[target / 2, target % 2] = value;
                    }
                    else if ((target % 4) == 2)
                    {// target is 2nd point
                        target -= 2;
                        if (points[target / 2, target % 2] > value)
                            points[target / 2, target % 2] = value;
                    }
                    setAllTextBox();
                }
            }
            catch (Exception E)
            {
                Debug.Print(E.ToString());
            }
        }

        private void setAllTextBox()
        {
            string name = "textBox";
            for(int i = 1; i <= 12; i++)
            {
                TextBox target = (TextBox)this.Controls.Find(name + i, true)[0];
                target.Text = points[(i-1) / 2, (i-1) % 2].ToString();
            }
        }

        private void count()
        {
            StretchOption[] opts = new StretchOption[0];
            if(chUse == 1)
            {
                opts = new StretchOption[3];
                opts[0] = new StretchOption(0, 0, points[0, 0], points[0, 1]);
                opts[1] = new StretchOption(points[0, 0], points[0, 1], points[1, 0], points[1, 1]);
                opts[2] = new StretchOption(points[1, 0], points[1, 1], 255, 255);
            }
            else
            {
                opts = new StretchOption[3*3];
                opts[0] = new StretchOption(0, 0, points[0, 0], points[0, 1], MyDeal.colorMode.R);
                opts[1] = new StretchOption(points[0, 0], points[0, 1], points[1, 0], points[1, 1], MyDeal.colorMode.R);
                opts[2] = new StretchOption(points[1, 0], points[1, 1], 255, 255, MyDeal.colorMode.R);

                opts[3] = new StretchOption(0, 0, points[2, 0], points[2, 1], MyDeal.colorMode.G);
                opts[4] = new StretchOption(points[2, 0], points[2, 1], points[3, 0], points[3, 1], MyDeal.colorMode.G);
                opts[5] = new StretchOption(points[3, 0], points[3, 1], 255, 255, MyDeal.colorMode.G);

                opts[6] = new StretchOption(0, 0, points[4, 0], points[4, 1], MyDeal.colorMode.B);
                opts[7] = new StretchOption(points[4, 0], points[4, 1], points[5, 0], points[5, 1], MyDeal.colorMode.B);
                opts[8] = new StretchOption(points[5, 0], points[5, 1], 255, 255, MyDeal.colorMode.B);
            }
            outView = MyDeal.stretch(imgView, opts);
        }

        private void buildLine()
        {
            try
            {
                chart2.Series.Clear();
                Series target;
                if (outView == null)
                    return;
                switch (mode)
                {
                    case (int)imgMode.GRAY:
                        target = buildLine(MyDeal.colorMode.GRAY);
                        target.Points.AddXY(0, 0);
                        target.Points.AddXY(points[0,0], points[0, 1]);
                        target.Points.AddXY(points[1, 0], points[1, 1]);
                        target.Points.AddXY(255, 255);
                        chart2.Series.Add(target);
                        return;
                    case (int)imgMode.R:
                        target = buildLine(MyDeal.colorMode.R);
                        target.Points.AddXY(0, 0);
                        target.Points.AddXY(points[0, 0], points[0, 1]);
                        target.Points.AddXY(points[1, 0], points[1, 1]);
                        target.Points.AddXY(255, 255);
                        chart2.Series.Add(target);
                        return;
                    case (int)imgMode.G:
                        target = buildLine(MyDeal.colorMode.G);
                        target.Points.AddXY(0, 0);
                        target.Points.AddXY(points[0, 0], points[0, 1]);
                        target.Points.AddXY(points[1, 0], points[1, 1]);
                        target.Points.AddXY(255, 255);
                        chart2.Series.Add(target);
                        return;
                    case (int)imgMode.B:
                        target = buildLine(MyDeal.colorMode.B);
                        target.Points.AddXY(0, 0);
                        target.Points.AddXY(points[0, 0], points[0, 1]);
                        target.Points.AddXY(points[1, 0], points[1, 1]);
                        target.Points.AddXY(255, 255);
                        chart2.Series.Add(target);
                        return;
                    case (int)imgMode.ORI:
                    case (int)imgMode.NEG:
                    default:
                        target = buildLine(MyDeal.colorMode.R);
                        target.Points.AddXY(0, 0);
                        target.Points.AddXY(points[0, 0], points[0, 1]);
                        target.Points.AddXY(points[1, 0], points[1, 1]);
                        target.Points.AddXY(255, 255);
                        chart2.Series.Add(target);

                        target = buildLine(MyDeal.colorMode.G);
                        target.Points.AddXY(0, 0);
                        target.Points.AddXY(points[2, 0], points[2, 1]);
                        target.Points.AddXY(points[3, 0], points[3, 1]);
                        target.Points.AddXY(255, 255);
                        chart2.Series.Add(target);

                        target = buildLine(MyDeal.colorMode.B);
                        target.Points.AddXY(0, 0);
                        target.Points.AddXY(points[4, 0], points[4, 1]);
                        target.Points.AddXY(points[5, 0], points[5, 1]);
                        target.Points.AddXY(255, 255);
                        chart2.Series.Add(target);
                        return;
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
            }
        }

        private Series buildLine(MyDeal.colorMode mode)
        {
            Series series = new Series();
            series.MarkerStyle = MarkerStyle.Circle;
            series.ChartType = SeriesChartType.Line;

            String Name = " ??";
            series.Color = Color.Pink;
            switch (mode)
            {
                case MyDeal.colorMode.R:
                    Name = "R";
                    series.Color = Color.Red;
                    break;
                case MyDeal.colorMode.G:
                    Name = "G";
                    series.Color = Color.Green;
                    break;
                case MyDeal.colorMode.B:
                    Name = "B";
                    series.Color = Color.Blue;
                    break;
                case MyDeal.colorMode.GRAY:
                    Name = "gray";
                    series.Color = Color.DarkGray;
                    break;
            }
            series.Name = Name;
            return series;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            count();
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            buildChart(chart1);
            buildLine();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton here = (RadioButton)sender;
            if (here.Checked)
            {
                chart1.Visible = true;
                chart2.Visible = false;
            }
            else
            {
                chart1.Visible = false;
                chart2.Visible = true;
            }
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox here = (TextBox)sender;
            int value = -1;
            Int32.TryParse(here.Text, out value);
            switch (e.KeyCode)
            {
                case Keys.Up:
                    value += 10;
                    break;
                case Keys.Down:
                    value -= 10;
                    break;
            }
            here.Text = value.ToString();
        }
    }
}
