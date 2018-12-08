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
using System.Windows.Forms.DataVisualization.Charting;

namespace HW2_video
{
    public partial class BasicForm : Form
    {

        protected MyTiff myTiff = new MyTiff();
        protected MyPlayer player = null;
        protected delegate void playCombine(MyPlayer src, Control dst, object[] args);
        protected delegate void chartDelegate(Chart use,object[] args);//Delegate for chart invoke
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

            //initial PSNR Chart
            Series PSNRSeries = new Series("Series1");
            PSNRSeries.ChartType = SeriesChartType.Column;
            chart1.Series.Add(PSNRSeries);

            player = new MyPlayer(pictureBox1);
            player.SideWorkDo.Add(new playCombine(countPSNR));
            player.SideWorkArgs.Add(new object[] { player, chart1, null });
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

                chart1.Series.ElementAt(0).Points.Clear();
                chart1.ChartAreas.ElementAt(0).AxisX.Maximum = myTiff.Size - 1;
                chart1.Series.ElementAt(0).Points.DataBindY(new double[myTiff.Size]);

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
            player.OnStop();
            player.OnPlay(new MyPlayer.PlayEventArgs(0, MyPlayer.PlayState.KEEP));
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
            player.OnPlay(new MyPlayer.PlayEventArgs(here.Value, MyPlayer.PlayState.KEEP));
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            TrackBar here = (TrackBar)sender;
            textBox_progress.Text = here.Value + " / " + (myTiff.Size - 1);
        }

        private void countPSNR(MyPlayer src, Control dst, object[] args)
        {//count PSNR by playing in src player last view , args is not use
            Chart here = (Chart)dst;
            here.Invoke(new chartDelegate(countPSNR_chart), (Chart)dst, new object[] { src });
        }

        private void countPSNR_chart(Chart here, object[] args)
        {//chartDelegate for count psnr each view args = { player }
            Series target = here.Series.FindByName("Series1");
            MyPlayer src = (MyPlayer)args.ElementAtOrDefault(0);
            if (target == null)
                here.Series.Add(target = new Series("Series1"));
            if (src != null)
            {
                Debug.Print(" " + src.Tiff.Current);
                try
                {
                    target.Points.RemoveAt(src.Tiff.Current);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Debug.Print(e.StackTrace);
                }
                target.Points.InsertXY(src.Tiff.Current, src.Tiff.Current, MyDeal.PSNR(src.Tiff.BackView, src.Tiff.View).ToString());
                
                foreach(DataPoint data in target.Points)
                {
                    Debug.Print("check " + data.XValue + " , " + data.YValues[0]);
                }
            }
        }
    }
}
