using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

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
        
        int selMode = 0;   // 0:origin , 1:negative , 2:gray , 3:R , 4:G , 5:B
        int mode
        {
            get
            {
                return selMode;
            }
            set
            {
                selMode = value;
                pictureBox1.Image = imgView;
            }
        }
        Bitmap originBitmap;
        Bitmap imgView
        {
            get
            {
                switch (mode)
                {
                    case 0:
                        return originBitmap;
                    default:
                        return originBitmap;
                }
                
            }
            set
            {
                originBitmap = new Bitmap(value);
            }
        }
        Series seriesR = new Series("R");
        Series seriesG = new Series("G");
        Series seriesB = new Series("B");


        /// <summary>
        /// function-->
        /// </summary>

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

        private void openPCX(string filePath)
        {
            fileName = filePath;
            thePCX.from(filePath);
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }
            textBox1.Lines = PCXinfo(thePCX);
            imgView = thePCX.getView();
            buildChart();
            pictureBox1.Image = imgView;
            MessageBox.Show(filePath, "開啟檔案");
        }

        private void buildChart()
        {
            try
            {
                chart1.Series.Clear();

                seriesR = MyDeal.buildSeries(originBitmap, (int)MyDeal.colorMode.R);
                chart1.Series.Add(seriesR);

                seriesG = MyDeal.buildSeries(originBitmap, (int)MyDeal.colorMode.G);
                chart1.Series.Add(seriesG);

                seriesB = MyDeal.buildSeries(originBitmap, (int)MyDeal.colorMode.B);
                chart1.Series.Add(seriesB);
            }
            catch(Exception e)
            {
                Debug.Print(e.ToString());
            }
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
                openPCX(openFileDialog1.FileName);
                mode = 0;
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

        private void openFileToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            openFileToolStripMenuItem_Click(sender, e);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            int X , Y;   //location in image
            int boxW = pictureBox1.Width;
            int boxH = pictureBox1.Height;
            int imgW, imgH;
            if(pictureBox1.Image == null)
            {
                imgW = boxW;
                imgH = boxH;
            }
            else
            {
                imgW = pictureBox1.Image.Width;
                imgH = pictureBox1.Image.Height;
            }
            X = e.X + (imgW - boxW) / 2;
            Y = e.Y + (imgH - boxH) / 2;
            try
            {
                if (X >= 0 && X < imgW && Y >= 0 && Y < imgH)
                {
                    toolStripStatusLabel1.Text = "( " + X + " , " + Y + " )";
                    if (originBitmap != null)
                    {
                        Color color = originBitmap.GetPixel(X, Y);
                        toolStripStatusLabel2.Text = "( " + color.R + " , " + color.G + " , " + color.B + " )";
                    }
                    else
                        toolStripStatusLabel2.Text = "( R , G , B )";
                }
                else
                {
                    toolStripStatusLabel1.Text = "( X , Y )";
                    toolStripStatusLabel2.Text = "( R , G , B )";
                }
            }
            catch
            {
                toolStripStatusLabel1.Text = "( X , Y )";
                toolStripStatusLabel2.Text = "( R , G , B )";
            }
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "( X , Y )";
            toolStripStatusLabel2.Text = "( R , G , B )";
        }

    }
}
