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
            checkedListBox1.SetItemChecked(1, true);
            checkedListBox2.SetItemChecked(1, true);
            setInfo();
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

        private void checkedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            CheckedListBox here = (CheckedListBox)sender;
            if (e.CurrentValue == CheckState.Checked) return;
            for (int i = 0; i < here.Items.Count; i++)
            {
                here.SetSelected(i, false);
                here.SetItemChecked(i, false);
            }
            e.NewValue = CheckState.Checked;

            int select = -1;
            double rate = 1.0;
            select = e.Index;
            foreach (int i in checkedListBox1.CheckedIndices)
            {
                select = i;
                Debug.Print(i.ToString());
            }
            
            
            switch (select)
            {
                case 0:
                    rate = 0.5;
                    break;
                case 1:
                    rate = 1.0;
                    break;
                case 2:
                    rate = 2.0;
                    break;
                default:
                    rate = 1.0;
                    break;
            }
            tempView = MyDeal.resize(imgView, rate);

            select = e.Index;
            Debug.Print("++");
            foreach (int i in checkedListBox2.CheckedIndices)
            {
                select = i;
                Debug.Print(i.ToString());
            }
            Debug.Print("--");
            
            switch (select)
            {
                case 0:
                    rate = 0.5;
                    break;
                case 1:
                    rate = 1.0;
                    break;
                case 2:
                    rate = 2.0;
                    break;
                default:
                    rate = 1.0;
                    break;
            }
            outView = MyDeal.resize(tempView, rate);
            
            Debug.Print("==");
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            setInfo();
        }
    }
}
