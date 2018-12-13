using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW2_video
{
    public class MyFilter
    {
        public enum BorderMethod { NULL, ZERO, NEAR };
        public delegate byte[] FilterCount(BitmapData data, int x, int y, BorderMethod borderMethod, MyFilter filter);

        /// <summary>
        /// member->
        /// </summary>

        int _size;
        public int size
        {
            get
            {
                return _size;
            }
        }
        double[,] data;

        /// <summary>
        /// function-->
        /// </summary>

        public MyFilter()
        {
            _size = 0;
            data = new double[size, size];
        }

        public MyFilter(int size)
        {
            _size = size;
            data = new double[size, size];
        }

        public MyFilter(int size, double value)
        {
            _size = size;
            data = new double[size, size];
            setData(value);
        }

        public double this[int x, int y]
        {
            get
            {
                return data[x, y];
            }
            set
            {
                data[x, y] = value;
            }
        }

        public void setData(double value)
        {// set all
            for (int j = 0; j < size; j++)
            {
                for (int i = 0; i < size; i++)
                {
                    try
                    {
                        data[i, j] = value;
                    }
                    catch
                    {
                        ;
                    }
                }
            }
        }

        public void setData(int startX, int startY, int endX, int endY, double value)
        {//set range
            for (int j = startY; j <= endY; j++)
            {
                for (int i = startX; i <= endX; i++)
                {
                    try
                    {
                        data[i, j] = value;
                    }
                    catch
                    {
                        ;
                    }
                }
            }
        }

        public static byte[] getPixel(BitmapData data, int x, int y, BorderMethod method)
        {
            byte[] output = new byte[3];
            switch (method)
            {
                case BorderMethod.NULL:
                    if (x < 0 || x >= data.Width || y < 0 || y >= data.Height)
                    {
                        return null;
                    }
                    else
                    {
                        unsafe
                        {
                            byte* target = (byte*)data.Scan0;
                            target += y * data.Stride + x * 3;
                            output[0] = target[0];
                            output[1] = target[1];
                            output[2] = target[2];
                        }
                        return output;
                    }
                case BorderMethod.ZERO:
                    if (x < 0 || x >= data.Width || y < 0 || y >= data.Height)
                    {
                        return new byte[3] { 0, 0, 0 };
                    }
                    else
                    {
                        unsafe
                        {
                            byte* target = (byte*)data.Scan0;
                            target += y * data.Stride + x * 3;
                            output[0] = target[0];
                            output[1] = target[1];
                            output[2] = target[2];
                        }
                        return output;
                    }
                case BorderMethod.NEAR:
                    if (x < 0)
                        x = 0;
                    else if (x >= data.Width)
                        x = data.Width - 1;
                    if (y < 0)
                        y = 0;
                    else if (y >= data.Height)
                        y = data.Height - 1;
                    unsafe
                    {
                        byte* target = (byte*)data.Scan0;
                        target += y * data.Stride + x * 3;
                        output[0] = target[0];
                        output[1] = target[1];
                        output[2] = target[2];
                    }
                    return output;
            }
            return output;
        }

        public static double[] getPixel(BitmapData data, int x, int y, BorderMethod method, double rate)
        {
            double[] output = new double[3];
            switch (method)
            {
                case BorderMethod.NULL:
                    if (x < 0 || x >= data.Width || y < 0 || y >= data.Height)
                    {
                        return null;
                    }
                    else
                    {
                        unsafe
                        {
                            byte* target = (byte*)data.Scan0;
                            target += y * data.Stride + x * 3;
                            output[0] = rate * target[0];
                            output[1] = rate * target[1];
                            output[2] = rate * target[2];
                        }
                        return output;
                    }
                case BorderMethod.ZERO:
                    if (x < 0 || x >= data.Width || y < 0 || y >= data.Height)
                    {
                        return new double[3] { 0, 0, 0 };
                    }
                    else
                    {
                        unsafe
                        {
                            byte* target = (byte*)data.Scan0;
                            target += y * data.Stride + x * 3;
                            output[0] = rate * target[0];
                            output[1] = rate * target[1];
                            output[2] = rate * target[2];
                        }
                        return output;
                    }
                case BorderMethod.NEAR:
                    if (x < 0)
                        x = 0;
                    else if (x >= data.Width)
                        x = data.Width - 1;
                    if (y < 0)
                        y = 0;
                    else if (y >= data.Height)
                        y = data.Height - 1;
                    unsafe
                    {
                        byte* target = (byte*)data.Scan0;
                        target += y * data.Stride + x * 3;
                        output[0] = rate * target[0];
                        output[1] = rate * target[1];
                        output[2] = rate * target[2];
                    }
                    return output;
            }
            return output;
        }

        /// <summary>
        /// val-->
        /// </summary>

        /// <summary>
        /// operate-->
        /// </summary>
        public static MyFilter add(MyFilter left, MyFilter right)
        {
            MyFilter newFilter = new MyFilter(left.size);
            for (int j = 0; j < newFilter.size; j++)
            {
                for (int i = 0; i < newFilter.size; i++)
                {
                    newFilter[i, j] = left[i, j] + right[i, j];
                }
            }
            return newFilter;
        }
    }
}
