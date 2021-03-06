﻿using System;
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
                Debug.Print("compress mode : " + compressMethod.ToString());
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
                compresser.BB_THRESHOLD = trackBar2.Value;
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
                        groupBox4.Visible = true;
                        groupBox5.Visible = false;
                        break;
                    case "radioButton2":
                        compressMethod = MyCompresser.COMPRESS_METHOD.LOCAL;
                        groupBox4.Visible = true;
                        groupBox5.Visible = false;
                        break;
                    case "radioButton3":
                        compressMethod = MyCompresser.COMPRESS_METHOD.TSS;
                        groupBox4.Visible = true;
                        groupBox5.Visible = false;
                        break;
                    case "radioButton4":
                        compressMethod = MyCompresser.COMPRESS_METHOD.INTER_SUB;
                        groupBox4.Visible = false;
                        groupBox5.Visible = false;
                        break;
                    case "radioButton5":
                        compressMethod = MyCompresser.COMPRESS_METHOD.INTRA_SUB;
                        groupBox4.Visible = false;
                        groupBox5.Visible = false;
                        break;
                    case "radioButton6":
                        compressMethod = MyCompresser.COMPRESS_METHOD.BLOCKBASE;
                        groupBox4.Visible = true;
                        groupBox5.Visible = true;
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

        private void CompressForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (compresser != null)
            {
                compresser.shutDown();
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                if (compresser != null)
                {
                    compresser.CurrentPlayer.flashIgnore = true;
                    compresser.CurrentPlayer.OnPlay(new MyPlayer.PlayEventArgs(-1));
                }
                pictureBoxFeature.Visible = false;
            }
            else
            {
                if (compresser != null)
                {
                    compresser.CurrentPlayer.flashIgnore = false;
                }
                pictureBoxFeature.Visible = true;
            }
        }

        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            if (compresser != null)
            {
                compresser.BB_THRESHOLD = trackBar2.Value;
            }
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            textBox1.Text = trackBar2.Value.ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int value = 0;
            if (!Int32.TryParse(textBox1.Text, out value))
                value = Int32.MinValue;//no value
            if ((value < trackBar2.Minimum) || (value > trackBar2.Maximum))
            {// illegal number
                MessageBox.Show("Please input an integer number(" + trackBar2.Minimum + "~" + trackBar2.Maximum + ")");
                textBox1.Text = trackBar2.Value.ToString();
            }
            else
            {
                trackBar2.Value = value;
            }
        }
    }

}
