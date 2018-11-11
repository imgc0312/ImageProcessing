using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HW1_PCXreader
{
    public partial class Form_BitPlane : HW1_PCXreader.ToolForm
    {
        BitPlanes.CodingMethod CodingMode = BitPlanes.CodingMethod.Binary;
        Bitmap input1 = null;
        BitPlanes input1P = new BitPlanes();
        Bitmap Input1
        {
            get
            {
                return input1;
            }
            set
            {
                input1 = value;
                pictureBox1.Image = input1;
                input1P.froms(value, CodingMode);
            }
        }
        Bitmap input2 = null;
        BitPlanes input2P = new BitPlanes();
        Bitmap Input2
        {
            get
            {
                return input2;
            }
            set
            {
                input2 = value;
                pictureBox2.Image = input2;
                input2P.froms(value, CodingMode);
            }
        }
        Bitmap output = null;
        Bitmap Output
        {
            get
            {
                return output;
            }
            set
            {
                output = value;
                pictureBox3.Image = output;
            }
        }

        BitPlanes planes = new BitPlanes();

        /// <summary>
        /// function-->
        /// </summary>

        public Form_BitPlane() : base()
        {
            InitializeComponent();
            mode = (int)imgMode.GRAY;
        }

        public Form_BitPlane(Form1 form1) : base(form1)
        {
            InitializeComponent();
            Input1 = MyDeal.gray(form1.Img[0]);
            mode = (int)imgMode.GRAY;
            count();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Input1 = openPCX(openFileDialog1.FileName, null, pictureBox1, MyDeal.gray);
                mode = (int)imgMode.GRAY;
            }
            else
            {
                Input1 = null;
            }
            count();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Input1 = null;
            count();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Input2 = openPCX(openFileDialog1.FileName, null, pictureBox2, MyDeal.gray);
                mode = (int)imgMode.GRAY;
            }
            else
            {
                Input2 = null;
            }
            count();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Input2 = null;
            count();
        }

        private void count()
        {
            Input1 = Input1;
            Input2 = Input2;
            for(int i = 7; i >= trackBar1.Value; i--)
            {
                planes[i] = input1P[i];
            }
            for(int i = trackBar1.Value - 1, j = 7; (i >= 0) && (j >= 0); i--, j--)
            {
                planes[i] = input2P[j];
            }

            pictureBox4.Image = planes[7];
            pictureBox5.Image = planes[6];
            pictureBox6.Image = planes[5];
            pictureBox7.Image = planes[4];
            pictureBox8.Image = planes[3];
            pictureBox9.Image = planes[2];
            pictureBox10.Image = planes[1];
            pictureBox11.Image = planes[0];
            Output = planes.merge(CodingMode);
        }

        private new void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            base.pictureBox_MouseMove(sender, e);
        }

        private new void pictureBox_MouseLeave(object sender, EventArgs e)
        {
            base.pictureBox_MouseLeave(sender, e);
        }

        private new void pictureBox_DoubleClick(object sender, EventArgs e)
        {
            base.pictureBox_DoubleClick(sender, e);
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            TrackBar here = (TrackBar)sender;
            textBox1.Text = here.Value.ToString();
            count();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int value = 0;
            TextBox here = (TextBox)sender;
            if (!Int32.TryParse(here.Text, out value))
                value = -1;//no value

            if ((value < 0) || (value > 8))
            {// illegal number
                MessageBox.Show("Please input an integer number(0~8)");
                here.Text = trackBar1.Value.ToString();
            }
            else
            {
                trackBar1.Value = value;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton here = (RadioButton)sender;
            if (here.Checked)
            {
                flowLayoutPanel1.Visible = true;
                pictureBox3.Visible = false;
            }
            else
            {
                flowLayoutPanel1.Visible = false;
                pictureBox3.Visible = true;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton here = (RadioButton)sender;
            if (here.Checked)
                CodingMode = BitPlanes.CodingMethod.Binary;
            else
                CodingMode = BitPlanes.CodingMethod.GrayCode;
            count();
        }
    }
}
