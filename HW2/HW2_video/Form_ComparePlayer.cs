using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace HW2_video
{
    public partial class Form_ComparePlayer : HW2_video.BasicForm
    {
        string seriesIdAB = "Series3";
        public Form_ComparePlayer() : base()
        {
            InitializeComponent();
        }

        protected override void initialForm()
        {
            base.initialForm();
            compareToolStripMenuItem.Enabled = false;
            openBToolStripMenuItem.Enabled = true;

            playerB = new MyPlayer(pictureBox2);
            player.SideWorkDo.Add(new playCombine(sideView));
            player.SideWorkArgs.Add(new object[] { player, null, new object[] { playerB } });
            groupBox3.Text = "not use";

            //initial B PSNR Chart
            Series PSNRSeries = addSeries(seriesIdB, "B PSNR(dB)", Color.Red);
            chart1.Series.Add(PSNRSeries);

            playerB.SideWorkDo.Add(new playCombine(countPSNR));
            playerB.SideWorkArgs.Add(new object[] { playerB, chart1, new object[] { seriesIdB } });

            //initial A-B PSNR Chart
            Series PSNRSeries2 = addSeries(seriesIdAB, "A-B PSNR(dB)", Color.Gold);
            chart1.Series.Add(PSNRSeries2);

            playerB.SideWorkDo.Add(new playCombine(countPSNR2));
            playerB.SideWorkArgs.Add(new object[] { playerB, chart1, new object[] { player, seriesIdAB } });
        }

        protected void sideView(MyPlayer src, Control dst, params object[] args)
        {//(MyPlayer)args[0] play with src, dst is not use
            MyPlayer here = (MyPlayer)args[0];
            if (here == null)
                return;
            here.OnPlay(new MyPlayer.PlayEventArgs(src.Tiff.Current));
        }

        protected void countPSNR2(MyPlayer src, Control dst, object[] args)
        {//count PSNR by playing in src player & (MyPlayer)args[0] in (string)args[1]<seriesId>
            Chart here = (Chart)dst;
            //here.Invoke(new chartDelegate(countPSNR_chart), (Chart)dst, new object[] { src, args[0] });
            here.BeginInvoke(new chartDelegate(countPSNR2_chart), (Chart)dst, new object[] { src, args[0], args[1] });
        }

        private void countPSNR2_chart(Chart here, object[] args)
        {//chartDelegate for count psnr each view args = { player, player , seriesName }
            string seriesName = (string)args.ElementAtOrDefault(2);
            Debug.Print("in countPSNR_chart " + seriesName);
            Series target = here.Series.FindByName(seriesName);
            //Series target = here.Series.ElementAt(0);
            MyPlayer A = (MyPlayer)args.ElementAtOrDefault(0);
            MyPlayer B = (MyPlayer)args.ElementAtOrDefault(1);
            if (A == null)
                return;
            if (B == null)
                return;
            if (target == null)
                return;
            Debug.Print(" " + A.Tiff.Current);
            try
            {
                if (A.Tiff.Current >= target.Points.Count)
                    return;
                else if (A.Tiff.Current < 0)
                    return;
                //target.Points.RemoveAt(src.Tiff.Current);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.Print(e.StackTrace);
            }
            //target.Points.InsertXY(src.Tiff.Current, src.Tiff.Current, MyDeal.PSNR(src.Tiff.BackView, src.Tiff.View).ToString());
            target.Points.ElementAt(A.Tiff.Current).XValue = A.Tiff.Current;
            target.Points.ElementAt(A.Tiff.Current).SetValueY(MyDeal.PSNR(A.Tiff.View, B.Tiff.View));
            foreach (DataPoint data in target.Points)
            {
                Debug.Print("check in countPSNR2_chart in " + A.Tiff.Current + " , " + B.Tiff.Current + " :" + data.XValue + " , " + data.YValues[0]);
            }
        }

        protected override void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            base.openToolStripMenuItem_Click(sender, e);
            if ((chart1.Series.FindByName(seriesIdAB) != null) && (myTiff!= null))
            {
                chart1.Series.FindByName(seriesIdAB).Points.Clear();
                int[] xValue = Enumerable.Range(0, myTiff.Size).ToArray();
                double[] yValue = new double[myTiff.Size];
                chart1.Series.FindByName(seriesIdAB).Points.DataBindXY(xValue, yValue);
            }
            this.Enabled = true;
        }
    }
}
