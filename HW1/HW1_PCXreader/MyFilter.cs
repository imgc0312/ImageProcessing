using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW1_PCXreader
{
    public class MyFilter
    {
        public enum BorderMethod { NULL, ZERO, NEAR };
        public delegate byte[] FilterCount(BitmapData data, int x, int y, BorderMethod borderMethod, MyFilter filter);
        public class MyFilterData
        {
            private double[,][] _data;

            public MyFilterData()
            {
                _data = new double[0, 0][];
            }

            public double[] this[int x, int y]
            {
                get
                {
                    return _data[x, y];
                }
                set
                {
                    _data[x, y] = value;
                }
            }

            public int fill(BitmapData srx, int x, int y, BorderMethod borderMethod, MyFilter filter)
            {
                int size = filter.size;
                _data = new double[size, size][];
                int startX = x - size / 2;
                int startY = y - size / 2;
                int pixels = 0;
                for (int j = 0; j < size; j++)
                {
                    for (int i = 0; i < size; i++)
                    {
                        _data[i,j] = getPixel(srx, startX + i, startY + j, borderMethod, filter[i,j]);
                        if (_data[i, j] != null)
                            pixels++;
                    }
                }
                return pixels;
            }

            public byte[] count(double rate)
            {// sigma each pixel in _data (rate*pixel)
                byte[] output = new byte[3];
                double[] temp = new double[3];
                foreach(double[] pixel in _data)
                {
                    if(pixel != null)
                    {
                        temp[0] += pixel[0];
                        temp[1] += pixel[1];
                        temp[2] += pixel[2];
                    }
                }
                temp[0] *= rate;
                temp[1] *= rate;
                temp[2] *= rate;

                try
                {
                    output[0] = Convert.ToByte(temp[0]);
                }
                catch
                {
                    output[0] = 255;
                }
                try
                {
                    output[1] = Convert.ToByte(temp[1]);
                }
                catch
                {
                    output[1] = 255;
                }
                try
                {
                    output[2] = Convert.ToByte(temp[2]);
                }
                catch
                {
                    output[2] = 255;
                }
                return output;
            }
        }

        /// <summary>
        /// Var->
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
            data = new double[size , size];
        }

        public MyFilter(int size)
        {
            _size = size;
            data = new double[size , size];
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
            for(int j = startY; j < endY; j++)
            {
                for(int i = startX; i < endX; i++)
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

        private static byte[] getPixel(BitmapData data, int x, int y, BorderMethod method)
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
                        return new byte[3]{ 0, 0, 0};
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

        private static double[] getPixel(BitmapData data, int x, int y, BorderMethod method, double rate)
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

        public static byte[] blur(BitmapData data, int x, int y, BorderMethod borderMethod, MyFilter filter)
        {//this is a FilterCount, Mean Blur
            byte[] output = new byte[3];
            int size = filter.size;
            if(size <= 1)
            {
                return getPixel(data, x, y, borderMethod);
            }
            else
            {
                //int[] mean = new int[3];
                //byte[] temp = null;
                //int startX = x - size / 2;
                //int startY = y - size / 2;
                //int endX = startX + size;
                //int endY = startY + size;
                //int pixels = 0;
                //for (int j = startY; j < endY; j++)
                //{
                //    for (int i = startX; i < endX; i++)
                //    {
                //        temp = getPixel(data, i, j, borderMethod);
                //        if(temp != null)
                //        {
                //            mean[0] += temp[0];
                //            mean[1] += temp[1];
                //            mean[2] += temp[2];
                //            pixels++;
                //        }
                //    }
                //}
                MyFilterData kernel = new MyFilterData();
                int pixels = 0;
                filter.setData(1.0);
                pixels = kernel.fill(data, x, y, borderMethod, filter);
                if (pixels <= 0)
                    throw new DivideByZeroException("blur pixel size 0");
                return kernel.count(1.0 / pixels);
            }
        }
    }

}
