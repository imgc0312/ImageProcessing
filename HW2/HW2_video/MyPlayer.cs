using System;
using System.Collections.Generic;
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
        PlayState curState = PlayState.STOP;
        int speed = 400;//ms

        /// <summary>
        /// function~
        /// </summary>
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
            OnPlay(new PlayEventArgs(0, PlayState.STOP));
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
            if(trackBar != null)
            {
                trackBar.Invoke(trackHandle);
            }
            if (viewer != null)
            {
                tiff.Current = e.value;
                PlayState oldState = curState;
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
                        Stop();
                        break;
                    case PlayState.BACK:
                        curState = PlayState.BACK;
                        Back();
                        //if (oldState == PlayState.STOP)
                        //    Back();
                        break;
                    case PlayState.KEEP:
                        viewer.Image = tiff.View;
                        switch (curState)
                        {
                            case PlayState.PLAY:
                                new Thread(new ThreadStart(new Action(() =>
                                {
                                    if (tiff.CurrState == 2)
                                        curState = PlayState.STOP;
                                    Thread.Sleep(speed);
                                    if (curState == oldState)
                                        Next();
                                }))).Start();
                                break;
                            case PlayState.BACK:
                                new Thread(new ThreadStart(new Action(() =>
                                {
                                    if (tiff.CurrState == 0)
                                        curState = PlayState.STOP;
                                    Thread.Sleep(speed);
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
    }
}
