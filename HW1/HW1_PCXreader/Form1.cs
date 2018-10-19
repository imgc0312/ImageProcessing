using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HW1_PCXreader
{
    public partial class Form1 : Form
    {
        string filePath = "unset";
        string fileName
        {
            get
            {
                string[] fileAr = filePath.Split('\\');
                return fileAr[fileAr.Length - 1];
            }
            set
            {
                filePath = value;
            }
        }
        MyPCX thePCX = new MyPCX();
        string[] info = new string[]{
            "File Name ",
            "Manufacturer ",
            "Version ",
            "Encoding ",
            "Bits Per Pixel ",
            "Xmin , Xmax, Ymin, Ymax ",
            "Hdpi , Vdpi ",
            "nPlanes ",
            "bytesPerLine ",
            "paletteInfo ",
            "H Screen Size , V Screen Size "
        };
        Image originView;
        public Form1()
        {
            InitializeComponent();
            //openFileDialog Setting
            openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "PCX file(*.pcx)|*.pcx";
            openFileDialog1.Title = "Select a PCX file";
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;
            textBox1.Lines = info;
        }

        private string textFromLines(string[] lines)
        {
            string text = "";
            foreach(string oneLine in lines)
            {
                text += oneLine + "\r\n";
            }
            return text;
        }

        private string[] PCXinfo(MyPCX thePCX)
        {
            string[] newLines = new string[info.Length];
            info.CopyTo(newLines, 0);
            newLines[0] += "\t\t:\t" + fileName;
            newLines[1] += "\t\t:\t" + thePCX.header.manufacturer;
            newLines[2] += "\t\t\t:\t" + thePCX.header.version;
            newLines[3] += "\t\t\t:\t" + thePCX.header.encoding;
            newLines[4] += "\t\t:\t" + thePCX.header.bitsPerPixel;
            newLines[5] += "\t:\t( " + thePCX.header.Xmin + " , " +thePCX.header.Ymin + " , " + thePCX.header.Xmax + " , " + thePCX.header.Ymax + " )";
            newLines[6] += "\t\t:\t( " + thePCX.header.Hdpi + " , " + thePCX.header.Vdpi + " )";
            newLines[7] += "\t\t\t:\t" + thePCX.header.nPlanes;
            newLines[8] += "\t\t:\t" + thePCX.header.bytesPerLine;
            newLines[9] += "\t\t:\t" + thePCX.header.paletteInfo;
            newLines[10] += ":\t( " + thePCX.header.hScreenSize + " , " + thePCX.header.vScreenSize + " )";
            return newLines;
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void selectImageToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fileName = openFileDialog1.FileName;
                thePCX.from(filePath);
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                    pictureBox1.Image = null;
                }
                //string[] newLines = new string[info.Length];
                //info.CopyTo(newLines, 0);
                //newLines[0] += "\t:\t" + fileName;
                //newLines[1] += "\t:\t" + thePCX.header.manufacturer;
                //newLines[2] += "\t:\t" + thePCX.header.version;
                textBox1.Lines = PCXinfo(thePCX);
                originView = thePCX.getView();
                pictureBox1.Image = originView;
                MessageBox.Show(filePath, "開啟檔案");

            }
               
            
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void openFileToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            openFileToolStripMenuItem_Click(sender, e);
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

        }
    }
}
