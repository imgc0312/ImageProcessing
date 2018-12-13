using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HW2_video
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form_NormalPlayer form2 = new Form_NormalPlayer();
            this.Visible = false;
            form2.ShowDialog();
            this.Visible = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CompressForm form2 = new CompressForm();
            this.Visible = false;
            form2.ShowDialog();
            this.Visible = true;
        }
    }
}
