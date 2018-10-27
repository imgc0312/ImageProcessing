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
    public partial class OperationForm : Form
    {
        protected string filePath = "unset";
        protected string fileName
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
        protected MyPCX thePCX = new MyPCX();
        protected string[] info = new string[]{
            "File Name \t\t:\t",
            "Manufacturer \t\t:\t",
            "Version \t\t\t:\t",
            "Encoding \t\t\t:\t",
            "Bits Per Pixel \t\t:\t",
            "Xmin , Xmax, Ymin, Ymax \t:\t",
            "Hdpi , Vdpi \t\t:\t",
            "nPlanes \t\t\t:\t",
            "bytesPerLine \t\t:\t",
            "paletteInfo \t\t:\t",
            "H Screen Size , V Screen Size :\t"
        };
        public enum imgMode : int { ORI, NEG, GRAY, R, G, B, Size };// 0:original , 1:negative , 2:gray , 3:R , 4:G , 5:B
        protected int selMode = (int)imgMode.ORI;   // 0:original , 1:negative , 2:gray , 3:R , 4:G , 5:B
        protected int mode
        {
            get
            {
                return selMode;
            }
            set
            {
                selMode = value;
                pictureBox1.Image = imgView;
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

        protected Bitmap[] Img = new Bitmap[(int)imgMode.Size];//image view-> 0:original , 1:negative , 2:gray , 3:R , 4:G , 5:B
        protected Bitmap imgView
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
        protected Series seriesT;
        protected Series seriesR;
        protected Series seriesG;
        protected Series seriesB;
        protected string colorLabel
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

        public OperationForm()
        {
            InitializeComponent();
            //openFileDialog Setting
            openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "PCX file(*.pcx)|*.pcx";
            openFileDialog1.Title = "Select a PCX file";
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;
            
            //set mode tag in menu item
            originalToolStripMenuItem.Tag = imgMode.ORI;
            negativeToolStripMenuItem.Tag = imgMode.NEG;
            grayToolStripMenuItem.Tag = imgMode.GRAY;
            redToolStripMenuItem.Tag = imgMode.R;
            greenToolStripMenuItem.Tag = imgMode.G;
            blueToolStripMenuItem.Tag = imgMode.B;

            //lock some not use
            foreach (ToolStripMenuItem item in modeToolStripMenuItem.DropDownItems)// lock image mode 
            {
                item.Enabled = false;
            }
        }

        protected string textFromLines(string[] lines)
        {
            string text = "";
            foreach (string oneLine in lines)
            {
                text += oneLine + "\r\n";
            }
            return text;
        }

        protected string getColorLabel(Color get)
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


        protected void openPCX(string filePath)
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
            imgView = thePCX.getView();
            mode = (int)imgMode.ORI;
            MessageBox.Show(filePath, "開啟成功");
            foreach (ToolStripMenuItem item in modeToolStripMenuItem.DropDownItems)// unlock image mode 
            {
                item.Enabled = true;
            }
        }

        protected void buildChart(Chart chart1)
        {
            try
            {
                chart1.Series.Clear();
                if (imgView == null)
                    return;
                switch (mode)
                {
                    case (int)imgMode.GRAY:
                        seriesT = MyDeal.buildSeries(imgView, (int)MyDeal.colorMode.GRAY);
                        chart1.Series.Add(seriesT);
                        return;
                    case (int)imgMode.R:
                        seriesT = MyDeal.buildSeries(imgView, (int)MyDeal.colorMode.R);
                        chart1.Series.Add(seriesT);
                        return;
                    case (int)imgMode.G:
                        seriesT = MyDeal.buildSeries(imgView, (int)MyDeal.colorMode.G);
                        chart1.Series.Add(seriesT);
                        return;
                    case (int)imgMode.B:
                        seriesT = MyDeal.buildSeries(imgView, (int)MyDeal.colorMode.B);
                        chart1.Series.Add(seriesT);
                        return;
                    case (int)imgMode.ORI:
                    case (int)imgMode.NEG:
                    default:
                        seriesR = MyDeal.buildSeries(imgView, (int)MyDeal.colorMode.R);
                        chart1.Series.Add(seriesR);

                        seriesG = MyDeal.buildSeries(imgView, (int)MyDeal.colorMode.G);
                        chart1.Series.Add(seriesG);

                        seriesB = MyDeal.buildSeries(imgView, (int)MyDeal.colorMode.B);
                        chart1.Series.Add(seriesB);
                        return;
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
            }
        }

        protected void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                openPCX(openFileDialog1.FileName);
                mode = 0;
            }
        }

        protected void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            PictureBox here = (PictureBox)sender;
            int X, Y;   //location in image
            int boxW = here.Width;
            int boxH = here.Height;
            int imgW, imgH;
            if (here.Image == null)
            {
                imgW = boxW;
                imgH = boxH;
            }
            else
            {
                imgW = here.Image.Width;
                imgH = here.Image.Height;
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

        protected void pictureBox_MouseLeave(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "( X , Y )";
            toolStripStatusLabel2.Text = colorLabel;
        }

        protected void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem here = (ToolStripMenuItem)sender;
            mode = (int)here.Tag;
        }

        protected void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileToolStripMenuItem.Enabled = true;
            clearToolStripMenuItem.Enabled = false;
            imgView = null;
            mode = (int)imgMode.ORI;
            foreach (ToolStripMenuItem item in modeToolStripMenuItem.DropDownItems)// lock image mode 
            {
                item.Enabled = false;
            }
        }
    }
}
