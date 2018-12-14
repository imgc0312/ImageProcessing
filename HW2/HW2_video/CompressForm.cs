using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HW2_video
{
    public partial class CompressForm : Form
    {
        protected MyTiff myTiff = new MyTiff();
        protected MyPlayer player = null;
        protected delegate void playCombine(MyPlayer src, Control dst, object[] args);
        MyCompresser compresser = null;
        /// <summary>
        /// function-->
        /// </summary>
        public CompressForm()
        {
            InitializeComponent();
            initialForm();
        }

        protected virtual void initialForm()
        {
            MyCompresser.sleepTime = MyCompresser.sleepShort;

            //openFileDialog Setting
            openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "tif file(*.tiff)|*.01.tiff;*01.*.tiff";
            openFileDialog1.Title = "Select a video file";
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;

            //saveFileDialog Setting
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "My compress tiff | *.MYCT";
            saveFileDialog1.Title = "Save an compress File";

            player = new MyPlayer(pictureBox1);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                myTiff.from(openFileDialog1.FileName);
                player.open(myTiff);
                player.OnPlay(new MyPlayer.PlayEventArgs(0, MyPlayer.PlayState.STOP));

                compresser = new MyCompresser(player, pictureBox2);
                compresser.setMatchMethod(compresser.findMatchNormal);
                compresser.connect(pictureBoxFeature);
                compresser.connect(trackBar1);
                compresser.connect(this, saveFileDialog1);
                if (checkBox1.Checked)
                {
                    MyCompresser.sleepTime = MyCompresser.sleepTime = MyCompresser.sleepLong; ;
                    if (compresser != null)
                    {
                        compresser.ReferencePlayer.flashIgnore = false;
                    }
                }
                compresser.CurrentPlayer.flashIgnore = false;
                textBox_progress.Text = "0 / " + (myTiff.Size - 1);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(compresser != null)
            {
                button1.Enabled = false;
                compresser.Compressing();
            }
        }

        private void trackBar_Scroll(object sender, EventArgs e)
        {
            TrackBar here = (TrackBar)sender;
            player.OnPlay(new MyPlayer.PlayEventArgs(here.Value, MyPlayer.PlayState.KEEP));
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            TrackBar here = (TrackBar)sender;
            textBox_progress.Text = here.Value + " / " + (myTiff.Size - 1);
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            button1.Enabled = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                MyCompresser.sleepTime = MyCompresser.sleepLong;
                if(compresser != null)
                {
                    compresser.ReferencePlayer.flashIgnore = false;
                }
            }
            else
            {
                MyCompresser.sleepTime = MyCompresser.sleepShort;
                if (compresser != null)
                {
                    compresser.ReferencePlayer.flashIgnore = true;
                    compresser.ReferencePlayer.OnPlay(new MyPlayer.PlayEventArgs(-1));
                }
            }
        }
    }

}
