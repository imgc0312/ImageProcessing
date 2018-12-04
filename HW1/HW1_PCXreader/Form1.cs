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
        public string filePath = "unset";
        public string fileName
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
        public MyPCX thePCX = new MyPCX();
        public string[] info = new string[]{
            "File Name \t\t:\t",
            "Manufacturer \t\t:\t",
            "Version \t\t\t:\t",
            "Encoding \t\t\t:\t",
            "Bits Per Pixel \t\t:\t",
            "Xmin , Ymin, Xmax, Ymax \t:\t",
            "Hdpi , Vdpi \t\t:\t",
            "nPlanes \t\t\t:\t",
            "bytesPerLine \t\t:\t",
            "paletteInfo \t\t:\t",
            "H Screen Size , V Screen Size :\t"
        };
        public enum imgMode : int{ ORI, NEG, GRAY, R, G, B , Size };// 0:original , 1:negative , 2:gray , 3:R , 4:G , 5:B
        public int selMode = (int)imgMode.ORI;   // 0:original , 1:negative , 2:gray , 3:R , 4:G , 5:B
        public int mode
        {
            get
            {
                return selMode;
            }
            set
            {
                selMode = value;
                pictureBox1.Image = imgView;
                pictureBox2.Image = palView;
                buildChart();
                switch (value)
                {
                    case (int)imgMode.ORI:
                        toolStripStatusLabel0.Text = "Original";
                        break;
                    case (int)imgMode.NEG:
                        toolStripStatusLabel0.Text = "Negative";
                        break;
                    case (int)imgMode.GRAY:
                        toolStripStatusLabel0.Text = "Gray";
                        break;
                    case (int)imgMode.R:
                        toolStripStatusLabel0.Text = "Red";
                        break;
                    case (int)imgMode.G:
                        toolStripStatusLabel0.Text = "Green";
                        break;
                    case (int)imgMode.B:
                        toolStripStatusLabel0.Text = "Blue";
                        break;
                    default:
                        toolStripStatusLabel0.Text = "Unknown";
                        break;
                }
            }
        }

        public Bitmap[] Img = new Bitmap[(int)imgMode.Size];//image view-> 0:original , 1:negative , 2:gray , 3:R , 4:G , 5:B
        Bitmap[] Pal = new Bitmap[(int)imgMode.Size];//palette view-> 0:original , 1:negative , 2:gray , 3:R , 4:G , 5:B
        public Bitmap imgView
        {
            get
            {
                switch (mode)
                {
                    case (int)imgMode.ORI:
                        return Img[0];
                    case (int)imgMode.NEG:
                        if (Img[mode] == null)
                            Img[mode] = MyDeal.negative(Img[0]);
                        return Img[mode];
                    case (int)imgMode.GRAY:
                        if (Img[mode] == null)
                            Img[mode] = MyDeal.gray(Img[0]);
                        return Img[mode];
                    case (int)imgMode.R:
                        if (Img[mode] == null)
                            Img[mode] = MyDeal.selectCh(Img[0], (int)MyDeal.colorMode.R);
                        return Img[mode];
                    case (int)imgMode.B:
                        if (Img[mode] == null)
                            Img[mode] = MyDeal.selectCh(Img[0], (int)MyDeal.colorMode.B);
                        return Img[mode];
                    case (int)imgMode.G:
                        if (Img[mode] == null)
                            Img[mode] = MyDeal.selectCh(Img[0], (int)MyDeal.colorMode.G);
                        return Img[mode];
                    default:
                        return Img[0];
                }
            }
            set
            {
                if (value == null)
                {
                    for (int i = 0; i < (int)imgMode.Size; i++)
                        Img[i] = null;
                }
                else
                {
                    Img[0] = new Bitmap(value); // set origin Img
                    for (int i = 1; i < (int)imgMode.Size; i++) // clear other Img
                        Img[i] = null;
                }
            }
        }
        public Bitmap palView
        {
            get
            {
                switch (mode)
                {
                    case (int)imgMode.ORI:
                        return Pal[0];
                    case (int)imgMode.NEG:
                        if(Pal[mode] == null)
                            Pal[mode] = MyDeal.negative(Pal[0]);
                        return Pal[mode];
                    case (int)imgMode.GRAY:
                        if (Pal[mode] == null)
                            Pal[mode] = MyDeal.gray(Pal[0]);
                        return Pal[mode];
                    case (int)imgMode.R:
                        if (Pal[mode] == null)
                            Pal[mode] = MyDeal.selectCh(Pal[0], (int)MyDeal.colorMode.R);
                        return Pal[mode];
                    case (int)imgMode.B:
                        if (Pal[mode] == null)
                            Pal[mode] = MyDeal.selectCh(Pal[0], (int)MyDeal.colorMode.B);
                        return Pal[mode];
                    case (int)imgMode.G:
                        if (Pal[mode] == null)
                            Pal[mode] = MyDeal.selectCh(Pal[0], (int)MyDeal.colorMode.G);
                        return Pal[mode];
                    default:
                        return Pal[0];
                }
            }
            set
            {
                if (value == null)
                {
                    for (int i = 0; i < (int)imgMode.Size; i++)
                        Pal[i] = null;
                }
                else
                {
                    Pal[0] = new Bitmap(value); // set origin pal
                    for (int i = 1; i < (int)imgMode.Size; i++) // clear other pal
                        Pal[i] = null;
                }
            }
        }
        Series seriesT ;
        Series seriesR ;
        Series seriesG ;
        Series seriesB ;
        public string colorLabel
        {
            get
            {
                switch (mode)
                {
                    case (int)imgMode.GRAY:
                    case (int)imgMode.R:
                    case (int)imgMode.G:
                    case (int)imgMode.B:
                        return "( value )";
                    case (int)imgMode.ORI:
                    case (int)imgMode.NEG:
                    default:
                        return "( R , G , B )";
                }
            }
        }

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
            foreach (ToolStripMenuItem item in modeToolStripMenuItem.DropDownItems)// lock image mode 
            {
                item.Enabled = false;
            }
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

        private string getColorLabel(Color get)
        {
            switch (mode)
            {
                case (int)imgMode.GRAY:
                case (int)imgMode.R:
                case (int)imgMode.G:
                case (int)imgMode.B:
                    return "( " + get.R + " )";
                case (int)imgMode.ORI:
                case (int)imgMode.NEG:
                    return "( " + get.R + " , " + get.G + " , " + get.B + " )";
            }
            return colorLabel;
        }

        private string[] PCXinfo(MyPCX thePCX)
        {
            string[] newLines = new string[info.Length];
            info.CopyTo(newLines, 0);
            newLines[0] += "" + fileName;
            newLines[1] += "" + thePCX.header.manufacturer;
            newLines[2] += "" + thePCX.header.version;
            newLines[3] += "" + thePCX.header.encoding;
            newLines[4] += "" + thePCX.header.bitsPerPixel;
            newLines[5] += "( " + thePCX.header.Xmin + " , " +thePCX.header.Ymin + " , " + thePCX.header.Xmax + " , " + thePCX.header.Ymax + " )";
            newLines[6] += "( " + thePCX.header.Hdpi + " , " + thePCX.header.Vdpi + " )";
            newLines[7] += "" + thePCX.header.nPlanes;
            newLines[8] += "" + thePCX.header.bytesPerLine;
            newLines[9] += "" + thePCX.header.paletteInfo;
            newLines[10] += "( " + thePCX.header.hScreenSize + " , " + thePCX.header.vScreenSize + " )";
            return newLines;
        }

        private void openPCX(string filePath)
        {
            fileName = filePath;
            if (!thePCX.from(filePath))
            {
                MessageBox.Show(filePath, "開啟失敗");
                return;
            }
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }
            textBox1.Lines = PCXinfo(thePCX);
            imgView = thePCX.getView();
            palView = thePCX.getPalette();
            mode = (int)imgMode.ORI;
            MessageBox.Show(filePath, "開啟成功");
            foreach(ToolStripMenuItem item in modeToolStripMenuItem.DropDownItems)// unlock image mode 
            {
                item.Enabled = true;
            }
        }

        private void buildChart()
        {
            try
            {
                chart1.Series.Clear();
                if (imgView == null)
                    return;
                switch (mode)
                {
                    case (int)imgMode.GRAY:
                        seriesT = MyDeal.buildSeries(imgView, MyDeal.colorMode.GRAY);
                        chart1.Series.Add(seriesT);
                        return;
                    case (int)imgMode.R:
                        seriesT = MyDeal.buildSeries(imgView, MyDeal.colorMode.R);
                        chart1.Series.Add(seriesT);
                        return;
                    case (int)imgMode.G:
                        seriesT = MyDeal.buildSeries(imgView, MyDeal.colorMode.G);
                        chart1.Series.Add(seriesT);
                        return;
                    case (int)imgMode.B:
                        seriesT = MyDeal.buildSeries(imgView, MyDeal.colorMode.B);
                        chart1.Series.Add(seriesT);
                        return;
                    case (int)imgMode.ORI:
                    case (int)imgMode.NEG:
                    default:
                        seriesR = MyDeal.buildSeries(imgView, MyDeal.colorMode.R);
                        chart1.Series.Add(seriesR);

                        seriesG = MyDeal.buildSeries(imgView, MyDeal.colorMode.G);
                        chart1.Series.Add(seriesG);

                        seriesB = MyDeal.buildSeries(imgView, MyDeal.colorMode.B);
                        chart1.Series.Add(seriesB);
                        return;
                }
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
                    if (imgView != null)
                    {
                        Color color = imgView.GetPixel(X, Y);
                        toolStripStatusLabel2.Text = getColorLabel(color);
                    }
                    else
                        toolStripStatusLabel2.Text = colorLabel;
                }
                else
                {
                    toolStripStatusLabel1.Text = "( X , Y )";
                    toolStripStatusLabel2.Text = colorLabel;
                }
            }
            catch
            {
                toolStripStatusLabel1.Text = "( X , Y )";
                toolStripStatusLabel2.Text = colorLabel;
            }
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "( X , Y )";
            toolStripStatusLabel2.Text = colorLabel;
        }

        private void originalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mode = (int)imgMode.ORI;
        }

        private void negativeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mode = (int)imgMode.NEG;
        }

        private void grayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mode = (int)imgMode.GRAY;
        }

        private void RedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mode = (int)imgMode.R;
        }

        private void GreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mode = (int)imgMode.G;
        }

        private void BlueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mode = (int)imgMode.B;
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            int X, Y;   //location in image
            int boxW = pictureBox2.Width;
            int boxH = pictureBox2.Height;
            int imgW, imgH;
            if (pictureBox2.Image == null)
            {
                imgW = boxW;
                imgH = boxH;
            }
            else
            {
                imgW = pictureBox2.Image.Width;
                imgH = pictureBox2.Image.Height;
            }
            X = e.X + (imgW - boxW) / 2;
            Y = e.Y + (imgH - boxH) / 2;
            try
            {
                if (X >= 0 && X < imgW && Y >= 0 && Y < imgH)
                {
                    int index = (Y / MyPCX.blockSize) * MyPCX.pleCols + (X / MyPCX.blockSize);
                    toolStripStatusLabel1.Text = "( " + "# " + index + " )";
                    if (palView != null)
                    {
                        Color color = palView.GetPixel(X, Y);
                        toolStripStatusLabel2.Text = getColorLabel(color);
                    }
                    else
                        toolStripStatusLabel2.Text = colorLabel;
                }
                else
                {
                    toolStripStatusLabel1.Text = "( # Number )";
                    toolStripStatusLabel2.Text = colorLabel;
                }
            }
            catch
            {
                toolStripStatusLabel1.Text = "( # Number )";
                toolStripStatusLabel2.Text = colorLabel;
            }
        }

        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "( # Number )";
            toolStripStatusLabel2.Text = colorLabel;
        }

        private void thresholdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_Threshold form2 = new Form_Threshold(this);
            form2.ShowDialog();
        }

        private void resizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_Resize form2 = new Form_Resize(this);
            form2.ShowDialog();
        }

        private void rotateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_Rotate form2 = new Form_Rotate(this);
            form2.ShowDialog();
        }

        private void penetrateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_Penetrate form2 = new Form_Penetrate(this);
            form2.ShowDialog();
        }

        private void bitPlaneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_BitPlane form2 = new Form_BitPlane(this);
            form2.ShowDialog();
        }

        private void stretchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_Stretch form2 = new Form_Stretch(this);
            form2.ShowDialog();
        }

        private void equalizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_Equalization form2 = new Form_Equalization(this);
            form2.ShowDialog();
        }

        private void lowPassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_Blur form2 = new Form_Blur(this);
            form2.ShowDialog();
        }

        private void highBoostToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_HighBoost form2 = new Form_HighBoost(this);
            form2.ShowDialog();
        }

        private void gradientToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_Gradient form2 = new Form_Gradient(this);
            form2.ShowDialog();
        }
    }
}
