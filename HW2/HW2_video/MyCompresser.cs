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
        public enum CONNECT { PICTURE_MOTION, PICTURE_FEATURE, PICTURE_MATCH }// for connect form control
        public enum COMPRESS_METHOD { ALL, LOCAL, TSS, INTER_SUB, INTRA_SUB }// compress method
        
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
        
        public static int sleepShort = 0; // sleepTime short ver
        public static int sleepLong = 50; // sleepTime long ver
        public static int sleepTime = sleepShort; // if too low the ref viewer would be error

        COMPRESS_METHOD compressMethod = COMPRESS_METHOD.LOCAL;
        findMatchDelegate findMatch = null;
        MyFilterData.CRITERIA_METHOD criteria = MyFilterData.CRITERIA_METHOD.SQUARE;
        PictureBox MotionViewer = null;
        PictureBox featureViewer = null;
        PictureBox MatchViewer = null;
        TrackBar trackBar = null;
        SaveFileDialog saver = null;
        CompressForm activeFrom = null;
        Thread compressThread = null;

        static MyFilter CompressKernel;
        Random random = new Random();

        public MyCompresser(MyPlayer RefPlayer, PictureBox CurViewer)
        {
            this.RefPlayer = RefPlayer;
            CurPlayer = new MyPlayer(CurViewer);
            compressFile = new MyCompressTiff();
            CompressKernel = new MyFilter(compressKernelSize);
            CompressKernel.setData(1.0);
        }

        public void shutDown()
        {
            if (compressThread != null)
            {
                if(compressThread.IsAlive)
                    compressThread.Abort();
                compressThread = null;
            }
        }

        public void setCompressMethod(COMPRESS_METHOD method)
        {
            switch (method)
            {
                case COMPRESS_METHOD.ALL:
                    this.findMatch = findMatchAll;
                    Debug.Print("OOXX**in all");
                    break;
                case COMPRESS_METHOD.LOCAL:
                    this.findMatch = findMatchLocal;
                    Debug.Print("OOXX**in local");
                    break;
                case COMPRESS_METHOD.TSS:
                    this.findMatch = findMatchTSS;
                    Debug.Print("OOXX**in TSS");
                    break;
                case COMPRESS_METHOD.INTER_SUB:
                    Debug.Print("OOXX**in INTER_SUB");
                    break;
                case COMPRESS_METHOD.INTRA_SUB:
                    Debug.Print("OOXX**in INTRA_SUB");
                    break;
            }
            this.compressMethod = method;
        }

        //public void setMatchMethod(MATCH_METHOD method)
        //{
        //    switch (method)
        //    {
        //        case MATCH_METHOD.ALL:
        //            this.findMatch = findMatchAll;
        //            return;
        //        case MATCH_METHOD.LOCAL:
        //            this.findMatch = findMatchLocal;
        //            return;
        //    }
        //}

        public void setCriteriaMethod(MyFilterData.CRITERIA_METHOD method)
        {
            switch (method)
            {
                case MyFilterData.CRITERIA_METHOD.ABSOLUTE:
                    this.criteria = method;
                    return;
                case MyFilterData.CRITERIA_METHOD.SQUARE:
                    this.criteria = method;
                    return;
            }
        }

        public void connect(PictureBox viewer)
        {
            featureViewer = viewer;
        }

        public void connect(PictureBox viewer, CONNECT forWhat)
        {
            switch (forWhat)
            {
                case CONNECT.PICTURE_MOTION:
                    MotionViewer = viewer;
                    return;
                case CONNECT.PICTURE_FEATURE:
                    featureViewer = viewer;
                    return;
                case CONNECT.PICTURE_MATCH:
                    MatchViewer = viewer;
                    return;

            }
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
            compressThread = new Thread(new ThreadStart(new Action(() =>
            {
                try
                {
                    switch (compressMethod)
                    {
                        case COMPRESS_METHOD.INTRA_SUB:
                            intraSubsample();
                            break;
                        case COMPRESS_METHOD.INTER_SUB:
                            interSubsample();
                            break;
                        case COMPRESS_METHOD.ALL:
                        case COMPRESS_METHOD.LOCAL:
                        case COMPRESS_METHOD.TSS:
                            motionMethod();
                            break;
                    }
                }
                catch (InvalidOperationException e)
                {
                    Debug.Print(e.StackTrace);
                    Debug.Print(e.ToString());
                    return;
                }
                
            })));
            compressThread.IsBackground = true;
            compressThread.Start();

        }

        protected void intraSubsample()
        {
            compressFile.type = MyCompressTiffDefine.TYPE.SUBSAMPLE;
            if (RefPlayer != null)
            {
                RefPlayer.OnPlay(new MyPlayer.PlayEventArgs(0, MyPlayer.PlayState.KEEP));
                CurPlayer.Tiff = RefPlayer.Tiff.clone();
                MyFilterData CurKernelGet = new MyFilterData();// the data copy from cur frame of kernel size
                int intraSubSize = 2;
                int subSizeHalf = intraSubSize / 2;
                double subWeight = 1.0 / (intraSubSize * intraSubSize);
                MyFilter intraSubFilter = new MyFilter(intraSubSize);
                intraSubFilter.setData(1.0);

                for (int i = 0; i < RefPlayer.Tiff.Size; i++)
                {//run frames
                 //Debug.Print("in frame " + i);
                    trackBar.Invoke(new threadHandler2(progressTrack), i);// reflash progress view
                    CurPlayer.OnPlay(new MyPlayer.PlayEventArgs(i));//flash current frame view

                    Bitmap curBitmapCp;// the copy Bitmap for cur frame
                    Bitmap dstBitmap;// viewer in counting out view
                    while (true)
                    {
                        try
                        {
                            curBitmapCp = new Bitmap((Image)CurPlayer.PredictView.Clone());// the copy Bitmap for cur frame
                        }
                        catch (InvalidOperationException)
                        {
                            Thread.Sleep(33);
                            continue;
                        }
                        break;
                    }
                    dstBitmap = new Bitmap(curBitmapCp.Width, curBitmapCp.Height);
                    while (true)
                    {
                        try
                        {
                            MotionViewer.Image = dstBitmap;
                        }
                        catch (InvalidOperationException)
                        {
                            Thread.Sleep(33);
                            continue;
                        }
                        break;
                    }

                    BitmapData curData = curBitmapCp.LockBits(MyDeal.boundB(curBitmapCp), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                    //Bitmap afterSubView = new Bitmap(intraSubSize, intraSubSize);// view in match blok
                    //while (true)
                    //{
                    //    try
                    //    {
                    //        MatchViewer.Image = afterSubView;
                    //    }
                    //    catch (InvalidOperationException)
                    //    {
                    //        Thread.Sleep(33);
                    //        continue;
                    //    }
                    //    break;
                    //}
                    //Graphics afterSubViewG = Graphics.FromImage(afterSubView);
                    Graphics dstBitmapG = Graphics.FromImage(dstBitmap);
                    int freq = 0;
                    for (int y = 0 + subSizeHalf; y < curBitmapCp.Height; y += intraSubSize)
                    {
                        for (int x = 0 + subSizeHalf; x < curBitmapCp.Width; x += intraSubSize)
                        {
                            if (activeFrom != null)
                            {
                                if (activeFrom.IsDisposed)//form be dispose
                                    return;
                            }
                            //draw target
                            if (freq % 2 == 0)
                            { // reduce flash freq
                                freq = 0;
                                CurPlayer.OnPlay(new MyPlayer.PlayEventArgs(-1, MyPlayer.PlayState.KEEP, x, y, intraSubSize, intraSubSize));
                            }
                            else
                                freq++;
                            CurKernelGet.fill(curData, x, y, MyFilter.BorderMethod.ZERO, intraSubFilter);
                            //MyDeal.tryDraw(featureViewer, CurKernelGet.transToBitmap());
                            byte[] subSample = CurKernelGet.count(subWeight);
                            //Debug.Print("in " + x + " , " + y + " : " + subSample[2] + " " + subSample[1] + " " + subSample[0]);
                            Color sampleColor = Color.FromArgb(subSample[2], subSample[1], subSample[0]);
                            Brush sample = new SolidBrush(sampleColor);
                            //afterSubViewG.Clear(sampleColor);
                            dstBitmapG.FillRectangle(sample, x - subSizeHalf, y - subSizeHalf, intraSubSize, intraSubSize);
                            MotionViewer.Invalidate();
                            //MatchViewer.Invalidate();
                            Thread.Sleep(sleepTime);
                            //Debug.Print("in frame " + i + " in " + x + " , "+ y + "find target " + targetX + " , " + targetY);
                        }
                    }
                    curBitmapCp.UnlockBits(curData);
                    compressFile.baseImg.Add(dstBitmap);// add dst imge
                    compressFile.motionTiff.Add(null);
                }

                if (activeFrom != null)
                    activeFrom.Invoke(new threadHandler(saveFile));// save the result
            }
        }

        protected void interSubsample()
        {
            compressFile.type = MyCompressTiffDefine.TYPE.SUBSAMPLE;
            if (RefPlayer != null)
            {
                RefPlayer.OnPlay(new MyPlayer.PlayEventArgs(0, MyPlayer.PlayState.KEEP));
                CurPlayer.Tiff = RefPlayer.Tiff;
                MyFilterData CurKernelGet = new MyFilterData();// the data copy from cur frame of kernel size
                for (int i = 0; i < RefPlayer.Tiff.Size; i++)
                {//run frames
                 //Debug.Print("in frame " + i);
                    trackBar.Invoke(new threadHandler2(progressTrack), i);// reflash progress view
                    CurPlayer.OnPlay(new MyPlayer.PlayEventArgs(i));//flash current frame view

                    if ((i % 2 == 0) || (i == (RefPlayer.Tiff.Size - 1) ))
                    {//uncompress frame number
                        compressFile.baseImg.Add((Image)RefPlayer.Tiff[i].Clone());// add ref imge
                        compressFile.motionTiff.Add(null);
                        continue;
                    }
                    else
                    {
                        compressFile.baseImg.Add(null);// add ref imge
                        compressFile.motionTiff.Add(null);
                        continue;
                    }
                }

                if (activeFrom != null)
                    activeFrom.Invoke(new threadHandler(saveFile));// save the result
            }
        }

        protected void motionMethod()
        {
            compressFile.type = MyCompressTiffDefine.TYPE.MOTION;
            if (RefPlayer != null)
            {
                MyFilterData CurKernelGet = new MyFilterData();// the data copy from cur frame of kernel size
                for (int i = 0; i < RefPlayer.Tiff.Size; i++)
                {//run frames
                 //Debug.Print("in frame " + i);
                    trackBar.Invoke(new threadHandler2(progressTrack), i);// reflash progress view

                    RefPlayer.OnPlay(new MyPlayer.PlayEventArgs(i - 1, MyPlayer.PlayState.KEEP));//flash ref frame view
                    CurPlayer.OnPlay(RefPlayer.NextView, new MyPlayer.PlayEventArgs(0));//flash current frame view
                    
                    //clear motion view
                    Bitmap motionBlock = null;
                    Bitmap motionVector = null;
                    for(int t = 0; t < 5; t++)
                    {
                        try
                        {
                            motionBlock = new Bitmap(RefPlayer.NextView.Width, RefPlayer.NextView.Height);
                            motionVector = new Bitmap(RefPlayer.NextView.Width, RefPlayer.NextView.Height);
                        }
                        catch (InvalidOperationException)
                        {
                            Thread.Sleep(29);
                            continue;
                        }
                        break;
                    }
                    MyDeal.tryDraw(MotionViewer, motionVector);
                    MyDeal.tryDrawBack(MotionViewer, motionBlock);
                    Graphics graphicMotionBlock = Graphics.FromImage(motionBlock);
                    Graphics graphicMotionVector = Graphics.FromImage(motionVector);
                    int colorLowBound = 64;// for random color value lower bound
                    int colorHighBound = 256 - colorLowBound;
                    Color penColor = Color.Black;

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
                            if (activeFrom != null)
                            {
                                if (activeFrom.IsDisposed)//form be dispose
                                    return;
                            }
                            //draw target
                            CurPlayer.OnPlay(RefPlayer.NextView, new MyPlayer.PlayEventArgs(0, MyPlayer.PlayState.KEEP, x, y, compressKernelSize, compressKernelSize));
                            CurKernelGet.fill(curData, x, y, MyFilter.BorderMethod.ZERO, CompressKernel);
                            MyDeal.tryDraw(featureViewer, CurKernelGet.transToBitmap());
                            //draw motion
                            penColor = Color.FromArgb(colorLowBound + random.Next() % colorHighBound, colorLowBound + random.Next() % colorHighBound, colorLowBound + random.Next() % colorHighBound);
                            graphicMotionBlock.FillRectangle(new SolidBrush(penColor), x - MyCompresser.compressKernelSize / 4, y - MyCompresser.compressKernelSize / 4, MyCompresser.compressKernelSize / 2, MyCompresser.compressKernelSize / 2);
                            MotionViewer.Invalidate();
                            //find match
                            int targetX = x;
                            int targetY = y;
                            findMatch(CurKernelGet, refData, RefPlayer, ref targetX, ref targetY);
                            theMotion[x, y] = new int[] { targetX, targetY };
                            //draw match vector
                            graphicMotionVector.DrawLine(new Pen(Color.FromArgb(128, penColor), 2.0f), x, y, targetX, targetY);

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

        ///
        /// -> find Match 
        ///
        public double findMatchAll(MyFilterData search, BitmapData refData, MyPlayer RefPlayer, ref int targetX, ref int targetY)
        {// all seach
            targetX = -1;
            targetY = -1;
            return findMatchNormal(search, refData, RefPlayer,ref targetX, ref targetY);
        }

        public double findMatchLocal(MyFilterData search, BitmapData refData, MyPlayer RefPlayer, ref int targetX, ref int targetY)
        {// local primary 
            return findMatchNormal(search, refData, RefPlayer, ref targetX, ref targetY);
        }

        public double findMatchNormal(MyFilterData search, BitmapData refData, MyPlayer RefPlayer, ref int targetX, ref int targetY)
        {// 2D loorer methd if x,y < 0 ? all seach : local primary 
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
                        if (x % 3 == 0)// reduce the reflash view frequence
                            RefPlayer.OnPlay(new MyPlayer.PlayEventArgs(-1, MyPlayer.PlayState.KEEP, x, y, compressKernelSize, compressKernelSize));
                        refKernelGet.fill(refData, x, y, MyFilter.BorderMethod.ZERO, CompressKernel);
                        double distance = MyFilterData.compare(search, refKernelGet, criteria);
                        if (dataRecord > distance)
                        {// find min distance
                            dataRecord.rate = distance;
                            dataRecord.x = x;
                            dataRecord.y = y;
                            MyDeal.tryDraw(MatchViewer, refKernelGet.transToBitmap());
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
                        compareBlock(search, refData, RefPlayer, x, y, dataRecord);
                    }
                }
                targetX = dataRecord.x;
                targetY = dataRecord.y;
                return dataRecord.rate;
            }
        }

        public double findMatchTSS(MyFilterData search, BitmapData refData, MyPlayer RefPlayer, ref int targetX, ref int targetY)
        {// 3 steps searcj
            matchRatePair dataRecord = new matchRatePair(Double.PositiveInfinity, targetX, targetY);
            MyFilterData refKernelGet = new MyFilterData();// the data copy from ref frame of kernel size

            int x = targetX;
            int y = targetY;
            for (int stepLength = refData.Width / 4; stepLength > 0; stepLength /= 2)
            {
                //cc
                compareBlock(search, refData, RefPlayer, x, y, dataRecord);

                //lt
                compareBlock(search, refData, RefPlayer, x - stepLength, y - stepLength, dataRecord);
                //ct
                compareBlock(search, refData, RefPlayer, x, y - stepLength, dataRecord);
                //rt
                compareBlock(search, refData, RefPlayer, x + stepLength, y - stepLength, dataRecord);
                //lc
                compareBlock(search, refData, RefPlayer, x - stepLength, y, dataRecord);
                
                //rc
                compareBlock(search, refData, RefPlayer, x + stepLength, y, dataRecord);
                //ld
                compareBlock(search, refData, RefPlayer, x - stepLength, y + stepLength, dataRecord);
                //cd
                compareBlock(search, refData, RefPlayer, x, y + stepLength, dataRecord);
                //rd
                compareBlock(search, refData, RefPlayer, x + stepLength, y + stepLength, dataRecord);

                x = dataRecord.x;
                y = dataRecord.y;
            }
            targetX = dataRecord.x;
            targetY = dataRecord.y;
            return dataRecord.rate;
        }

        private void compareBlock(MyFilterData search, BitmapData refData, MyPlayer RefPlayer, int x, int y, matchRatePair dataRecord)
        {// compare the (x , y) similar rate && flash dataRecord & view
            RefPlayer.OnPlay(new MyPlayer.PlayEventArgs(-1, MyPlayer.PlayState.KEEP, x, y, compressKernelSize, compressKernelSize));
            MyFilterData refKernelGet = new MyFilterData();
            refKernelGet.fill(refData, x, y, MyFilter.BorderMethod.ZERO, CompressKernel);
            double distance = MyFilterData.compare(search, refKernelGet, criteria);
            if (dataRecord > distance)
            {// find min distance
                dataRecord.rate = distance;
                dataRecord.x = x;
                dataRecord.y = y;
                MyDeal.tryDraw(MatchViewer, refKernelGet.transToBitmap());
            }
            Thread.Sleep(sleepTime);
        }
    }
}
