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
        MyCompresser.COMPRESS_METHOD compressMethod = MyCompresser.COMPRESS_METHOD.LOCAL;
        MyFilterData.CRITERIA_METHOD criteria = MyFilterData.CRITERIA_METHOD.SQUARE;
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
                if (compresser != null)
                    compresser.shutDown();
                compresser = new MyCompresser(player, pictureBox2);
                compresser.setCompressMethod(compressMethod);
                compresser.setCriteriaMethod(criteria);
                compresser.connect(pictureBoxFeature, MyCompresser.CONNECT.PICTURE_FEATURE);
                compresser.connect(pictureBoxFeatureRef, MyCompresser.CONNECT.PICTURE_MATCH);
                compresser.connect(pictureBoxMotion, MyCompresser.CONNECT.PICTURE_MOTION);
                compresser.connect(trackBar1);
                compresser.connect(this, saveFileDialog1);
                if (checkBox1.Checked)
                {
                    MyCompresser.sleepTime = MyCompresser.sleepLong; ;
                    if (compresser != null)
                    {
                        compresser.ReferencePlayer.flashIgnore = false;
                    }
                    pictureBoxFeatureRef.Visible = true;
                }
                if (checkBox2.Checked)
                {
                    pictureBoxMotion.Visible = true;
                    pictureBox2.Visible = false;
                }
                compresser.CurrentPlayer.flashIgnore = false;
                button1.Enabled = true;
                groupBox3.Enabled = true;
                textBox_progress.Text = "0 / " + (myTiff.Size - 1);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(compresser != null)
            {
                button1.Enabled = false;
                groupBox3.Enabled = false;
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
            groupBox3.Enabled = true;
            compresser = null;
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
                pictureBoxFeatureRef.Visible = true;
            }
            else
            {
                MyCompresser.sleepTime = MyCompresser.sleepShort;
                if (compresser != null)
                {
                    compresser.ReferencePlayer.flashIgnore = true;
                    compresser.ReferencePlayer.OnPlay(new MyPlayer.PlayEventArgs(-1));
                }
                pictureBoxFeatureRef.Visible = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                pictureBoxMotion.Visible = true;
                pictureBox2.Visible = false;
            }
            else
            {
                pictureBoxMotion.Visible = false;
                pictureBox2.Visible = true;
            }
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton here = (RadioButton)sender;
            if (here.Checked)
            {
                switch (here.Name)
                {
                    case "radioButton1":
                        compressMethod = MyCompresser.COMPRESS_METHOD.ALL;
                        break;
                    case "radioButton2":
                        compressMethod = MyCompresser.COMPRESS_METHOD.LOCAL;
                        break;
                    case "radioButton3":
                        compressMethod = MyCompresser.COMPRESS_METHOD.TSS;
                        break;
                }
                if (compresser != null)
                {
                    compresser.setCompressMethod(compressMethod);
                }
            }
        }

        private void radioButtonCriteria_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton here = (RadioButton)sender;
            if (here.Checked)
            {
                switch (here.Name)
                {
                    case "radioButtonSQ":
                        criteria = MyFilterData.CRITERIA_METHOD.SQUARE;
                        break;
                    case "radioButtonAB":
                        criteria = MyFilterData.CRITERIA_METHOD.ABSOLUTE;
                        break;
                }
                if (compresser != null)
                {
                    compresser.setCriteriaMethod(criteria);
                }

            }
        }
    }

}
