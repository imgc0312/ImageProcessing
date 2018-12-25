using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace HW1_PCXreader
{
    public partial class Form_FractalFile : HW1_PCXreader.ToolForm
    {
        Fractal f = null;
        Thread encodeThread;
        string SNRText = "SNR (dB) : ";
        Bitmap inputCur = null;
        Bitmap InputCur
        {
            get
            {
                return inputCur;
            }
            set
            {
                inputCur = value;
                if (value == null)
                    pictureBox1.Image = null;
                else
                {
                    pictureBox1.Image = new Bitmap(value.Width, value.Height);
                }
                pictureBox1.BackgroundImage = inputCur;
                
            }
        }

        Bitmap inputRef = null;
        Bitmap InputRef
        {
            get
            {
                return inputRef;
            }
            set
            {
                inputRef = value;
                pictureBox2.BackgroundImage = inputRef;
            }
        }

        ProgressMonitor monitor = null;

        /// <summary>
        /// function-->
        /// </summary>

        public Form_FractalFile() : base()
        {
            InitializeComponent();
            initial();
        }

        public Form_FractalFile(Form1 form1) : base(form1)
        {
            InitializeComponent();
            InputCur = MyDeal.gray(form1.Img[0]);
            initial();
        }

        private void initial()
        {
            mode = (int)imgMode.GRAY;
            toolStripStatusLabel3.Visible = true;
            toolStripStatusLabel3.Text = SNRText;

            //saveFileDialog Setting
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "My fractal file | *.MYFF";
            saveFileDialog1.Title = "Save an compress File";

            //openFileDialog Setting
            openFileDialog2 = new OpenFileDialog();
            openFileDialog2.Filter = "My Fractal file(*.MYFF)|*.MYFF";
            openFileDialog2.Title = "Select a My Fractal file";
            openFileDialog2.CheckFileExists = true;
            openFileDialog2.CheckPathExists = true;

            monitor = new ProgressMonitor(progressBar1);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                InputCur = openPCX(openFileDialog1.FileName, null, pictureBox1, MyDeal.gray);
                mode = (int)imgMode.GRAY;

            }
            else
            {
                InputCur = null;
            }
            toolStripStatusLabel3.Text = SNRText + MyDeal.SNR(InputCur, InputRef);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                InputRef = openPCX(openFileDialog1.FileName, null, pictureBox2, MyDeal.gray);
                mode = (int)imgMode.GRAY;
            }
            else
            {
                InputRef = null;
            }
            toolStripStatusLabel3.Text = SNRText + MyDeal.SNR(InputCur, InputRef);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(InputCur == null)
            {
                MessageBox.Show("No Current Image to Encode !!");
            }
            else
            {
                f = new Fractal();
                f.connect(this, saveFileDialog1);
                f.connect(monitor);
                f.connect(pictureBox1);
                encodeThread = new Thread(new ThreadStart(new Action(() =>
                {
                    f.encode((Bitmap)InputCur.Clone());
                })));
                encodeThread.IsBackground = true;
                encodeThread.Start();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if(openFileDialog2.ShowDialog()  == DialogResult.OK)
            {
                f = new Fractal();
                FileStream fs = (FileStream)openFileDialog2.OpenFile();
                f.import(FractalFile.readFromFile(fs));
                fs.Close();
                MessageBox.Show("Import successed!!");
                return;
            }
            MessageBox.Show("Import failed!!");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (f != null)
            {
                f.connect(monitor);
                InputCur = f.decode(InputCur);
                toolStripStatusLabel3.Text = SNRText + MyDeal.SNR(InputCur, InputRef);
            }
            else
            {
                MessageBox.Show("No Import Fractal to Decode !!");
            }
        }

        protected override void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            PictureBox here = (PictureBox)sender;
            int X, Y;   //location in image
            int boxW = here.Width;
            int boxH = here.Height;
            int imgW, imgH;
            if (here.BackgroundImage == null)
            {
                imgW = boxW;
                imgH = boxH;
            }
            else
            {
                imgW = here.BackgroundImage.Width;
                imgH = here.BackgroundImage.Height;
            }
            X = e.X + (imgW - boxW) / 2;
            Y = e.Y + (imgH - boxH) / 2;
            try
            {
                if (X >= 0 && X < imgW && Y >= 0 && Y < imgH)
                {
                    toolStripStatusLabel1.Text = "( " + X + " , " + Y + " )";
                    Bitmap view = (Bitmap)here.BackgroundImage;
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

        protected override void pictureBox_DoubleClick(object sender, EventArgs e)
        {
            PictureBox here = (PictureBox)sender;
            Form_ImageView form2 = new Form_ImageView(here.BackgroundImage, mode);
            form2.ShowDialog();
        }

        private void Form_FractalFile_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(encodeThread != null)
            {
                if (encodeThread.IsAlive)
                {
                    encodeThread.Abort();
                }
            }
        }
    }
}
