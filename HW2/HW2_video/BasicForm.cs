using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HW2_video
{
    public partial class BasicForm : Form
    {

        protected MyTiff myTiff = new MyTiff();
        protected MyPlayer player = null;
        protected MyPlayer player2 = null;
        private delegate void playCombine(MyPlayer src, MyPlayer dst);
        /// <summary>
        /// function-->
        /// </summary>
        /// 
        public BasicForm()
        {
            InitializeComponent();
            initialForm();
        }

        protected virtual void initialForm()
        {
            //openFileDialog Setting
            openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "tif file(*.tiff)|*.01.tiff;*01.*.tiff";
            openFileDialog1.Title = "Select a video file";
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;

            player = new MyPlayer(pictureBox1);
            player2 = new MyPlayer(pictureBox2);
            player.SideWorkDo = new playCombine(lastView);
            player.SideWorkArgs = new object[] { player, player2 };
        }

        private void lastView(MyPlayer src, MyPlayer dst)
        {
            dst.OnPlay(src.LastView, new MyPlayer.PlayEventArgs(0, MyPlayer.PlayState.STOP));
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                myTiff.from(openFileDialog1.FileName);
                player.open(myTiff);
                player.connect(trackBar1);
                player.OnPlay(new MyPlayer.PlayEventArgs(0, MyPlayer.PlayState.STOP));
                textBox_progress.Text = "0 / " + (myTiff.Size - 1);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(player.State != MyPlayer.PlayState.PLAY)
            {
                player.OnPlay();
                //button3.Enabled = false;
            }
            else
            {
                player.OnStop();
                //button3.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            player.OnPlay(new MyPlayer.PlayEventArgs(0, MyPlayer.PlayState.STOP));
            //button1.Enabled = true;
            //button3.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (player.State != MyPlayer.PlayState.BACK)
            {
                player.OnBack();
                //button1.Enabled = false;
            }
            else
            {
                player.OnStop();
                //button1.Enabled = true;
            }
        }

        private void trackBar_Scroll(object sender, EventArgs e)
        {
            TrackBar here = (TrackBar)sender;
            player.OnPlay(new MyPlayer.PlayEventArgs(here.Value, MyPlayer.PlayState.STOP));
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            TrackBar here = (TrackBar)sender;
            textBox_progress.Text = here.Value + " / " + (myTiff.Size - 1);
        }
    }
}
