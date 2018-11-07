using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HW1_PCXreader
{
    public partial class Form_Penetrate : HW1_PCXreader.ToolForm
    {
        Bitmap input1 = null;
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
            }
        }
        Bitmap input2 = null;
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

        /// <summary>
        /// function-->
        /// </summary>

        public Form_Penetrate() : base()
        {
            InitializeComponent();
            mode = (int)imgMode.ORI;
        }

        public Form_Penetrate(Form1 form1) : base(form1)
        {
            InitializeComponent();
            Input1 = form1.Img[0];
            mode = (int)imgMode.ORI;
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            int value = 100;
            TextBox here = (TextBox)sender;
            if (!Int32.TryParse(here.Text, out value))
                value = -1;//no value

            if ((value < 0) || (value > 100))
            {// illegal number
                MessageBox.Show("Please input an integer number(0~100)");
                if (here.Tag.Equals("CH1"))
                    here.Text = trackBar1.Value.ToString();
            }
            else
            {
                if (here.Tag.Equals("CH1"))
                    trackBar1.Value = value;
            }
        }

        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            TrackBar here = (TrackBar)sender;
            if (here.Tag.Equals("CH1"))
            {
                textBox1.Text = here.Value.ToString();
            }
            count();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Input1 = openPCX(openFileDialog1.FileName, null, pictureBox1);
                mode = (int)imgMode.ORI;
            }
            else
            {
                Input1 = null;
            }
            count();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Input2 = openPCX(openFileDialog1.FileName, null, pictureBox2);
                mode = (int)imgMode.ORI;
            }
            else
            {
                Input2 = null;
            }
            count();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap temp = Input1;
            Input1 = Input2;
            Input2 = temp;
            count();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Input1 = null;
            Input2 = null;
            trackBar1.Value = 100;
            count();
        }

        private void count()
        {
            Output = MyDeal.opacity(Input1, Input2, trackBar1.Value);
        }
    }
}
