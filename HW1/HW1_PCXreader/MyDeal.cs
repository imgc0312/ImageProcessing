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
        public enum valueMethod : int { Near, Linear }//取值法: 鄰近取直, 線性插值 
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

        public static double[] countFreqs(Bitmap view)//default to use R ch
        {
            return countFreqsR(view);
        }

        public static double[] countFreqs(Bitmap view , int color)// color please use the MyDeal.colorMode.*
        {
            switch (color)
            {
                case (int)colorMode.R:
                    return countFreqsR(view);
                case (int)colorMode.G:
                    return countFreqsG(view);
                case (int)colorMode.B:
                    return countFreqsB(view);
                default:
                    return countFreqsR(view);
            }
            
        }

        //public static double[] countFreqsT(Bitmap view, int ch)// R freqency
        //{
        //    if (ch < 0 || ch >= 3) {
        //        Debug.Print("countFreqsT(): wrong ch -> " + ch);
        //        return null;
        //    }
        //    FreqsRecord freqs = new FreqsRecord();
        //    for (int i = 0; i < 256; i++)
        //    {
        //        freqs.f[i] = 0;
        //    }
        //    try
        //    {
        //        MyMat viewMat = new MyMat(view);
        //        int size = view.Width * view.Height;
        //        viewMat.runAll(
        //            new DealBGR<FreqsRecord>(
        //                delegate (ref byte B, ref byte G, ref byte R, ref FreqsRecord records)
        //                {
        //                    records.f[R]++;
        //                }
        //            ), ref freqs);
        //        for (int i = 0; i < 256; i++)
        //        {
        //            freqs.f[i] /= size;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.Print(e.ToString());
        //    }
        //    return freqs.f;
        //}

        //public class FreqsRecord
        //{
        //    public double[] f ;
        //    public FreqsRecord()
        //    {
        //        f = new double[256];
        //        for (int i = 0; i < 256; i++)
        //        {
        //            f[i] = 0;
        //        }
        //    }
        //}

        public static double[] countFreqsR(Bitmap view)// R freqency
        {
            double[] freqs = new double[256];
            for (int i = 0; i < 256; i++)
            {
                freqs[i] = 0;
            }
            try
            {
                int size = view.Width * view.Height;
                for (int i = 0; i < view.Width; i++)
                {
                    for (int j = 0; j < view.Height; j++)
                    {
                        freqs[view.GetPixel(i, j).R] ++;
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
            return freqs;
        }

        public static double[] countFreqsG(Bitmap view)// G freqency
        {
            double[] freqs = new double[256];
            for (int i = 0; i < 256; i++)
            {
                freqs[i] = 0;
            }
            try
            {
                int size = view.Width * view.Height;
                for (int i = 0; i < view.Width; i++)
                {
                    for (int j = 0; j < view.Height; j++)
                    {
                        freqs[view.GetPixel(i, j).G]++;
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
            return freqs;
        }
        public static double[] countFreqsB(Bitmap view)// B freqency
        {
            double[] freqs = new double[256];
            for (int i = 0; i < 256; i++)
            {
                freqs[i] = 0;
            }
            try
            {
                int size = view.Width * view.Height;
                for (int i = 0; i < view.Width; i++)
                {
                    for (int j = 0; j < view.Height; j++)
                    {
                        freqs[view.GetPixel(i, j).B]++;
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
            return freqs;
        }

        public static Series buildSeries(Bitmap view, int mode)// mode please use the MyDeal.colorMode.*
        {
            if (view == null)
                return null;
            Series series = new Series();
            String Name = " ??";
            series.Color = Color.Pink;
            switch (mode)
            {
                case (int)colorMode.R:
                    Name = "R";
                    series.Color = Color.Red;
                    break;
                case (int)colorMode.G:
                    Name = "G";
                    series.Color = Color.Green;
                    break;
                case (int)colorMode.B:
                    Name = "B";
                    series.Color = Color.Blue;
                    break;
                case (int)colorMode.GRAY:
                    Name = "gray";
                    series.Color = Color.DarkGray;
                    break;
            }
            series.Name = Name;
            string[] xValue = Enumerable.Range(0, 256).ToArray().Select(x => x.ToString()).ToArray();
            double[] yValue;
            yValue = MyDeal.countFreqs(view, mode);
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

        public static Bitmap threshold(Bitmap src, int bound, int channel)
        {
            Bitmap dst;
            Color get,use;
            int value;
            int maxVal = 0;
            if (src == null)
                return null;
            dst = (Bitmap)src.Clone();
            switch (channel)
            {
                case 1:// R
                    for(int i = 0; i < dst.Width; i++)
                    {
                        for(int j = 0; j < dst.Height; j++)
                        {
                            get = dst.GetPixel(i,j);
                            value = threshold(get.R, bound, maxVal, 0);
                            use = Color.FromArgb(get.A, value, get.G, get.B);
                            dst.SetPixel(i, j, use);
                        }
                    }
                    break;
                case 2:// G
                    for (int i = 0; i < dst.Width; i++)
                    {
                        for (int j = 0; j < dst.Height; j++)
                        {
                            get = dst.GetPixel(i, j);
                            value = threshold(get.G, bound, maxVal, 0);
                            use = Color.FromArgb(get.A, get.R, value, get.B);
                            dst.SetPixel(i, j, use);
                        }
                    }
                    break;
                case 3:// B
                    for (int i = 0; i < dst.Width; i++)
                    {
                        for (int j = 0; j < dst.Height; j++)
                        {
                            get = dst.GetPixel(i, j);
                            value = threshold(get.B, bound, maxVal, 0);
                            use = Color.FromArgb(get.A, get.R, get.G, value);
                            dst.SetPixel(i, j, use);
                        }
                    }
                    break;
                default:
                    for (int i = 0; i < dst.Width; i++)
                    {
                        for (int j = 0; j < dst.Height; j++)
                        {
                            get = dst.GetPixel(i, j);
                            value = threshold(get.R, bound, maxVal, 0);
                            use = Color.FromArgb(get.A, value, value, value);
                            dst.SetPixel(i, j, use);
                        }
                    }
                    break;
            }
            return dst;
        }

        private static int threshold(int input, int bound, int output, int method)
        {
            switch (method)
            {
                default:
                    if (input < bound)
                        return output;
                    else
                        return input;
            }
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
