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
        public Form_NormalPlayer() : base()
        {
            InitializeComponent();
        }

        protected override void initialForm()
        {
            base.initialForm();
            player.SideWorkDo.Add(new playCombine(lastView));
            player.SideWorkArgs.Add(new object[] { player, pictureBox2, null });
        }

        protected void lastView(MyPlayer src, Control dst, object[] args)
        {//play src player last view in dst player, args is not use
            PictureBox here = (PictureBox)dst;
            here.BackgroundImage = src.LastView;
        }
    }
}
