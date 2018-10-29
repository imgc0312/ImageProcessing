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
    public partial class Form_Threshold : HW1_PCXreader.OperationForm
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
                    trackBar1.Value = 0;
                    trackBar2.Value = 0;
                    trackBar3.Value = 0;
                    groupBox5.Enabled = (chUse >= 1);
                    groupBox6.Enabled = (chUse >= 2);
                    groupBox7.Enabled = (chUse >= 3);
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
                    if (value)
                    {
                        trackBar1.Value = 0;
                        trackBar2.Value = 0;
                        trackBar3.Value = 0;
                        groupBox5.Enabled = false;
                        groupBox6.Enabled = false;
                        groupBox7.Enabled = false;
                    }
                    else
                    {
                        groupBox5.Enabled = (chUse >= 1);
                        groupBox6.Enabled = (chUse >= 2);
                        groupBox7.Enabled = (chUse >= 3);
                    }
                }
                catch
                {
                   
                }
                
            }
        }

        public Form_Threshold() : base()
        {
            InitializeComponent();
            thresholdToolStripMenuItem.Enabled = false;
            openEnable = openEnable;
        }

        public Form_Threshold(Form1 form1) : base(form1)
        {
            InitializeComponent();
            thresholdToolStripMenuItem.Enabled = false;
            openEnable = openEnable;
        }

        public Form_Threshold(OperationForm form1) : base(form1)
        {
            InitializeComponent();
            thresholdToolStripMenuItem.Enabled = false;
            openEnable = openEnable;
        }

        protected override void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            base.clearToolStripMenuItem_Click(sender, e);
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            int value = 0;
            TextBox here = (TextBox)sender;
            if (!Int32.TryParse(here.Text, out value))
                value = -1;//no value

            if ((value < 0) || (value > 255))
            {// illegal number
                MessageBox.Show("Please input an integer number(0~255)");
                if (here.Tag.Equals("CH1"))
                    here.Text = trackBar1.Value.ToString();
                else if (here.Tag.Equals("CH2"))
                    here.Text = trackBar2.Value.ToString();
                else if (here.Tag.Equals("CH3"))
                    here.Text = trackBar3.Value.ToString();
            }
            else
            {
                if (here.Tag.Equals("CH1"))
                    trackBar1.Value = value;
                else if (here.Tag.Equals("CH2"))
                    trackBar2.Value = value;
                else if (here.Tag.Equals("CH3"))
                    trackBar3.Value = value;

            }

        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            TrackBar here = (TrackBar)sender;
            int targetCh = -1;
            if (here.Tag.Equals("CH1"))
            {
                textBox1.Text = here.Value.ToString();
                if (chUse == 1)// only one channel mode
                    targetCh = -1;
                else
                    targetCh = 1;
                trackBar2.Value = 0;// not support muilty channel threshold
                trackBar3.Value = 0;
            }
            else if (here.Tag.Equals("CH2"))
            {
                textBox2.Text = here.Value.ToString();
                targetCh = 2;
                trackBar1.Value = 0;// not support muilty channel threshold
                trackBar3.Value = 0;
            }
            else if (here.Tag.Equals("CH3"))
            {
                textBox3.Text = here.Value.ToString();
                targetCh = 3;
                trackBar1.Value = 0;// not support muilty channel threshold
                trackBar2.Value = 0;
            }
            outView = MyDeal.threshold(imgView, here.Value, targetCh);
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            buildChart(chart1);
        }
    }
}
