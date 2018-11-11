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
            "Xmin , Xmax, Ymin, Ymax \t:\t",
            "Hdpi , Vdpi \t\t:\t",
            "nPlanes \t\t\t:\t",
            "bytesPerLine \t\t:\t",
            "paletteInfo \t\t:\t",
            "H Screen Size , V Screen Size :\t"
        };
        public enum imgMode : int { ORI, NEG, GRAY, R, G, B, Size };// 0:original , 1:negative , 2:gray , 3:R , 4:G , 5:B
        public int selMode = (int)imgMode.ORI;   // 0:original , 1:negative , 2:gray , 3:R , 4:G , 5:B
        public int chUse = 3;// channel use amount
        public virtual int mode
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
                        chUse = 3;
                        break;
                    case (int)imgMode.NEG:
                        toolStripStatusLabel0.Text = "Negative";
                        chUse = 3;
                        break;
                    case (int)imgMode.GRAY:
                        toolStripStatusLabel0.Text = "Gray";
                        chUse = 1;
                        break;
                    case (int)imgMode.R:
                        toolStripStatusLabel0.Text = "Red";
                        chUse = 1;
                        break;
                    case (int)imgMode.G:
                        toolStripStatusLabel0.Text = "Green";
                        chUse = 1;
                        break;
                    case (int)imgMode.B:
                        toolStripStatusLabel0.Text = "Blue";
                        chUse = 1;
                        break;
                    default:
                        toolStripStatusLabel0.Text = "Unknown";
                        chUse = 1;
                        break;
                }
            }
        }

        protected Bitmap[] Img = new Bitmap[(int)imgMode.Size];//image view-> 0:original , 1:negative , 2:gray , 3:R , 4:G , 5:B
        protected Bitmap OutImg = null;//image view-> 0:original , 1:negative , 2:gray , 3:R , 4:G , 5:B
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
        public Bitmap outView
        {
            get
            {
                return OutImg;
            }
            set
            {
                OutImg = value;
                pictureBox2.Image = OutImg;
            }
        }
        protected Series seriesT;
        protected Series seriesR;
        protected Series seriesG;
        protected Series seriesB;
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
        private bool dataClear = true;//data is clear
        protected virtual bool openEnable
        {
            get
            {
                return dataClear;
            }
            set
            {
                dataClear = value;
                openFileToolStripMenuItem.Enabled = value;
                clearToolStripMenuItem.Enabled = !value;
            }
        }

        /// <summary>
        /// function-->
        /// </summary>

        public OperationForm()
        {
            InitializeComponent();
            initialForm();
            openEnable = true;
        }

        public OperationForm(Form1 form1)
        {
            InitializeComponent();
            initialForm();

            for (int i = 0; i < form1.Img.Length; i++)
            {
                if (form1.Img[i] == null)
                    Img[i] = null;
                else
                    Img[i] = (Bitmap)form1.Img[i].Clone();
            }
            mode = form1.mode;
            if(imgView != null)
            {
                foreach (ToolStripMenuItem item in modeToolStripMenuItem.DropDownItems)// unlock image mode 
                {
                    item.Enabled = true;
                }
                openEnable = false;
            }
        }

        public OperationForm(OperationForm form)// if this inherit old operated image
        {
            InitializeComponent();
            initialForm();
            mode = form.mode;
            Img[mode] = form.outView;
            pictureBox1.Image = imgView;
            if (imgView != null)
            {
                foreach (ToolStripMenuItem item in modeToolStripMenuItem.DropDownItems)// unlock image mode 
                {
                    item.Enabled = false; // if this inherit old operated image
                }
                openEnable = false;
            }
        }

        protected void initialForm()
        {
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
            openEnable = true;
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
            openEnable = false;
        }

        protected void buildChart(Chart chart1)
        {
            try
            {
                chart1.Series.Clear();
                if (outView == null)
                    return;
                switch (mode)
                {
                    case (int)imgMode.GRAY:
                        seriesT = MyDeal.buildSeries(outView, MyDeal.colorMode.GRAY);
                        chart1.Series.Add(seriesT);
                        return;
                    case (int)imgMode.R:
                        seriesT = MyDeal.buildSeries(outView, MyDeal.colorMode.R);
                        chart1.Series.Add(seriesT);
                        return;
                    case (int)imgMode.G:
                        seriesT = MyDeal.buildSeries(outView, MyDeal.colorMode.G);
                        chart1.Series.Add(seriesT);
                        return;
                    case (int)imgMode.B:
                        seriesT = MyDeal.buildSeries(outView, MyDeal.colorMode.B);
                        chart1.Series.Add(seriesT);
                        return;
                    case (int)imgMode.ORI:
                    case (int)imgMode.NEG:
                    default:
                        seriesR = MyDeal.buildSeries(outView, MyDeal.colorMode.R);
                        chart1.Series.Add(seriesR);

                        seriesG = MyDeal.buildSeries(outView, MyDeal.colorMode.G);
                        chart1.Series.Add(seriesG);

                        seriesB = MyDeal.buildSeries(outView, MyDeal.colorMode.B);
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
                    Bitmap view = (Bitmap)here.Image;
                    if (view != null)
                    {
                        Color color = view.GetPixel(X, Y);
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

        protected virtual void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileToolStripMenuItem.Enabled = true;
            clearToolStripMenuItem.Enabled = false;
            imgView = null;
            mode = (int)imgMode.ORI;
            foreach (ToolStripMenuItem item in modeToolStripMenuItem.DropDownItems)// lock image mode 
            {
                item.Enabled = false;
            }
            openEnable = true;
        }

        protected virtual void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if(outView == null)
                outView = imgView;
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

        private void pictureBox_DoubleClick(object sender, EventArgs e)
        {
            PictureBox here = (PictureBox)sender;
            if(here.Image != null)
            {
                Form_ImageView form2 = new Form_ImageView(here.Image, mode);
                form2.ShowDialog();
            }
        }

        private void OperationForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }
    }

}
