using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
        protected MyCompressTiff myCompressTiff;
        protected MyTiff myTiffB = new MyTiff();
        protected MyPlayer playerB = null;
        protected MyCompressTiff myCompressTiffB;
        protected string seriesIdA = "Series1";
        protected string seriesIdB = "Series2";
        protected delegate void playCombine(MyPlayer src, Control dst, params object[] args);
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
            openFileDialog1.Filter = "tif file(*.tiff)|*.01.tiff;*01.*.tiff|my compress file(*.MYCT)|*.MYCT";
            openFileDialog1.Title = "Select a video file";
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;

            //initial PSNR Chart
            chart1.ChartAreas.ElementAt(0).AxisX.Minimum = 0;
            chart1.ChartAreas.ElementAt(0).AxisX.Maximum = 0;
            chart1.ChartAreas.ElementAt(0).AxisX.Interval = 5;//橫坐標 標籤距
            chart1.ChartAreas.ElementAt(0).AxisY.Minimum = 0;
            chart1.ChartAreas.ElementAt(0).AxisY.Maximum = 100;

            //add psnr
            chart1.Series.Clear();
            Series PSNRSeries = addSeries(seriesIdA, "A PSNR(dB)", Color.Blue);
            chart1.Series.Add(PSNRSeries);

            player = new MyPlayer(pictureBox1);
            player.SideWorkDo.Add(new playCombine(countPSNR));
            player.SideWorkArgs.Add(new object[] { player, chart1, new object[] { seriesIdA } });
        }

        protected Series addSeries(string id, string test, Color color)
        {
            Series PSNRSeries = new Series(id);
            PSNRSeries.LegendText = test;
            PSNRSeries.ChartType = SeriesChartType.Line;
            PSNRSeries.Color = color;
            return PSNRSeries;
        }

        protected virtual void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                switch (openFileDialog1.FilterIndex)
                {
                    case 1:
                        if (!openFileDialog1.FileName.Contains(".tiff"))
                        {
                            MessageBox.Show(openFileDialog1.FileName, "open fail < not a tiff file >");
                            return;
                        }
                        myTiff.from(openFileDialog1.FileName);
                        myCompressTiff = null;
                        break;
                    case 2:
                        if (!openFileDialog1.FileName.Contains(".MYCT"))
                        {
                            MessageBox.Show(openFileDialog1.FileName, "open fail < not a MYCT file >");
                            return;
                        }
                        FileStream fs = (FileStream)openFileDialog1.OpenFile();
                        myCompressTiff = MyCompressTiff.readFromFile(fs);
                        myTiff = MyCompressTiff.decode(myCompressTiff);
                        fs.Close();
                        break;
                }
                
                player.open(myTiff);
                player.connect(trackBar1);
                player.OnPlay(new MyPlayer.PlayEventArgs(0, MyPlayer.PlayState.STOP));
                textBox_progress.Text = "0 / " + (myTiff.Size - 1);

                chart1.ChartAreas.ElementAt(0).AxisX.Maximum = myTiff.Size - 1;//set x axis maximun
                if (chart1.Series.FindByName(seriesIdA) != null)
                {
                    chart1.Series.FindByName(seriesIdA).Points.Clear();
                    chart1.Series.FindByName(seriesIdA).Points.DataBindY(new double[myTiff.Size]);

                }
            }
        }

        private void openBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                switch (openFileDialog1.FilterIndex)
                {
                    case 1:
                        if (!openFileDialog1.FileName.Contains(".tiff"))
                        {
                            MessageBox.Show(openFileDialog1.FileName, "open fail < not a tiff file >");
                            return;
                        }
                        myTiffB.from(openFileDialog1.FileName);
                        myCompressTiffB = null;
                        break;
                    case 2:
                        if (!openFileDialog1.FileName.Contains(".MYCT"))
                        {
                            MessageBox.Show(openFileDialog1.FileName, "open fail < not a MYCT file >");
                            return;
                        }
                        FileStream fs = (FileStream)openFileDialog1.OpenFile();
                        myCompressTiffB = MyCompressTiff.readFromFile(fs);
                        myTiffB = MyCompressTiff.decode(myCompressTiffB);
                        fs.Close();
                        break;
                }

                playerB.open(myTiffB);
                playerB.OnPlay(new MyPlayer.PlayEventArgs(0, MyPlayer.PlayState.STOP));

                if (chart1.Series.FindByName(seriesIdB) != null)
                {
                    chart1.Series.FindByName(seriesIdB).Points.Clear();
                    string[] xValue = Enumerable.Range(0, myTiffB.Size).ToArray().Select(x => x.ToString()).ToArray();
                    chart1.Series.FindByName(seriesIdB).Points.DataBindXY(xValue, new double[myTiffB.Size]);
                }
            }
        }

        protected void button1_Click(object sender, EventArgs e)
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

        protected void button2_Click(object sender, EventArgs e)
        {
            player.OnStop();
            player.OnPlay(new MyPlayer.PlayEventArgs(0, MyPlayer.PlayState.KEEP));
            //button1.Enabled = true;
            //button3.Enabled = true;
        }

        protected void button3_Click(object sender, EventArgs e)
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

        protected void trackBar_Scroll(object sender, EventArgs e)
        {
            TrackBar here = (TrackBar)sender;
            player.OnPlay(new MyPlayer.PlayEventArgs(here.Value, MyPlayer.PlayState.KEEP));
        }

        protected void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            TrackBar here = (TrackBar)sender;
            textBox_progress.Text = here.Value + " / " + (myTiff.Size - 1);
        }

        protected void countPSNR(MyPlayer src, Control dst, object[] args)
        {//count PSNR by playing in src player last view , args[0] is (string)seriesId
            Chart here = (Chart)dst;
            //here.Invoke(new chartDelegate(countPSNR_chart), (Chart)dst, new object[] { src, args[0] });
            here.BeginInvoke(new chartDelegate(countPSNR_chart), (Chart)dst, new object[] { src, args[0] });
        }

        private void countPSNR_chart(Chart here, object[] args)
        {//chartDelegate for count psnr each view args = { player , seriesName }
            string seriesName = (string)args.ElementAtOrDefault(1);
            Debug.Print("in countPSNR_chart " + seriesName);
            Series target = here.Series.FindByName(seriesName);
            //Series target = here.Series.ElementAt(0);
            MyPlayer src = (MyPlayer)args.ElementAtOrDefault(0);
            if (src == null)
                return;
            if (target == null)
                return;
            Debug.Print(" " + src.Tiff.Current);
            try
            {
                if (src.Tiff.Current >= target.Points.Count)
                    return;
                else if (src.Tiff.Current < 0)
                    return;
                //target.Points.RemoveAt(src.Tiff.Current);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.Print(e.StackTrace);
            }
            //target.Points.InsertXY(src.Tiff.Current, src.Tiff.Current, MyDeal.PSNR(src.Tiff.BackView, src.Tiff.View).ToString());
            target.Points.ElementAt(src.Tiff.Current).XValue = src.Tiff.Current;
            target.Points.ElementAt(src.Tiff.Current).SetValueY(MyDeal.PSNR(src.Tiff.BackView, src.Tiff.View));
            foreach (DataPoint data in target.Points)
            {
                Debug.Print("check " + data.XValue + " , " + data.YValues[0]);
            }
        }

        private void normalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_NormalPlayer form2 = new Form_NormalPlayer();
            this.Visible = false;
            form2.ShowDialog();
            this.Close();
        }

        private void compareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form_ComparePlayer form2 = new Form_ComparePlayer();
            this.Visible = false;
            form2.ShowDialog();
            this.Close();
        }
    }
}
