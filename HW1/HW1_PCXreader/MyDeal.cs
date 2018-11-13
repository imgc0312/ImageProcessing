using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace HW1_PCXreader
{
    public class MyDeal
    {
        public static ProgressMonitor progress = new ProgressMonitor();
        public enum colorMode : int { R, G, B, GRAY};
        public enum valueMethod : int { Nearly, Linear}//取值法: 鄰近取直, 線性插值 
        public enum RotateMethod : int { posi, nega };
        public enum TransforMethod : int { Rotate, Mirror };

        public static void setBytesByBytes(ref byte[] target, byte[] srcBytes, int StartIndex, int Size)
        {
            target = new byte[Size];
            //IntPtr buffer = Marshal.AllocHGlobal(Size);
            //try
            //{
            //    Marshal.Copy(srcBytes, StartIndex, buffer, Size);
            //    Marshal.PtrToStructure(buffer, target);
            //}
            //finally
            //{
            //    Marshal.FreeHGlobal(buffer);
            //}
            Buffer.BlockCopy(srcBytes, StartIndex, target, 0, Size);
        }

        //public static void setTByBytes<T>(T target, byte[] srcBytes, int StartIndex)
        //{
        //    int Size = Marshal.SizeOf(target);
        //    IntPtr buffer = Marshal.AllocHGlobal(Size);
        //    try
        //    {
        //        Marshal.Copy(srcBytes, StartIndex, buffer, Size);
        //        Marshal.PtrToStructure(buffer, target);
        //    }
        //    finally
        //    {
        //        Marshal.FreeHGlobal(buffer);
        //    }
        //}

        public static void setTByBytes(ref byte target, byte[] srcBytes, int StartIndex)
        {
            if (StartIndex >= srcBytes.Length)
                throw new Exception("out of bound :srcBytes");
            target = srcBytes[StartIndex];
        }

        public static void setTByBytes(ref ushort target, byte[] srcBytes, int StartIndex)
        {
            target = BitConverter.ToUInt16(srcBytes, StartIndex);
        }

        public static double[] countFreqs(Bitmap view)//default to use all ch
        {
            return countFreqsT(view, colorMode.GRAY);
        }
        
        public static double[] countFreqsT(Bitmap view, colorMode channel)// freqency
        {
            double[] freqs = new double[256];
            BitmapData srcData = view.LockBits(MyF.bound(view), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            for (int i = 0; i < 256; i++)
            {
                freqs[i] = 0;
            }
            try
            {
                int skipByte = srcData.Stride - srcData.Width * 3;
                int size = srcData.Width * srcData.Height;
                int byteOffset = 0;
                switch (channel)
                {
                    case colorMode.R:
                        byteOffset = 2;
                        break;
                    case colorMode.G:
                        byteOffset = 1;
                        break;
                    case colorMode.B:
                        byteOffset = 0;
                        break;
                    case colorMode.GRAY:
                    default:
                        byteOffset = 0;
                        break;
                }
                unsafe
                {
                    byte* srcPtr = (byte*)(srcData.Scan0);

                    for (int i = 0; i < srcData.Width; i++)
                    {
                        for (int j = 0; j < srcData.Height; j++)
                        {
                            freqs[srcPtr[byteOffset]]++;
                            srcPtr += 3;
                        }
                        srcPtr += skipByte;
                    }
                }
                
                
                for (int i = 0; i < 256; i++)
                {
                    freqs[i] /= size;
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
            }
            view.UnlockBits(srcData);
            return freqs;
        }

        public static int[] countFreqsAll(Bitmap src)// use for OTSU
        {
            int[] freqs = new int[256 * 3];
            BitmapData srcData = src.LockBits(MyF.bound(src), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            for (int i = 0; i < 256 * 3; i++)
            {
                freqs[i] = 0;
            }
            try
            {
                int skipByte = srcData.Stride - srcData.Width * 3;
                int size = srcData.Width * srcData.Height;
                unsafe
                {
                    byte* srcPtr = (byte*)(srcData.Scan0);

                    for (int i = 0; i < srcData.Width; i++)
                    {
                        for (int j = 0; j < srcData.Height; j++)
                        {
                            freqs[ 0 + srcPtr[0]]++;
                            freqs[256 + srcPtr[1]]++;
                            freqs[512 + srcPtr[2]]++;
                            srcPtr += 3;
                        }
                        srcPtr += skipByte;
                    }
                }
                
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
            }
            src.UnlockBits(srcData);
            return freqs;
        }

        public static Series buildSeries(Bitmap view, colorMode mode)// mode please use the MyDeal.colorMode.*
        {
            if (view == null)
                return null;
            Series series = new Series();
            String Name = " ??";
            series.Color = Color.Pink;
            switch (mode)
            {
                case colorMode.R:
                    Name = "R";
                    series.Color = Color.Red;
                    break;
                case colorMode.G:
                    Name = "G";
                    series.Color = Color.Green;
                    break;
                case colorMode.B:
                    Name = "B";
                    series.Color = Color.Blue;
                    break;
                case colorMode.GRAY:
                    Name = "gray";
                    series.Color = Color.DarkGray;
                    break;
            }
            series.Name = Name;
            string[] xValue = Enumerable.Range(0, 256).ToArray().Select(x => x.ToString()).ToArray();
            double[] yValue;
            yValue = MyDeal.countFreqsT(view, mode);
            series.ChartType = SeriesChartType.Column;
            series.Points.DataBindXY(xValue, yValue);
            return series;
        }

        public static Bitmap negative(Bitmap src)
        {
            if (src == null)
                return null;
            Bitmap output = (Bitmap)src.Clone();
            for(int i = 0; i < output.Width ; i++)
            {
                for(int j = 0; j < output.Height; j++)
                {              
                    Color ori = output.GetPixel(i, j);
                    output.SetPixel(i, j, Color.FromArgb(255 - ori.R, 255 - ori.G, 255 - ori.B));
                }
            }
            return output;
        }

        public static Bitmap gray(Bitmap src)
        {
            if (src == null)
                return null;
            Bitmap output = (Bitmap)src.Clone();
            for (int i = 0; i < output.Width; i++)
            {
                for (int j = 0; j < output.Height; j++)
                {
                    Color ori = output.GetPixel(i, j);
                    int gray = (299 * ori.R + 587 * ori.G + 114 * ori.B + 500) / 1000;
                    output.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                }
            }
            return output;
        }

        public static Bitmap selectCh(Bitmap src, int selectColor)//select channel
        {
            if (src == null)
                return null;
            Bitmap output = (Bitmap)src.Clone();
            for (int i = 0; i < output.Width; i++)
            {
                for (int j = 0; j < output.Height; j++)
                {
                    Color ori = output.GetPixel(i, j);
                    int value = 0;
                    switch (selectColor)
                    {
                        case (int)colorMode.R:
                            value = ori.R;
                            break;
                        case (int)colorMode.B:
                            value = ori.B;
                            break;
                        case (int)colorMode.G:
                            value = ori.G;
                            break;
                    }
                    output.SetPixel(i, j, Color.FromArgb(value, value, value));
                }
            }
            return output;
        }

        public static Bitmap threshold(Bitmap src, int[] threshs)
        {
            Bitmap dst;
            byte maxVal = 0;
            if (src == null)
                return null;
            dst = (Bitmap)src.Clone();

            BitmapData srcData = src.LockBits(MyF.bound(src), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dst.LockBits(MyF.bound(dst), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            
            unsafe
            {
                int skipByte = dstData.Stride - dstData.Width * 3;
                byte* srcPtr = (byte*)(srcData.Scan0);
                byte* dstPtr = (byte*)(dstData.Scan0);
                for (int i = 0; i < dstData.Width; i++)
                {
                    for (int j = 0; j < dstData.Height; j++)
                    {
                        dstPtr[0] = threshold(srcPtr[0], threshs[0], maxVal, 0);
                        dstPtr[1] = threshold(srcPtr[1], threshs[1], maxVal, 0);
                        dstPtr[2] = threshold(srcPtr[2], threshs[2], maxVal, 0);
                        srcPtr += 3;
                        dstPtr += 3;
                    }
                    srcPtr += skipByte;
                    dstPtr += skipByte;
                }

            }
            src.UnlockBits(srcData);
            dst.UnlockBits(dstData);
            return dst;
        }

        public static Bitmap threshold(Bitmap src, int thresh, colorMode channel)
        {
            Bitmap dst;
            byte maxVal = 0;
            int byteOffset = 0;
            if (src == null)
                return null;
            dst = (Bitmap)src.Clone();

            BitmapData srcData = src.LockBits(MyF.bound(src), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dst.LockBits(MyF.bound(dst), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            switch (channel)
            {
                case colorMode.R:
                    byteOffset = 2;
                    break;
                case colorMode.G:
                    byteOffset = 1;
                    break;
                case colorMode.B:
                    byteOffset = 0;
                    break;
                case colorMode.GRAY:
                default:
                    byteOffset = -1;
                    break;
            }
            unsafe
            {
                int skipByte = dstData.Stride - dstData.Width * 3;
                byte* srcPtr = (byte*)(srcData.Scan0);
                byte* dstPtr = (byte*)(dstData.Scan0);
                for (int i = 0; i < dstData.Width; i++)
                {
                    for (int j = 0; j < dstData.Height; j++)
                    {
                        if(byteOffset < 0)
                        {
                            dstPtr[0] = threshold(srcPtr[0], thresh, maxVal, 0);
                            dstPtr[1] = threshold(srcPtr[1], thresh, maxVal, 0);
                            dstPtr[2] = threshold(srcPtr[2], thresh, maxVal, 0);
                        }
                        else
                        {
                            dstPtr[byteOffset] = threshold(srcPtr[byteOffset], thresh, maxVal, 0);
                        }
                        srcPtr += 3;
                        dstPtr += 3;
                    }
                    srcPtr += skipByte;
                    dstPtr += skipByte;
                }

            }
            src.UnlockBits(srcData);
            dst.UnlockBits(dstData);
            return dst;
        }

        private static byte threshold(byte input, int thresh, byte maxVal, int method)
        {
            switch (method)
            {
                default:
                    if (input < thresh)
                        return maxVal;
                    else
                        return input;
            }
        }

        public static int[] OTSU(Bitmap src)
        {
            int[] threshs = new int[3];
            int[] allFreqs = countFreqsAll(src);
            int pixSize = src.Width * src.Height;
            threshs[0] = OTSU(ref allFreqs, 0, pixSize);
            threshs[1] = OTSU(ref allFreqs, 256, pixSize);
            threshs[2] = OTSU(ref allFreqs, 512, pixSize);
            return threshs;
        }

        private static int OTSU(ref int[] histogram, int start, int pixSize)
        {
            int answer = 0;
            int sum = 0;//all weight sum
            int wA = 0; // local A size
            int wB = 0; // local B size
            int sA = 0; // local A weight sum
            double mA = 0;  // local A Average
            double mB = 0;  // local B Average
            double curVar = 0;  // current Variance
            double maxVar = 0;  //maximun Variance 
            int thresh1 = 0;  // Lower thresh
            int thresh2 = 0;  // Higher thresh
            for (int index = 1; index < 256; index++)
            {
                sum += index * histogram[start + index];
            }
            for (int index = 0; index < 256; index++)
            {
                wA += histogram[start + index];
                wB = pixSize - wA;
                if ((wA == 0) || (wB == 0))
                    continue;
                sA += index * histogram[start + index];
                mA = (double)sA / wA;
                mB = (double)(sum - sA) / wB;
                curVar = wA * wB * (mA - mB) * (mA - mB);
                if(curVar >= maxVar)
                {
                    thresh1 = index;
                    if (curVar > maxVar)
                        thresh2 = index;
                    maxVar = curVar;
                }
            }
            answer = Convert.ToInt32((double)(thresh1 + thresh2)/2);
            return answer;
        }

        public static Bitmap resize(Bitmap src, double rate)
        {
            return resize(src, rate, valueMethod.Nearly, null);
        }

        public static Bitmap resize(Bitmap src, double rate, valueMethod method, ProgressMonitor monitor)
        {
            
            if (src == null)
                return null;
            if (monitor == null)
                monitor = progress;
            monitor.start(); // start view progress

            int newWidth = Convert.ToInt32(src.Width * rate);
            int newHeight = Convert.ToInt32(src.Height * rate);
            
            int progressCurrent = 0;
            int progressEnd = newWidth * newHeight;

            Bitmap dst = new Bitmap(newWidth, newHeight, src.PixelFormat);

            BitmapData srcData = src.LockBits(MyF.bound(src), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dst.LockBits(MyF.bound(dst), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            
            unsafe
            {
                int skipByte = dstData.Stride - dstData.Width * 3;
                byte* dstPtr = (byte*)(dstData.Scan0);
                byte[] use = null;
                for (int j = 0; j < newHeight; j++)
                {
                    for (int i = 0; i < newWidth; i++)
                    {
                        use = getPixel(srcData, i / rate, j / rate, method);
                        if(use != null)
                        {
                            dstPtr[0] = use[0];
                            dstPtr[1] = use[1];
                            dstPtr[2] = use[2];
                        }
                        else
                        {
                            Debug.Print("getPixel Empty");
                        }
                        dstPtr += 3;
                        progressCurrent++;
                        monitor.OnValueChanged(new ValueEventArgs() { value = (double)progressCurrent / progressEnd });
                    }
                    dstPtr += skipByte;
                }

            }
            src.UnlockBits(srcData);
            dst.UnlockBits(dstData);
            monitor.fine();
            return dst;
        }

        public static Bitmap rotate(Bitmap src, int angle, RotateMethod method)
        {
            switch (method)
            {
                case RotateMethod.posi:
                    return rotate_positive(src, angle, null);
                case RotateMethod.nega:
                default:
                    return rotate_negative(src, angle, null);
            }
        }

        public static Bitmap rotate(Bitmap src, int angle, RotateMethod method, ProgressMonitor monitor)
        {
            switch (method)
            {
                case RotateMethod.posi:
                    return rotate_positive(src, angle, monitor);
                case RotateMethod.nega:
                default:
                    return rotate_negative(src, angle, monitor);
            }
        }

        public static Bitmap rotate_positive(Bitmap src, int angle, ProgressMonitor monitor)
        {

            if (src == null)
                return null;
            if (monitor == null)
                monitor = progress;
            monitor.start(); // start view progress

            int width = src.Width;
            int height = src.Height;
            int newWidth = width;
            int newHeight = height;
            double[] oriCentroid = { width / 2, height / 2 };
            linearTransfor(ref newWidth, ref newHeight, angle, TransforMethod.Rotate);//count new width & height
            double[] Centroid = { newWidth / 2, newHeight / 2 };

            int progressCurrent = 0;
            int progressEnd = width * height;

            Bitmap dst = new Bitmap(newWidth, newHeight, src.PixelFormat);

            BitmapData srcData = src.LockBits(MyF.bound(src), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dst.LockBits(MyF.bound(dst), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            
            unsafe
            {
                
                int skipByte = srcData.Stride - srcData.Width * 3;
                byte* srcPtr = (byte*)(srcData.Scan0);
                byte[] use = new byte[3];
                MyMat rotateMat = MyMat.ROTATE(angle);
                MyMat baseMat = new MyMat(2, 1);
                MyMat dirMat;
                for (int j = 0; j < height; j++)
                {
                    baseMat[1, 0] = (double)j - oriCentroid[1];
                    for (int i = 0; i < width; i++)
                    {
                        baseMat[0, 0] = (double)i - oriCentroid[0];

                        use[0] = srcPtr[0];
                        use[1] = srcPtr[1];
                        use[2] = srcPtr[2];
                        dirMat = MyMat.mul(rotateMat, baseMat);
                        
                        if (dirMat.rows != 2)
                        {
                            Debug.Print("dirMat.rows != 2" + dirMat.rows);
                            break;
                        }
                        setPixel(dstData, dirMat[0, 0] + Centroid[0], dirMat[1, 0] + Centroid[1], use);

                        srcPtr += 3;
                        progressCurrent++;
                        monitor.OnValueChanged(new ValueEventArgs() { value = (double)progressCurrent / progressEnd });
                    }
                    srcPtr += skipByte;
                }

            }
            src.UnlockBits(srcData);
            dst.UnlockBits(dstData);
            monitor.fine();
            return dst;
        }

        public static Bitmap rotate_negative(Bitmap src, int angle, ProgressMonitor monitor)
        {
            return linearTransfor(src, angle, TransforMethod.Rotate, monitor);
        }

        public static Bitmap mirror(Bitmap src, int angle)
        {
            return linearTransfor(src, angle, TransforMethod.Mirror, null);
        }

        public static Bitmap mirror(Bitmap src, int angle, ProgressMonitor monitor)
        {
            return linearTransfor(src, angle, TransforMethod.Mirror, monitor);
        }

        public static Bitmap linearTransfor(Bitmap src, int angle, TransforMethod method, ProgressMonitor monitor)
        {

            if (src == null)
                return null;
            if (monitor == null)
                monitor = progress;
            monitor.start(); // start view progress

            int width = src.Width;
            int height = src.Height;
            double[] oriCentroid = { width / 2, height / 2 };
            linearTransfor(ref width, ref height, angle, method);//count new width & height
            double[] Centroid = { width / 2, height / 2 };

            int progressCurrent = 0;
            int progressEnd = width * height;

            Bitmap dst = new Bitmap(width, height, src.PixelFormat);

            BitmapData srcData = src.LockBits(MyF.bound(src), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dst.LockBits(MyF.bound(dst), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {

                int skipByte = dstData.Stride - dstData.Width * 3;
                byte* dstPtr = (byte*)(dstData.Scan0);
                byte[] use = null;
                MyMat tranforMat = null;
                switch (method)
                {
                    case TransforMethod.Mirror:
                        tranforMat = MyMat.MIRROR(-1 * angle);
                        break;
                    case TransforMethod.Rotate:
                    default:
                        tranforMat = MyMat.ROTATE(-1 * angle);
                        break;
                }
                MyMat baseMat = new MyMat(2, 1);
                MyMat dirMat;
                for (int j = 0; j < height; j++)
                {
                    baseMat[1, 0] = (double)j - Centroid[1];
                    for (int i = 0; i < width; i++)
                    {
                        baseMat[0, 0] = (double)i - Centroid[0];
                        dirMat = MyMat.mul(tranforMat, baseMat);

                        use = getPixel(srcData, dirMat[0, 0] + oriCentroid[0], dirMat[1, 0] + oriCentroid[1], valueMethod.Nearly);
                        dstPtr[0] = use[0];
                        dstPtr[1] = use[1];
                        dstPtr[2] = use[2];

                        dstPtr += 3;
                        progressCurrent++;
                        monitor.OnValueChanged(new ValueEventArgs() { value = (double)progressCurrent / progressEnd });
                    }
                    dstPtr += skipByte;
                }

            }
            src.UnlockBits(srcData);
            dst.UnlockBits(dstData);
            monitor.fine();
            return dst;
        }

        public static void linearTransfor(ref int width, ref int height, int angle, TransforMethod method)
        {
            MyMat tranforMat = null;
            MyMat baseMat = new MyMat(2, 1);
            MyMat dirMat1 = null;
            MyMat dirMat2 = null;
            double tempWidth = 0.0, tempHeight = 0.0;
            switch (method)
            {
                case TransforMethod.Mirror:
                    tranforMat = MyMat.MIRROR(angle);
                    break;
                case TransforMethod.Rotate:
                default:
                    tranforMat = MyMat.ROTATE(angle);
                    break;
            }
            baseMat[0, 0] = (double)width / 2;
            baseMat[1, 0] = (double)height / 2;
            dirMat1 = MyMat.mul(tranforMat, baseMat);
            baseMat[1, 0] = -baseMat[1, 0];
            dirMat2 = MyMat.mul(tranforMat, baseMat);
            tempWidth = 2 * Math.Max(Math.Abs(dirMat1[0, 0]), Math.Abs(dirMat2[0, 0]));
            tempHeight = 2 * Math.Max(Math.Abs(dirMat1[1, 0]), Math.Abs(dirMat2[1, 0]));
            width = Convert.ToInt32(tempWidth);
            height = Convert.ToInt32(tempHeight);
        }

        public static Bitmap opacity(Bitmap fore, Bitmap back, int rate)
        {
            Bitmap dst = null;
            int newWidth = 0;
            int newHeight = 0;
            if (fore == null)
                return back;
            if (back == null)
                return fore;
            newWidth = Math.Max(fore.Width, back.Width);
            newHeight = Math.Max(fore.Height, back.Height);
            dst = new Bitmap(newWidth, newHeight, fore.PixelFormat);
            BitmapData foreData = fore.LockBits(MyF.bound(fore), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData backData = back.LockBits(MyF.bound(back), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dstData = dst.LockBits(MyF.bound(dst), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            unsafe
            {
                int skipByte = dstData.Stride - dstData.Width * 3;
                byte* dstPtr = (byte*)(dstData.Scan0);
                byte[] useFore = null;
                byte[] useBack = null;
                for (int j = 0; j < newHeight; j++)
                {
                    for (int i = 0; i < newWidth; i++)
                    {

                        useFore = getPixel(foreData, i, j, valueMethod.Nearly);
                        useBack = getPixel(backData, i, j, valueMethod.Nearly);
                        dstPtr[0] = Convert.ToByte(((int)useFore[0] * rate + (int)useBack[0] * (100 - rate)) / 100);
                        dstPtr[1] = Convert.ToByte(((int)useFore[1] * rate + (int)useBack[1] * (100 - rate)) / 100);
                        dstPtr[2] = Convert.ToByte(((int)useFore[2] * rate + (int)useBack[2] * (100 - rate)) / 100);

                        dstPtr += 3;
                    }
                    dstPtr += skipByte;
                }

            }
            fore.UnlockBits(foreData);
            back.UnlockBits(backData);
            dst.UnlockBits(dstData);
            return dst;
        }

        public static Bitmap stretch(Bitmap src, StretchOption[] opts)
        {
            Bitmap dst;
            if (src == null)
                return null;
            dst = (Bitmap)src.Clone();
            BitmapData dstData = dst.LockBits(MyF.bound(dst), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            
            unsafe
            {
                int skipByte = dstData.Stride - dstData.Width * 3;
                byte* dstPtr = (byte*)(dstData.Scan0);
                for (int i = 0; i < dstData.Width; i++)
                {
                    for (int j = 0; j < dstData.Height; j++)
                    {
                        foreach(StretchOption opt in opts)
                        {
                            if((opt.CH == colorMode.B) || (opt.CH == colorMode.GRAY))
                            {
                                if (opt.inRange(dstPtr[0]))
                                    dstPtr[0] = Convert.ToByte(opt.map(dstPtr[0]));
                            }
                            if ((opt.CH == colorMode.G) || (opt.CH == colorMode.GRAY))
                            {
                                if (opt.inRange(dstPtr[1]))
                                    dstPtr[1] = Convert.ToByte(opt.map(dstPtr[1]));
                            }
                            if ((opt.CH == colorMode.R) || (opt.CH == colorMode.GRAY))
                            {
                                if (opt.inRange(dstPtr[2]))
                                    dstPtr[2] = Convert.ToByte(opt.map(dstPtr[2]));
                            }
                        }
                        dstPtr += 3;
                    }
                    dstPtr += skipByte;
                }

            }
            dst.UnlockBits(dstData);
            return dst;
        }

        ///
        /// -->pixel method
        ///
        private static byte[] getPixel(BitmapData src, double x, double y, valueMethod method)
        {
            byte[] color = new byte[3] { 0,0,0};
            if ((x < -0.5) || (x > (src.Width -0.5)) || (y < -0.5) || (y > (src.Height -0.5)))//out of bound
                return color;
            switch (method)
            {
                case valueMethod.Linear:
                    double rw = Math.Ceiling(x) - x; // right color weight
                    double lw = 1.0 - rw; // left color weight
                    double uw = Math.Ceiling(y) - y; // upper color weight
                    double dw = 1.0 - uw; // downer color weight
                    int lx = Convert.ToInt32(Math.Floor(x));    //left x
                    int rx = Convert.ToInt32(Math.Ceiling(x));  //right x
                    int ly = Convert.ToInt32(Math.Floor(y));    //downer y
                    int ry = Convert.ToInt32(Math.Ceiling(y));  //upper y

                    if (lx < 0)
                        lx = 0;
                    else if (lx >= src.Width)
                        lx = src.Width - 1;
                    if (ly < 0)
                        ly = 0;
                    else if (ly >= src.Height)
                        ly = src.Height - 1;
                    if (rx < 0)
                        rx = 0;
                    else if (rx >= src.Width)
                        rx = src.Width - 1;
                    if (ry < 0)
                        ry = 0;
                    else if (ry >= src.Height)
                        ry = src.Height - 1;

                    double[] tColor = new double[3];
                    unsafe
                    {
                        byte* ptr = (byte*)(src.Scan0);
                        int offset = 0;
                        // pix 00
                        offset = ly * src.Stride + lx * 3;
                        tColor[0] = ptr[offset + 0] * lw * dw;
                        tColor[1] = ptr[offset + 1] * lw * dw;
                        tColor[2] = ptr[offset + 2] * lw * dw;
                        //pix 01
                        offset += (rx - lx) * 3;
                        tColor[0] += ptr[offset + 0] * rw * dw;
                        tColor[1] += ptr[offset + 1] * rw * dw;
                        tColor[2] += ptr[offset + 2] * rw * dw;
                        // pix 10
                        offset = ry * src.Stride + lx * 3;
                        tColor[0] += ptr[offset + 0] * lw * uw;
                        tColor[1] += ptr[offset + 1] * lw * uw;
                        tColor[2] += ptr[offset + 2] * lw * uw;
                        //pix 11
                        offset += (rx - lx) * 3;
                        tColor[0] += ptr[offset + 0] * rw * uw;
                        tColor[1] += ptr[offset + 1] * rw * uw;
                        tColor[2] += ptr[offset + 2] * rw * uw;
                    }
                    color[0] = Convert.ToByte(tColor[0]);
                    color[1] = Convert.ToByte(tColor[1]);
                    color[2] = Convert.ToByte(tColor[2]);
                    break;
                    //////////////////
                case valueMethod.Nearly:
                default://鄰近取值法 
                    int tx = 0;
                    int ty = 0;
                    tx = Convert.ToInt32(x);
                    ty = Convert.ToInt32(y);
                    if (tx < 0)
                        tx = 0;
                    else if (tx >= src.Width)
                        tx = src.Width - 1;
                    if (ty < 0)
                        ty = 0;
                    else if (ty >= src.Height)
                        ty = src.Height - 1;

                    unsafe
                    {
                        byte* ptr = (byte*)(src.Scan0);
                        ptr += ty * src.Stride;
                        ptr += tx * 3;
                        color[0] = ptr[0];
                        color[1] = ptr[1];
                        color[2] = ptr[2];
                    }
                    break;
            }

            return color;
        }

        private static bool setPixel(BitmapData dst, double x, double y, byte[] color)
        {
            if(color.Length != 3)
            {
                Debug.Print("setPixel : color not 3byte");
                return false;
            }
            int tx = 0;
            int ty = 0;
            tx = Convert.ToInt32(x);
            ty = Convert.ToInt32(y);
            if ((tx < 0) || (tx >= dst.Width) || (ty < 0) || (ty >= dst.Height))
                return false;

            unsafe
            {
                byte* ptr = (byte*)(dst.Scan0);
                ptr += ty * dst.Stride;
                ptr += tx * 3;
                ptr[0] = color[0];
                ptr[1] = color[1];
                ptr[2] = color[2];
            }
            return true;
        }
    }




    /// <summary>
    /// other class-->
    /// </summary>

    // for progress
    public class ValueEventArgs : EventArgs //for progressbar
    {
        public double value { set; get; }
        public int percent { get { return Convert.ToInt32(value * 100); } }
    }

    public delegate void ValueChangedEventHandler(object sender, ValueEventArgs e);

    public class ProgressMonitor
    {
        private event ValueChangedEventHandler ValueChanged; //change event
        public double current = 0;
        public ProgressBar view { get; set; }
        //int tryTime = 0;
        public ProgressMonitor()
        {
            ValueChanged = new ValueChangedEventHandler(ValueChangeMethod);
            view = null;
        }

        public ProgressMonitor(ProgressBar view)
        {
            ValueChanged = new ValueChangedEventHandler(ValueChangeMethod);
            this.view = view;
        }

        public void start()
        {
            current = 0;
            if (view != null)
            {
                view.Value = 0;
                view.Visible = true;
                //Debug.Print("progress : start" + tryTime);
            }          
        }

        public void fine()
        {
            current = 1.0;
            if (view != null)
            {
                view.Value = 100;
                view.Visible = false;
                //Debug.Print("progress : fine" + tryTime);
                //tryTime++;
            }  
        }

        public void OnValueChanged(ValueEventArgs e)
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, e);
            }
        }

        public void ValueChangeMethod(object sender, ValueEventArgs e)
        {
            current = e.value;
            if (view != null)
            {
                view.Value = e.percent;
                //Debug.Print("progress : " + view.Value);
            }
            //Debug.Print("progress : change");
        }
    }

    // for stretch
    public class StretchOption
    {
        private int start;
        private int end;
        private double rate;
        private double bias;
        public MyDeal.colorMode CH;

        public StretchOption()
        {
            start = 0;
            end = 255;
            rate = 1.0;
            bias = 0.0;
            CH = MyDeal.colorMode.GRAY;
        }

        public StretchOption(int start, int end, double rate, double bias)
        {
            set(start, end, rate, bias);
        }

        public StretchOption(int start, int end, double rate, double bias, MyDeal.colorMode CH)
        {
            set(start, end, rate, bias, CH);
        }

        public StretchOption(int x1, int y1, int x2, int y2)
        {
            set(x1, y1, x2, y2);
        }

        public StretchOption(int x1, int y1, int x2, int y2, MyDeal.colorMode CH)
        {
            set(x1, y1, x2, y2, CH);
        }

        public void set(int start, int end, double rate, double bias)
        {
            set(start, end, rate, bias, MyDeal.colorMode.GRAY);
        }

        public void set(int start, int end, double rate, double bias, MyDeal.colorMode CH)
        {
            this.CH = CH;
            this.rate = rate;
            if (end > 255)
                this.end = 255;
            else
                this.end = end;
            if (start < 0)
            {
                this.start = 0;
                this.bias = bias + rate * (this.start - start);
            }
            else
            {
                this.start = start;
                this.bias = bias;
            }
            Debug.Print("start " + this.start + "," + this.end + " , rate " + this.rate + " , " + this.bias);
        }

        public void set(int x1, int y1, int x2, int y2)
        {
            set(x1, y1, x2, y2, MyDeal.colorMode.GRAY);
        }

        public void set(int x1, int y1, int x2, int y2, MyDeal.colorMode CH)
        {
            int theBase = x2 - x1;
            if (theBase == 0)
            {
                if(x1 == 255)
                    set(x1, x2, 0.0, (double)y1, CH);
                else
                    set(x1, x2, 0.0, (double)y2, CH);
            }
            else
            {
                set(x1, x2, ((double)y2 - y1) / theBase, (double)y1, CH);
            }
        }

        public bool inRange(int index)
        {
            return ((index >= start) && (index <= end));
        }

        public int map(int index)
        {
            int output = Convert.ToInt32(rate * (index - start) + bias);
            //Debug.Print(" " + index + " => " + output + " func : "  + start + ","+ rate + "," + bias);
            if (output < 0)
                output = 0;
            else if (output > 255)
                output = 255;
            return output;
        }
    } 
}
