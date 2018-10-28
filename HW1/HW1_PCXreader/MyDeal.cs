using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace HW1_PCXreader
{
    class MyDeal
    {
        public enum colorMode : int { R, G, B, GRAY};
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

        public static int threshold(int input, int bound, int output, int method)
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
    }
}
