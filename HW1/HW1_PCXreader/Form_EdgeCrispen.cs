using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HW1_PCXreader
{
    public partial class Form_EdgeCrispen : HW1_PCXreader.Form_Blur
    {
        public Form_EdgeCrispen() :base()
        {
            InitializeComponent();
        }

        public Form_EdgeCrispen(Form1 form1) : base(form1)
        {
            InitializeComponent();
        }

        public Form_EdgeCrispen(OperationForm form1) : base(form1)
        {
            InitializeComponent();
        }

        protected override void button1_Click(object sender, EventArgs e)
        {
            base.button1_Click(sender, e);
            outView = MyDeal.subBitmap(imgView, countOutView, 127);
            double SNR = MyDeal.SNR(imgView, outView);
            string[] blurInfo = textBox2.Lines;
            blurInfo[2] = info[2] + SNR.ToString("0.00");
            textBox2.Lines = blurInfo;
        }
    }
}
