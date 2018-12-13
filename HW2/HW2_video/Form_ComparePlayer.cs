using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace HW2_video
{
    public partial class Form_ComparePlayer : HW2_video.BasicForm
    {
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
        }

        protected void sideView(MyPlayer src, Control dst, params object[] args)
        {//(MyPlayer)args[0] play with src, dst is not use
            MyPlayer here = (MyPlayer)args[0];
            if (here == null)
                return;
            here.OnPlay(new MyPlayer.PlayEventArgs(src.Tiff.Current));
        }

        protected override void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            base.openToolStripMenuItem_Click(sender, e);
            this.Enabled = true;
        }
    }
}
