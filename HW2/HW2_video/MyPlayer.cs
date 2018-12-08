using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HW2_video
{
    public class MyPlayer
    {
        /// <summary>
        /// define type
        /// </summary>
        public class PlayEventArgs : EventArgs //for progressbar
        {
            public PlayEventArgs(int value) : base()
            {
                this.value = value;
                this.state = PlayState.KEEP;
            }

            public PlayEventArgs(int value, PlayState state) : base()
            {
                this.value = value;
                this.state = state;
            }
            public int value { set; get; }
            public PlayState state { set; get; }
        }

        public enum PlayState { PLAY, STOP, BACK , KEEP };
        public delegate void playHandler(MyTiff tiff, PlayEventArgs e);
        public delegate void threadHandler();

        /// <summary>
        /// control~
        /// </summary>
        PictureBox viewer = null;//view control
        TrackBar trackBar = null;//progress control
        threadHandler trackHandle;//handle progress control

        /// <summary>
        /// member
        /// </summary>
        public event playHandler playEventHandler;
        MyTiff tiff = new MyTiff();
        public MyTiff Tiff { get { return tiff; } }
        public Image LastView { get
            {
                if (viewer != null)
                    return viewer.BackgroundImage;
                return null;
            } }
        public Image PredictView
        {
            get
            {
                if (Tiff != null)
                    return Tiff.View;
                return null;
            }
        }
        PlayState curState = PlayState.STOP;
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
        int _speed = 300;//ms
        public int Speed { get { return _speed; } }
        public List<Delegate> SideWorkDo = new List<Delegate>(0);
        public List<object[]> SideWorkArgs = new List<object[]>(0);
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

        public void OnPlay(MyTiff tiff,PlayEventArgs e)
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

        public void Stop()//view current event
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

        private void playChangeMethod(MyTiff tiff, PlayEventArgs e)
        {
            this.tiff = tiff;
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
                        Next();
                        //if (oldState == PlayState.STOP)
                        //    Next();
                        break;
                    case PlayState.STOP:
                        curState = PlayState.STOP;
                        //Stop();
                        break;
                    case PlayState.BACK:
                        curState = PlayState.BACK;
                        Back();
                        //if (oldState == PlayState.STOP)
                        //    Back();
                        break;
                    case PlayState.KEEP:
                        sideWorking();
                        viewer.BackgroundImage = tiff.View;
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

        private void progressTrack()
        {
            trackBar.Value = tiff.Current;
        }

        private void sideWorking()
        {//do things beside playing in all
            if (SideWorkDo != null)
            {
                for(int i = 0; i < SideWorkDo.Count; i++)
                {
                    sideWorking(SideWorkDo.ElementAt(i), SideWorkArgs.ElementAt(i));
                }
            }
        }

        private void sideWorking(Delegate work, params object[] args)
        {//do things beside playing 
            if(work != null)
                work.DynamicInvoke(args);
        }
    }
}
