using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace HW2_video
{
    public class MyCompresser
    {
        public delegate double findMatchDelegate(MyFilterData search, BitmapData refData, MyPlayer RefPlayer, ref int x, ref int y);
        private delegate void threadHandler();//for save
        private delegate void threadHandler2(int value);//for track
        private class matchRatePair
        {// a data pair use for findMatch
            public double rate;
            public int x;
            public int y;
            public matchRatePair(double rate, int x, int y)
            {
                this.rate = rate;
                this.x = x;
                this.y = y;
            }

            public static bool operator >(matchRatePair left, matchRatePair right)
            {
                return (left.rate > right.rate);
            }
            public static bool operator <(matchRatePair left, matchRatePair right)
            {
                return (left.rate < right.rate);
            }

            public static bool operator >(matchRatePair left, double rate)
            {
                return (left.rate > rate);
            }
            public static bool operator <(matchRatePair left, double rate)
            {
                return (left.rate < rate);
            }
        }

        MyPlayer RefPlayer;// reference frame viewer && compress tiff source
        public MyPlayer ReferencePlayer { get { return RefPlayer; } }
        MyPlayer CurPlayer;// courrent frame viewer
        public MyPlayer CurrentPlayer { get { return CurPlayer; } }
        MyCompressTiff compressFile = null;//encode file
        public static int compressKernelSize = 8; // compress Kernel Size
        public static int sleepTime = 0; // if too low the ref viewer would be error

        findMatchDelegate findMatch = null;
        PictureBox featureViewer = null;
        TrackBar trackBar = null;
        SaveFileDialog saver = null;
        CompressForm activeFrom = null;

        static MyFilter CompressKernel;

        public MyCompresser(MyPlayer RefPlayer, PictureBox CurViewer)
        {
            this.RefPlayer = RefPlayer;
            CurPlayer = new MyPlayer(CurViewer);
            compressFile = new MyCompressTiff();
            CompressKernel = new MyFilter(compressKernelSize);
            CompressKernel.setData(1.0);
        }

        public void setMatchMethod(findMatchDelegate findMatchWay)
        {
            findMatch = findMatchWay;
        }

        public void connect(PictureBox viewer)
        {
            featureViewer = viewer;
        }

        public void connect(TrackBar trackBar)
        {
            this.trackBar = trackBar;
            try
            {
                this.trackBar.Maximum = this.RefPlayer.Tiff.Size - 1;
            }
            catch (NullReferenceException)
            {
                ;
            }
            
        }

        public void connect(CompressForm activeFrom, SaveFileDialog saver)
        {
            this.activeFrom = activeFrom;
            this.saver = saver;
        }

        public void Compressing()
        {
            new Thread(new ThreadStart(new Action(() =>
            {
                try
                {
                    if (RefPlayer != null)
                    {
                        MyFilterData CurKernelGet = new MyFilterData();// the data copy from cur frame of kernel size
                        for (int i = 0; i < RefPlayer.Tiff.Size; i++)
                        //for (int i = 0; i < 3; i++)
                        {
                            //Debug.Print("in frame " + i);
                            trackBar.Invoke(new threadHandler2(progressTrack), i);

                            RefPlayer.OnPlay(new MyPlayer.PlayEventArgs(i - 1, MyPlayer.PlayState.KEEP));
                            CurPlayer.OnPlay(RefPlayer.NextView, new MyPlayer.PlayEventArgs(0));
                            if (i == 0)
                            {//uncompress frame number
                                compressFile.baseImg.Add((Image)RefPlayer.Tiff[i].Clone());// add ref imge
                                compressFile.motionTiff.Add(null);
                                continue;
                            }

                            Bitmap refBitmapCp;// the copy Bitmap for ref frame
                            Bitmap curBitmapCp;// the copy Bitmap for cur frame
                            while (true)
                            {
                                try
                                {
                                    refBitmapCp = new Bitmap((Image)RefPlayer.PredictView.Clone());// the copy Bitmap for ref frame
                                }
                                catch (InvalidOperationException)
                                {
                                    Thread.Sleep(33);
                                    continue;
                                }
                                break;
                            }
                            while (true)
                            {
                                try
                                {
                                    curBitmapCp = new Bitmap((Image)RefPlayer.NextView.Clone());// the copy Bitmap for cur frame
                                }
                                catch (InvalidOperationException)
                                {
                                    Thread.Sleep(33);
                                    continue;
                                }
                                break;
                            }
                            BitmapData refData = refBitmapCp.LockBits(MyDeal.boundB(refBitmapCp), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                            BitmapData curData = curBitmapCp.LockBits(MyDeal.boundB(curBitmapCp), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                            MyMotionTiff theMotion = new MyMotionTiff(curBitmapCp.Width, curBitmapCp.Height);
                            compressFile.baseImg.Add(null);
                            compressFile.motionTiff.Add(theMotion);
                            for (int y = 0 + compressKernelSize / 2; y < curBitmapCp.Height; y += compressKernelSize)
                            {
                                for (int x = 0 + compressKernelSize / 2; x < curBitmapCp.Width; x += compressKernelSize)
                                {
                                    CurPlayer.OnPlay(RefPlayer.NextView, new MyPlayer.PlayEventArgs(0, MyPlayer.PlayState.KEEP, x, y, compressKernelSize, compressKernelSize));
                                    CurKernelGet.fill(curData, x, y, MyFilter.BorderMethod.ZERO, CompressKernel);
                                    if (featureViewer != null)
                                        featureViewer.Image = CurKernelGet.transToBitmap();
                                    int targetX = x;
                                    int targetY = y;
                                    findMatchNormal(CurKernelGet, refData, RefPlayer, ref targetX, ref targetY);
                                    theMotion[x, y] = new int[] { targetX, targetY };
                                    //Debug.Print("in frame " + i + " in " + x + " , "+ y + "find target " + targetX + " , " + targetY);
                                }
                            }
                            refBitmapCp.UnlockBits(refData);
                            curBitmapCp.UnlockBits(curData);
                        }

                        if (activeFrom != null)
                            activeFrom.Invoke(new threadHandler(saveFile));// save the result
                    }
                }
                catch (InvalidOperationException e)
                {
                    Debug.Print(e.StackTrace);
                    Debug.Print(e.ToString());
                    return;
                }
                
            }))).Start();

        }

        public double findMatchNormal(MyFilterData search, BitmapData refData, MyPlayer RefPlayer, ref int targetX, ref int targetY)
        {
            matchRatePair dataRecord = new matchRatePair(Double.PositiveInfinity, 0, 0);
            MyFilterData refKernelGet = new MyFilterData();// the data copy from ref frame of kernel size
            int farDistance2 = 128;//define half of this is mean the distance value from target is far
            int nearDistance2 = 32;//define half of this is mean the distance value from target is near
            int farJumpStap = compressKernelSize;// define search pixel jump step lenth when seach in far area
                        
            if ((targetX < 0) || (targetY < 0))
            {// search all pixels
                
                for (int y = 0 + compressKernelSize / 2; y < refData.Height; y += 1)
                {
                    for (int x = 0 + compressKernelSize / 2; x < refData.Width; x += 1)
                    {
                        if(x%2 == 0)// reduce the reflash view frequence
                            RefPlayer.OnPlay(new MyPlayer.PlayEventArgs(-1, MyPlayer.PlayState.KEEP, x, y, compressKernelSize, compressKernelSize));
                        refKernelGet.fill(refData, x, y, MyFilter.BorderMethod.ZERO, CompressKernel);
                        double distance = MyFilterData.compare(search, refKernelGet);
                        if (dataRecord > distance)
                        {// find min distance
                            dataRecord.rate = distance;
                            dataRecord.x = x;
                            dataRecord.y = y;
                        }
                        Thread.Sleep(sleepTime);
                    }
                }
                targetX = dataRecord.x;
                targetY = dataRecord.y;
                return dataRecord.rate;
            }
            else
            {//local primary
                // the far area  left & top posion
                int fx = targetX - farDistance2 / 2;
                int fy = targetY - farDistance2 / 2;
                // the near area posion
                int nl = targetX - nearDistance2 / 2;
                int nt = targetY - nearDistance2 / 2;
                int nr = targetX + nearDistance2 / 2;
                int nb = targetY + nearDistance2 / 2;

                int jumpStap = farJumpStap;
                for (int y = (fy < (0 + compressKernelSize / 2) ? (0 + compressKernelSize / 2) : fy); (y < refData.Height) && (y < fy + farDistance2); y += jumpStap)
                {
                    for (int x = (fx < (0 + compressKernelSize / 2) ? (0 + compressKernelSize / 2) : fx); (x < refData.Width) && (x < fx + farDistance2); x += jumpStap)
                    {
                        if ((x >= nl) && (x < nr) && (y >= nt) && (y < nb))
                        {// in near
                            jumpStap = 1;
                        }
                        else
                        {//in far
                            jumpStap = farJumpStap;
                        }
                        //Debug.Print("match Test " + x + " , " + y);
                        RefPlayer.OnPlay(new MyPlayer.PlayEventArgs(-1, MyPlayer.PlayState.KEEP, x, y, compressKernelSize, compressKernelSize));
                        refKernelGet.fill(refData, x, y, MyFilter.BorderMethod.ZERO, CompressKernel);
                        double distance = MyFilterData.compare(search, refKernelGet);
                        if (dataRecord > distance)
                        {// find min distance
                            dataRecord.rate = distance;
                            dataRecord.x = x;
                            dataRecord.y = y;
                        }
                        Thread.Sleep(sleepTime);
                    }
                }
                targetX = dataRecord.x;
                targetY = dataRecord.y;
                return dataRecord.rate;
            }
        }

        protected void progressTrack(int value)
        {
            trackBar.Value = value;
        }

        private void saveFile()
        {
            if (saver != null)
            {//save compress file
                if (saver.ShowDialog() == DialogResult.OK)
                {
                    // If the file name is not an empty string open it for saving.  
                    if (saver.FileName != "")
                    {
                        FileStream fs = (FileStream)saver.OpenFile();
                        MyCompressTiff.writeToFile(fs, compressFile);
                        fs.Close();
                    }
                }
            }
            activeFrom.button1.Enabled = true;
        }

    }
}
