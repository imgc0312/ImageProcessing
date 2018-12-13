using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HW2_video
{
    public partial class Form_NormalPlayer : HW2_video.BasicForm
    {
        private MyTiff motionVector = null;//if have
        private MyPlayer motionPlayer = null; //if have

        public Form_NormalPlayer() : base()
        {
            InitializeComponent();
        }

        protected override void initialForm()
        {
            base.initialForm();
            normalToolStripMenuItem.Enabled = false;

            player.SideWorkDo.Add(new playCombine(lastView));
            player.SideWorkArgs.Add(new object[] { player, pictureBox2, null });
            motionPlayer = new MyPlayer(pictureBox3);
            player.SideWorkDo.Add(new playCombine(motionView));
            player.SideWorkArgs.Add(new object[] { player, null, new object[] { motionPlayer } });
            groupBox3.Text = "not use";
        }

        protected void lastView(MyPlayer src, Control dst, params object[] args)
        {//play src player last view in dst player, args is not use
            PictureBox here = (PictureBox)dst;
            here.BackgroundImage = src.LastView;
        }

        protected void motionView(MyPlayer src, Control dst, params object[] args)
        {//play motion view in (MyPlayer)args[0], dst is not use
            MyPlayer here = (MyPlayer)args[0];
            here.OnPlay(new MyPlayer.PlayEventArgs(src.Tiff.Current));
        }

        protected override void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            base.openToolStripMenuItem_Click(sender, e);
            if(myCompressTiff != null)
            {
                motionVector = new MyTiff();
                foreach(MyMotionTiff  motionTiff in myCompressTiff.motionTiff)
                {
                    motionVector.views.Add(MyMotionTiff.drawVector(motionTiff));
                }
                motionPlayer.open(motionVector);
                groupBox3.Text = "motion Vector";
            }
            else
            {
                motionVector = null;
                motionPlayer.Tiff = null;
                groupBox3.Text = "not use";
            }
            this.Enabled = true;
        }
    }
}
