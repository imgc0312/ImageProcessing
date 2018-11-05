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
    class MyDeal
    {
        public static ProgressMonitor progress = new ProgressMonitor();
        public enum colorMode : int { R, G, B, GRAY};
        public enum valueMethod : int { Near, Linear}//取值法: 鄰近取直, 線性插值 
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
            double mA = 0;  // local A dis
            double mB = 0;  // local B dis
            double curVar = 0;  // current var
            double maxVar = 0;  //maximun var 
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
            return resize(src, rate, valueMethod.Near, null);
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
            BitmapData dstData = dst.LockBits(MyF.bound(dst), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            
            unsafe
            {
                int skipByte = dstData.Stride - dstData.Width * 3;
                byte* dstPtr = (byte*)(dstData.Scan0);
                byte[] use = null;
                for (int i = 0; i < newWidth; i++)
                {
                    for (int j = 0; j < newHeight; j++)
                    {
                        use = getPixel(srcData, i / rate, j / rate, method);
                        if(use != null)
                        {
                            dstPtr[0] = use[0];
                            dstPtr[1] = use[1];
                            dstPtr[2] = use[2];
                            dstPtr += 3;
                        }
                        else
                        {
                            Debug.Print("getPixel Empty");
                        }
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

        private static byte[] getPixel(BitmapData src, double x, double y, valueMethod method)
        {
            byte[] color = new byte[3];

            int tx = 0;
            int ty = 0;
            switch (method)
            {
                default://鄰近取值法 
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
                    break;
            }

            unsafe
            {
                byte* ptr = (byte*)(src.Scan0);
                ptr += ty * src.Stride;
                ptr += tx * 3;
                color[0] = ptr[0];
                color[1] = ptr[1];
                color[2] = ptr[2];
            }
            return color;

        }
        
    }

    /// <summary>
    /// other class-->
    /// </summary>

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
}
