using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HW2_video
{
    public class MyPlayer
    {
        public bool flashIgnore = true;//ignore the play event args flash?
        /// <summary>
        /// define type
        /// </summary>
        public class PlayEventArgs : EventArgs //for progressbar
        {
            public PlayEventArgs(int value) : base()
            {
                this.value = value;
                this.state = PlayState.KEEP;
                this.args = null;
            }

            public PlayEventArgs(int value, PlayState state) : base()
            {
                this.value = value;
                this.state = state;
                this.args = null;
            }

            public PlayEventArgs(int value, PlayState state, params object[] args) : base()
            {
                this.value = value;
                this.state = state;
                this.args = args;
            }
            public int value { set; get; } // play frame number
            public PlayState state { set; get; } // chage play state
            public object[] args { set; get; } //other args ex: for filter Target (startX, startY, width, height)
        }

        public enum PlayState { PLAY, STOP, BACK, KEEP };
        public delegate void playHandler(MyTiff tiff, PlayEventArgs e);
        public delegate void threadHandler();

        /// <summary>
        /// control~
        /// </summary>
        protected PictureBox viewer = null;//view control
        protected TrackBar trackBar = null;//progress control
        protected threadHandler trackHandle;//handle progress control

        /// <summary>
        /// member
        /// </summary>
        public event playHandler playEventHandler;
        protected MyTiff tiff = new MyTiff();
        public MyTiff Tiff { get { return tiff; } set { tiff = value;  } }
        public Image LastView
        {
            get
            {
                if (viewer != null)
                    return viewer.BackgroundImage;
                return null;
            }
        }
        public Image PredictView
        {
            get
            {
                if (Tiff != null)
                    return Tiff.View;
                return null;
            }
        }
        public Image NextView
        {
            get
            {
                if (Tiff != null)
                    return Tiff.NextView;
                return null;
            }
        }
        protected PlayState curState = PlayState.STOP;
        public PlayState State
        {
            get
            {
                return curState;
            }
            set
            {
                switch (value)
                {
                    case PlayState.PLAY:
                        OnPlay();
                        break;
                    case PlayState.STOP:
                        OnStop();
                        break;
                }
            }
        }
        protected int _speed = 300;//ms
        public int Speed { get { return _speed; } }
        public List<Delegate> SideWorkDo = new List<Delegate>(0);
        public List<object[]> SideWorkArgs = new List<object[]>(0);
        Random random = new Random();// use for sleep
        /// <summary>
        /// function~
        /// </summary>

        public MyPlayer(PictureBox viewer)
        {
            this.viewer = viewer;
            playEventHandler = playChangeMethod;
            curState = PlayState.STOP;
            trackHandle = new threadHandler(this.progressTrack);
        }

        public void open(MyTiff tiff)
        {
            this.tiff = tiff;
            OnPlay(new PlayEventArgs(0, PlayState.KEEP));
        }

        public void connect(TrackBar trackBar)
        {
            this.trackBar = trackBar;
            this.trackBar.Maximum = this.tiff.Size - 1;
        }

        public void OnPlay()//start to play event
        {
            if (this.playEventHandler != null)
            {
                this.playEventHandler(this.tiff, new PlayEventArgs(tiff.Current, PlayState.PLAY));
            }
        }

        public void OnPlay(PlayEventArgs e)
        {
            if (this.playEventHandler != null)
            {
                this.playEventHandler(this.tiff, e);
            }
        }

        public void OnPlay(MyTiff tiff, PlayEventArgs e)
        {
            if (this.playEventHandler != null)
            {
                this.playEventHandler(tiff, e);
            }
        }

        public void OnPlay(Image view, PlayEventArgs e)
        {
            if (this.playEventHandler != null)
            {
                this.playEventHandler(new MyTiff(view), e);
            }
        }

        public void OnStop()//start to stop event
        {
            if (this.playEventHandler != null)
            {
                this.playEventHandler(this.tiff, new PlayEventArgs(tiff.Current, PlayState.STOP));
            }
        }

        public void OnBack()//start to go back event
        {
            if (this.playEventHandler != null)
            {
                this.playEventHandler(this.tiff, new PlayEventArgs(tiff.Current, PlayState.BACK));
            }
        }

        public void Next()//view next event
        {
            if (this.playEventHandler != null)
            {
                this.playEventHandler(this.tiff, new PlayEventArgs(tiff.Next, PlayState.KEEP));
            }
        }

        public void Keep()//view current event
        {
            if (this.playEventHandler != null)
            {
                this.playEventHandler(this.tiff, new PlayEventArgs(tiff.Current, PlayState.KEEP));
            }
        }

        public void Back()//view last event
        {
            if (this.playEventHandler != null)
            {
                this.playEventHandler(this.tiff, new PlayEventArgs(tiff.Back, PlayState.KEEP));
            }
        }

        public void Bound()//view current event && drawBound
        {
            if (this.playEventHandler != null)
            {
                this.playEventHandler(this.tiff, new PlayEventArgs(tiff.Current, PlayState.KEEP));
            }
        }

        protected virtual void playChangeMethod(MyTiff tiff, PlayEventArgs e)
        {
            this.tiff = tiff;
            if (tiff == null)
                return;
            if(e.value >= 0)
                tiff.Current = e.value;
            PlayState oldState = curState;
            if (trackBar != null)
            {
                trackBar.Invoke(trackHandle);
            }
            if (viewer != null)
            {
                switch (e.state)
                {
                    case PlayState.PLAY:
                        curState = PlayState.PLAY;
                        Keep();
                        //if (oldState == PlayState.STOP)
                        //    Next();
                        break;
                    case PlayState.STOP:
                        curState = PlayState.STOP;
                        //Stop();
                        break;
                    case PlayState.BACK:
                        curState = PlayState.BACK;
                        Keep();
                        //if (oldState == PlayState.STOP)
                        //    Back();
                        break;
                    case PlayState.KEEP:
                        sideWorking();
                        if (e.args == null)
                            draw();
                        else
                            draw((int)e.args[0], (int)e.args[1], (int)e.args[2], (int)e.args[3]);
                        switch (curState)
                        {
                            case PlayState.PLAY:
                                new Thread(new ThreadStart(new Action(() =>
                                {
                                    if (tiff.CurrState == 2)
                                        curState = PlayState.STOP;
                                    Thread.Sleep(Speed);
                                    if (curState == oldState)
                                        Next();
                                }))).Start();
                                break;
                            case PlayState.BACK:
                                new Thread(new ThreadStart(new Action(() =>
                                {
                                    if (tiff.CurrState == 0)
                                        curState = PlayState.STOP;
                                    Thread.Sleep(Speed);
                                    if (curState == oldState)
                                        Back();
                                }))).Start();
                                break;
                        }
                        break;
                }
            }
        }

        protected void progressTrack()
        {
            trackBar.Value = tiff.Current;
        }

        protected void sideWorking()
        {//do things beside playing in all
            if (SideWorkDo != null)
            {
                for (int i = 0; i < SideWorkDo.Count; i++)
                {
                    //Debug.Print("i" + i);
                    sideWorking(SideWorkDo.ElementAt(i), SideWorkArgs.ElementAt(i));
                }
            }
        }

        protected void sideWorking(Delegate work, params object[] args)
        {//do things beside playing 
            if (work != null)
            {
                //foreach (object arg in args)
                //{
                //    if (arg != null)
                //        Debug.Print(arg.ToString());
                //    else
                //        Debug.Print("<null>");
                //}
                work.DynamicInvoke(args);
            }
                
        }

        protected void draw()
        {
            if (viewer != null)
            {
                new Thread(new ThreadStart(new Action(() =>
                {
                    MyDeal.tryDrawBack(viewer, tiff.View, -1);
                    MyDeal.tryDraw(viewer, null);
                }))).Start();
            }
        }

        protected void draw(int X, int Y, int width, int height)
        {
            if ((viewer != null) && (!flashIgnore))
            {
                new Thread(new ThreadStart(new Action(() =>
                {
                    int startX = X - width / 2;
                    int startY = Y - height / 2;
                    Bitmap targetDraw;
                    int viewWidth;
                    int viewHeight;
                    MyDeal.tryDrawBack(viewer, tiff.View);
                    while (true)
                    {
                        try
                        {
                            viewWidth = tiff.View.Width;
                            viewHeight = tiff.View.Height;
                        }
                        catch (InvalidOperationException)
                        {
                            Thread.Sleep(23);
                            continue;
                        }
                        break;
                    }
                    targetDraw = new Bitmap(viewWidth, viewHeight);
                    int tx, ty;
                    for (int i = 0; i < width; i++)
                    {//draw top line
                        tx = startX + i;
                        ty = startY;
                        if (tx < 0)
                            continue;
                        if (tx >= targetDraw.Width)
                            continue;
                        if (ty < 0)
                            continue;
                        if (ty >= targetDraw.Height)
                            continue;
                        targetDraw.SetPixel(tx, ty, Color.Red);
                    }
                    for (int i = 0; i < width; i++)
                    {//draw bottom line
                        tx = startX + i;
                        ty = startY + height;
                        if (tx < 0)
                            continue;
                        if (tx >= targetDraw.Width)
                            continue;
                        if (ty < 0)
                            continue;
                        if (ty >= targetDraw.Height)
                            continue;
                        targetDraw.SetPixel(tx, ty, Color.Red);
                    }
                    for (int i = 0; i < height; i++)
                    {//draw left line
                        tx = startX;
                        ty = startY + i;
                        if (tx < 0)
                            continue;
                        if (tx >= targetDraw.Width)
                            continue;
                        if (ty < 0)
                            continue;
                        if (ty >= targetDraw.Height)
                            continue;
                        targetDraw.SetPixel(tx, ty, Color.Red);
                    }
                    for (int i = 0; i < height; i++)
                    {//draw right line
                        tx = startX + width;
                        ty = startY + i;
                        if (tx < 0)
                            continue;
                        if (tx >= targetDraw.Width)
                            continue;
                        if (ty < 0)
                            continue;
                        if (ty >= targetDraw.Height)
                            continue;
                        targetDraw.SetPixel(tx, ty, Color.Red);
                    }
                    MyDeal.tryDraw(viewer, targetDraw);
                    
                }))).Start();
            }
        }
    }
}
