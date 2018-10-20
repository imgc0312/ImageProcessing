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
        public enum colorMode : int { R, G, B};
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
            Series series = new Series();
            String Name = " ??";
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
    }
}
