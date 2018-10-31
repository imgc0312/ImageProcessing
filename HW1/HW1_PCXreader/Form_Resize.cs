using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HW1_PCXreader
{
    public partial class Form_Resize : HW1_PCXreader.OperationForm
    {
        private new string[] info = new string[]{
            " Original ",
            "\t Width = ",
            "\t Height = ",
            "------------------",
            " Resized ",
            "\t Width = ",
            "\t Height = "
        };

        protected override bool openEnable
        {
            get
            {
                return base.openEnable;
            }
            set
            {
                base.openEnable = value;
                if(tableLayoutPanel1 != null)
                    tableLayoutPanel1.Enabled = !value;
            }
        }

        protected Bitmap tempView = null;

        protected ProgressMonitor progress = new ProgressMonitor();
        /// <summary>
        /// function-->
        /// </summary>
        public Form_Resize() : base()
        {
            InitializeComponent();
            initialForm();
        }

        public Form_Resize(Form1 form) : base(form)
        {
            InitializeComponent();
            initialForm();
        }

        public Form_Resize(OperationForm form) : base(form)
        {
            InitializeComponent();
            initialForm();
        }

        protected new void initialForm()
        {
            openEnable = openEnable;
            resizeToolStripMenuItem.Enabled = false;
            radioButton2.Checked = true;
            radioButton5.Checked = true;
            setInfo();
            progress.view = progressBar1;
            progress.view.Visible = false;
        }
        protected override void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            base.clearToolStripMenuItem_Click(sender, e);
            radioButton2.Checked = true;
            radioButton5.Checked = true;
        }

        private void setInfo()
        {
            string[] newLines = new string[info.Length];
            info.CopyTo(newLines, 0);
            if(imgView == null)
            {
                newLines[1] += "0";
                newLines[2] += "0";
            }
            else
            {
                newLines[1] += imgView.Width;
                newLines[2] += imgView.Height;
            }
            if(outView == null)
            {
                newLines[5] += "0";
                newLines[6] += "0";
            }
            else
            {
                newLines[5] += outView.Width;
                newLines[6] += outView.Height;
            }
            textBox1.Lines = newLines;
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            setInfo();
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton here = (RadioButton)sender;
            if (here.Checked)
                resize();
        }

        public void resize()
        {
            progress.start();
            double[] rate = { 0.5, 1.0, 2.0 };
            int choose = 1;
            foreach (RadioButton select in groupBox5.Controls)
            {
                if (select.Checked)
                    choose = select.TabIndex;
            }
            if ((choose < 0) || (choose > 2))
                choose = 1;
            progress.fine();
            tempView = MyDeal.resize(imgView, rate[choose], MyDeal.valueMethod.Near, progress);
            progress.start();
            foreach (RadioButton select in groupBox6.Controls)
            {
                if (select.Checked)
                    choose = select.TabIndex;
            }
            if ((choose < 0) || (choose > 2))
                choose = 1;
            progress.fine();
            outView = MyDeal.resize(tempView, rate[choose], MyDeal.valueMethod.Near, progress);

        }

        
    }
}
